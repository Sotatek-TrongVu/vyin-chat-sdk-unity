namespace Gamania.GIMChat
{
    /// <summary>
    /// Base parameters for creating any type of message.
    /// </summary>
    public class GimBaseMessageCreateParams
    {
        /// <summary>
        /// Custom data attached to the message.
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Custom message type for categorization.
        /// </summary>
        public string CustomType { get; set; }
    }
}
