namespace Gamania.GIMChat
{
    /// <summary>
    /// Identifies the origin of a collection delegate callback.
    /// Allows the UI layer to decide whether to animate the update.
    /// Values 0–99 are local/cache sources; 100+ are real-time WebSocket events.
    /// </summary>
    public enum GimCollectionEventSource
    {
        // ── Local / Cache sources (0–99) ──────────────────────────────
        /// <summary>Channel list updated via delta-sync changelog</summary>
        ChannelChangelog = 0,

        /// <summary>Message list updated via delta-sync changelog</summary>
        MessageChangelog = 1,

        /// <summary>Messages added during pagination fill</summary>
        MessageFill = 2,

        /// <summary>A local pending message was created before server ACK</summary>
        LocalMessagePendingCreated = 10,

        /// <summary>A local message failed to send</summary>
        LocalMessageFailed = 11,

        /// <summary>A local message was canceled</summary>
        LocalMessageCanceled = 12,

        // ── Real-time WebSocket event sources (100+) ──────────────────
        /// <summary>A message was successfully sent (server ACK / pending → succeeded)</summary>
        EventMessageSent = 100,

        /// <summary>A new message was received via real-time event</summary>
        EventMessageReceived = 101,

        /// <summary>A message was updated via real-time event</summary>
        EventMessageUpdated = 102,

        /// <summary>A message was deleted</summary>
        EventMessageDeleted = 103,

        /// <summary>Channel metadata changed</summary>
        EventChannelChanged = 200,

        /// <summary>Channel was deleted</summary>
        EventChannelDeleted = 201,

        /// <summary>Channel member count changed</summary>
        EventMemberCountChanged = 202,

        /// <summary>A user joined the channel</summary>
        EventUserJoined = 300,

        /// <summary>A user left the channel</summary>
        EventUserLeft = 301,
    }

    /// <summary>
    /// Extension methods for <see cref="GimCollectionEventSource"/>.
    /// </summary>
    public static class GimCollectionEventSourceExtensions
    {
        /// <summary>
        /// Returns true when the event originated from a real-time WebSocket push
        /// (value >= 100), false for local cache or changelog sources.
        /// UI layer can use this to decide whether to animate the update.
        /// </summary>
        public static bool IsFromEvent(this GimCollectionEventSource source)
            => (int)source >= 100;
    }
}
