namespace Gamania.GIMChat
{
    /// <summary>
    /// Parameters for creating a user message.
    /// Used when sending messages to a channel.
    /// </summary>
    public class GimUserMessageCreateParams : GimBaseMessageCreateParams
    {
        /// <summary>
        /// Message text content.
        /// </summary>
        public string Message { get; set; }
    }
}
