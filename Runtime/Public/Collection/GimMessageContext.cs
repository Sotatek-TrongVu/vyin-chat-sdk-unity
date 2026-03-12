namespace Gamania.GIMChat
{
    /// <summary>
    /// Context object passed with every <see cref="IGimMessageCollectionDelegate"/> callback.
    /// Provides the event origin and the sending status at the time of the callback,
    /// so the UI layer can make informed rendering decisions.
    /// </summary>
    public sealed class GimMessageContext
    {
        /// <summary>Origin of the event that triggered this callback</summary>
        public GimCollectionEventSource Source { get; }

        /// <summary>Sending status of the affected message(s) at callback time</summary>
        public GimMessageSendingStatus SendingStatus { get; }

        /// <summary>
        /// True when the event came from a real-time WebSocket push.
        /// UI can use this to decide whether to animate the insertion/update.
        /// </summary>
        public bool IsFromEvent => Source.IsFromEvent();

        public GimMessageContext(GimCollectionEventSource source, GimMessageSendingStatus sendingStatus)
        {
            Source = source;
            SendingStatus = sendingStatus;
        }
    }
}
