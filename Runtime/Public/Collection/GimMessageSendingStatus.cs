namespace Gamania.GIMChat
{
    /// <summary>
    /// Represents the lifecycle state of a message being sent.
    /// </summary>
    public enum GimMessageSendingStatus
    {
        /// <summary>Not applicable / unknown state</summary>
        None = 0,

        /// <summary>Message is queued locally, waiting for server ACK</summary>
        Pending = 1,

        /// <summary>Server confirmed the message was received</summary>
        Succeeded = 2,

        /// <summary>Send failed; message is in the failed list</summary>
        Failed = 3,

        /// <summary>Send was canceled by the application</summary>
        Canceled = 4,
    }
}
