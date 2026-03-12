// Runtime/Internal/Domain/Collections/SortedMessageList.cs
// Sorted message list data structure, sorted by CreatedAt timestamp in ascending order.
// Used for in-memory message cache in GimMessageCollection.

using System;
using System.Collections.Generic;

namespace Gamania.GIMChat.Internal.Domain.Collections
{
    /// <summary>
    /// Non-thread-safe sorted message list, sorted by CreatedAt in ascending order.
    /// Caller is responsible for multi-threading synchronization.
    ///
    /// Core invariants:
    /// - Messages are always sorted by <see cref="GimBaseMessage.CreatedAt"/> in ascending order
    /// - MessageId uniqueness: same MessageId replaces existing item
    /// - ReqId uniqueness (pending messages): used for pending → succeeded state transition
    /// </summary>
    internal sealed class SortedMessageList
    {
        // Primary storage, sorted by CreatedAt. Same timestamps preserve insertion order
        private readonly List<GimBaseMessage> _messages = new();

        // Fast lookup: MessageId → list index (rebuilt after changes)
        private readonly Dictionary<long, int> _idIndex = new();

        // ReqId → index, for pending → succeeded transitions
        private readonly Dictionary<string, int> _reqIdIndex = new();

        // ══════════════════════════════════════════════════════════════════════════════
        // Read Operations
        // ══════════════════════════════════════════════════════════════════════════════

        /// <summary>Number of messages in the list.</summary>
        public int Count => _messages.Count;

        /// <summary>Oldest (earliest) message, returns null if empty.</summary>
        public GimBaseMessage OldestMessage => _messages.Count > 0 ? _messages[0] : null;

        /// <summary>Latest (most recent) message, returns null if empty.</summary>
        public GimBaseMessage LatestMessage => _messages.Count > 0 ? _messages[_messages.Count - 1] : null;

        /// <summary>
        /// Returns a read-only snapshot of all messages, sorted by time.
        /// </summary>
        public IReadOnlyList<GimBaseMessage> ToReadOnlyList()
            => _messages.AsReadOnly();

        /// <summary>Checks if the specified message id exists.</summary>
        public bool Contains(long messageId)
            => _idIndex.ContainsKey(messageId);

        /// <summary>
        /// Counts messages with CreatedAt before the specified timestamp.
        /// </summary>
        /// <param name="timestamp">Unix millisecond timestamp.</param>
        /// <param name="inclusive">If true, includes messages with CreatedAt == timestamp.</param>
        public int GetCountBefore(long timestamp, bool inclusive = false)
        {
            var count = 0;
            foreach (var msg in _messages)
            {
                if (inclusive ? msg.CreatedAt <= timestamp : msg.CreatedAt < timestamp)
                    count++;
                else
                    break; // List is sorted, no need to continue
            }
            return count;
        }

        /// <summary>
        /// Counts messages with CreatedAt after the specified timestamp.
        /// </summary>
        /// <param name="timestamp">Unix millisecond timestamp.</param>
        /// <param name="inclusive">If true, includes messages with CreatedAt == timestamp.</param>
        public int GetCountAfter(long timestamp, bool inclusive = false)
        {
            var count = 0;
            for (var i = _messages.Count - 1; i >= 0; i--)
            {
                if (inclusive ? _messages[i].CreatedAt >= timestamp : _messages[i].CreatedAt > timestamp)
                    count++;
                else
                    break; // List is sorted, no need to continue
            }
            return count;
        }

        /// <summary>Tries to get a message by server-assigned id.</summary>
        public bool TryGet(long messageId, out GimBaseMessage message)
        {
            if (_idIndex.TryGetValue(messageId, out var index))
            {
                message = _messages[index];
                return true;
            }
            message = null;
            return false;
        }

        // ══════════════════════════════════════════════════════════════════════════════
        // Write Operations
        // ══════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Inserts a new message while maintaining time order.
        ///
        /// Deduplication rules:
        /// - Succeeded messages (MessageId > 0): deduplicated by MessageId.
        ///   Inserting same MessageId replaces existing message.
        /// - Pending messages (MessageId == 0): not deduplicated by MessageId.
        ///   Each pending message is independent, tracked only by ReqId.
        ///   If ReqId exists, replaces in place.
        /// </summary>
        public void Insert(GimBaseMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            // Only deduplicate succeeded messages by MessageId
            if (message.MessageId > 0 && _idIndex.ContainsKey(message.MessageId))
            {
                // Replace in place (same position; succeeded messages keep timestamp)
                var idx = _idIndex[message.MessageId];
                if (!string.IsNullOrEmpty(_messages[idx].ReqId))
                    _reqIdIndex.Remove(_messages[idx].ReqId);

                _messages[idx] = message;
                RebuildIndexEntry(message, idx);
                return;
            }

            // Deduplicate pending messages by ReqId (MessageId == 0)
            if (message.MessageId == 0 && !string.IsNullOrEmpty(message.ReqId)
                && _reqIdIndex.ContainsKey(message.ReqId))
            {
                var idx = _reqIdIndex[message.ReqId];
                _messages[idx] = message;
                // No need to update _reqIdIndex; index is the same
                return;
            }

            // New item: binary search to find insertion position by CreatedAt
            var insertAt = FindInsertionIndex(message.CreatedAt);
            _messages.Insert(insertAt, message);
            RebuildFullIndex();
        }

