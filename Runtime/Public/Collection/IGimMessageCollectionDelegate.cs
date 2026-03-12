using System.Collections.Generic;

namespace Gamania.GIMChat
{
    /// <summary>
    /// Receives real-time and cache-driven events from a <see cref="GimMessageCollection"/>.
    /// All callbacks are dispatched on the Unity main thread via MainThreadDispatcher.
    ///
    /// Usage:
    /// <code>
    /// collection.Delegate = myHandler;   // weak-referenced internally
    /// </code>
    /// </summary>
    public interface IGimMessageCollectionDelegate
    {
        // ── Message CRUD ────────────────────────────────────────────────────────

        /// <summary>
        /// One or more messages were added to the collection.
        /// Sources: new message received, pagination fill, changelog sync,
        ///          or a pending message being created locally.
        /// </summary>
        void OnMessagesAdded(
            GimMessageCollection collection,
            GimMessageContext context,
            GimGroupChannel channel,
            IReadOnlyList<GimBaseMessage> addedMessages);

        /// <summary>
        /// One or more messages were updated in place.
        /// Sources: streaming update, pending → succeeded transition,
        ///          or changelog sync with server edits.
        /// </summary>
        void OnMessagesUpdated(
            GimMessageCollection collection,
            GimMessageContext context,
            GimGroupChannel channel,
            IReadOnlyList<GimBaseMessage> updatedMessages);

        /// <summary>
        /// One or more messages were removed from the collection.
        /// Sources: explicit delete event or changelog sync.
        /// </summary>
        void OnMessagesDeleted(
            GimMessageCollection collection,
            GimMessageContext context,
            GimGroupChannel channel,
            IReadOnlyList<GimBaseMessage> deletedMessages);

        // ── Channel state ───────────────────────────────────────────────────────

        /// <summary>
        /// The channel this collection belongs to was updated (name, metadata, etc.).
        /// </summary>
        void OnChannelUpdated(
            GimMessageCollection collection,
            GimMessageContext context,
            GimGroupChannel updatedChannel);

        /// <summary>
        /// The channel this collection belongs to was deleted.
        /// The collection should be disposed after this callback.
        /// </summary>
        void OnChannelDeleted(
            GimMessageCollection collection,
            GimMessageContext context,
            string deletedChannelUrl);

        // ── Sync gap ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Called when background sync detects a large gap in the message timeline.
        /// The app should dispose this collection and create a new one.
        /// </summary>
        void OnHugeGapDetected(GimMessageCollection collection);
    }
}
