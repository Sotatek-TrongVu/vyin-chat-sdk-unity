namespace Gamania.GIMChat
{
    /// <summary>
    /// Context object passed with every <see cref="IGimGroupChannelCollectionDelegate"/> callback.
    /// Provides the event origin so the UI layer can decide whether to animate the update.
    /// </summary>
    public sealed class GimChannelContext
    {
        /// <summary>Origin of the event that triggered this callback</summary>
        public GimCollectionEventSource Source { get; }

        /// <summary>
        /// True when the event came from a real-time WebSocket push.
        /// UI can use this to decide whether to animate the update.
        /// </summary>
        public bool IsFromEvent => Source.IsFromEvent();

        public GimChannelContext(GimCollectionEventSource source)
        {
            Source = source;
        }
    }
}