        /// <summary>
        /// Transitions a pending message (identified by <paramref name="reqId"/>)
        /// to succeeded message using server-returned <paramref name="serverMessage"/>.
        /// Pending message is replaced in place, maintaining sort order.
        /// Returns true if found and transitioned successfully.
        /// </summary>
        public bool TransitionPendingToSucceeded(string reqId, GimBaseMessage serverMessage)
        {
            if (string.IsNullOrEmpty(reqId)) return false;
            if (serverMessage == null) throw new ArgumentNullException(nameof(serverMessage));

            if (!_reqIdIndex.TryGetValue(reqId, out var idx)) return false;

            // Remove old indexes
            var old = _messages[idx];
            _idIndex.Remove(old.MessageId);
            _reqIdIndex.Remove(reqId);

            // Replace with server message
            _messages[idx] = serverMessage;
            RebuildFullIndex();
            return true;
        }

        /// <summary>
        /// Removes a message by server id.
        /// Returns true if successfully removed.
        /// </summary>
        public bool Remove(long messageId)
        {
            if (!_idIndex.TryGetValue(messageId, out var idx)) return false;

            var msg = _messages[idx];
            if (!string.IsNullOrEmpty(msg.ReqId))
                _reqIdIndex.Remove(msg.ReqId);

            _messages.RemoveAt(idx);
            RebuildFullIndex();
            return true;
        }

        /// <summary>Removes all messages.</summary>
        public void Clear()
        {
            _messages.Clear();
            _idIndex.Clear();
            _reqIdIndex.Clear();
        }

        /// <summary>
        /// Inserts multiple messages, skipping duplicates (succeeded by MessageId, pending by ReqId).
        /// Returns the list of actually inserted messages (non-duplicates).
        /// </summary>
        public IReadOnlyList<GimBaseMessage> InsertAllIfNotExist(IEnumerable<GimBaseMessage> messages)
        {
            if (messages == null) return new List<GimBaseMessage>().AsReadOnly();

            var inserted = new List<GimBaseMessage>();
            foreach (var msg in messages)
            {
                if (msg == null) continue;

                // Skip if exists (succeeded by MessageId)
                if (msg.MessageId > 0 && _idIndex.ContainsKey(msg.MessageId))
                    continue;

                // Skip if exists (pending by ReqId)
                if (msg.MessageId == 0 && !string.IsNullOrEmpty(msg.ReqId) && _reqIdIndex.ContainsKey(msg.ReqId))
                    continue;

                Insert(msg);
                inserted.Add(msg);
            }
            return inserted.AsReadOnly();
        }

        // ══════════════════════════════════════════════════════════════════════════════
        // Helper Methods
        // ══════════════════════════════════════════════════════════════════════════════

        /// <summary>Binary search to find insertion position by createdAt.</summary>
        private int FindInsertionIndex(long createdAt)
        {
            int lo = 0, hi = _messages.Count;
            while (lo < hi)
            {
                var mid = (lo + hi) / 2;
                if (_messages[mid].CreatedAt <= createdAt)
                    lo = mid + 1;
                else
                    hi = mid;
            }
            return lo;
        }

        /// <summary>Rebuilds the full index.</summary>
        private void RebuildFullIndex()
        {
            _idIndex.Clear();
            _reqIdIndex.Clear();
            for (var i = 0; i < _messages.Count; i++)
                RebuildIndexEntry(_messages[i], i);
        }

        /// <summary>Rebuilds index entry for a single message.</summary>
        private void RebuildIndexEntry(GimBaseMessage message, int index)
        {
            // Only create MessageId index for succeeded messages (MessageId > 0)
            // Pending messages (MessageId == 0) should not collide in this index
            if (message.MessageId > 0)
                _idIndex[message.MessageId] = index;

            if (!string.IsNullOrEmpty(message.ReqId))
                _reqIdIndex[message.ReqId] = index;
        }
    }
}
