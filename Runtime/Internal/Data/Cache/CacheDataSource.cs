using System.Collections.Generic;
using System.Linq;

namespace Gamania.GIMChat.Internal.Data.Cache
{
    /// <summary>
    /// Shared in-memory cache singleton.
    /// Currently implements pending/failed message queues only.
    /// TODO: expand to channel cache, message cache, user cache, state transitions, local DB sync.
    /// </summary>
    public class CacheDataSource
    {
        public static CacheDataSource Instance { get; } = new CacheDataSource();

        private readonly Dictionary<string, List<GimBaseMessage>> _pending = new();
        private readonly Dictionary<string, List<GimBaseMessage>> _failed = new();
        private readonly Dictionary<string, GimGroupChannel> _channels = new();

        // ── Pending messages ──────────────────────────────────────────────────────

        /// <summary>
        /// Returns all pending messages for the given channel.
        /// </summary>
        public IReadOnlyList<GimBaseMessage> GetPendingMessages(string channelUrl)
        {
            return _pending.TryGetValue(channelUrl, out var list)
                ? list.AsReadOnly()
                : new List<GimBaseMessage>().AsReadOnly();
        }

        /// <summary>
        /// Stores a pending message (MessageId == 0, ReqId set) for its channel.
        /// </summary>
        public void AddPendingMessage(GimBaseMessage message)
        {
            if (!_pending.TryGetValue(message.ChannelUrl, out var list))
            {
                list = new List<GimBaseMessage>();
                _pending[message.ChannelUrl] = list;
            }
            list.Add(message);
        }

        /// <summary>
        /// Removes the pending message with the given reqId from the channel.
        /// No-op if not found.
        /// </summary>
        public void RemovePendingMessage(string reqId, string channelUrl)
        {
            if (!_pending.TryGetValue(channelUrl, out var list)) return;
            list.RemoveAll(m => m.ReqId == reqId);
        }

        // ── Failed messages ───────────────────────────────────────────────────────

        /// <summary>
        /// Returns all failed messages for the given channel.
        /// </summary>
        public IReadOnlyList<GimBaseMessage> GetFailedMessages(string channelUrl)
        {
            return _failed.TryGetValue(channelUrl, out var list)
                ? list.AsReadOnly()
                : new List<GimBaseMessage>().AsReadOnly();
        }

        /// <summary>
        /// Stores a failed message for its channel.
        /// </summary>
        public void AddFailedMessage(GimBaseMessage message)
        {
            if (!_failed.TryGetValue(message.ChannelUrl, out var list))
            {
                list = new List<GimBaseMessage>();
                _failed[message.ChannelUrl] = list;
            }
            list.Add(message);
        }

        /// <summary>
        /// Removes the specified messages from the channel's failed list.
        /// Matches by MessageId.
        /// </summary>
        public void DeleteFailedMessages(IEnumerable<GimBaseMessage> messages, string channelUrl)
        {
            if (!_failed.TryGetValue(channelUrl, out var list)) return;
            var ids = new HashSet<long>(messages.Select(m => m.MessageId));
            list.RemoveAll(m => ids.Contains(m.MessageId));
        }

        /// <summary>
        /// Removes all failed messages for the given channel.
        /// </summary>
        public void DeleteAllFailedMessages(string channelUrl)
        {
            _failed.Remove(channelUrl);
        }

        // ── Channel cache (for GroupChannelCollection) ────────────────────────────

        /// <summary>
        /// Adds or updates a channel in the shared cache.
        /// Called when LoadMore returns channels or when real-time events update a channel.
        /// </summary>
        public void SetChannel(GimGroupChannel channel)
        {
            if (channel == null || string.IsNullOrEmpty(channel.ChannelUrl)) return;
            _channels[channel.ChannelUrl] = channel;
        }

        /// <summary>
        /// Adds or updates multiple channels in the shared cache.
        /// </summary>
        public void SetChannels(IEnumerable<GimGroupChannel> channels)
        {
            if (channels == null) return;
            foreach (var ch in channels)
                SetChannel(ch);
        }

        /// <summary>
        /// Returns channels from cache for the given URIs, in the order of uris.
        /// Missing channels are skipped.
        /// </summary>
        public IReadOnlyList<GimGroupChannel> GetGroupChannelsFromCache(IEnumerable<string> uris)
        {
            if (uris == null) return new List<GimGroupChannel>().AsReadOnly();
            var result = new List<GimGroupChannel>();
            foreach (var url in uris)
            {
                if (string.IsNullOrEmpty(url)) continue;
                if (_channels.TryGetValue(url, out var ch))
                    result.Add(ch);
            }
            return result.AsReadOnly();
        }

        /// <summary>
        /// Removes a channel from cache (e.g., when channel is deleted).
        /// </summary>
        public void RemoveChannel(string channelUrl)
        {
            if (!string.IsNullOrEmpty(channelUrl))
                _channels.Remove(channelUrl);
        }

        // ── Reset ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Clears all in-memory data. Intended for testing or full session reset.
        /// </summary>
        public void Clear()
        {
            _pending.Clear();
            _failed.Clear();
            _channels.Clear();
        }
    }
}
