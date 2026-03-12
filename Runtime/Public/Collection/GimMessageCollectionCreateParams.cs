namespace Gamania.GIMChat
{
    /// <summary>
    /// MessageCollection creation parameters.
    /// </summary>
    public class GimMessageCollectionCreateParams
    {
        /// <summary>
        /// Message list query parameters for controlling pagination size and filters.
        /// </summary>
        public GimMessageListParams MessageListParams { get; set; }

        /// <summary>
        /// Starting time point for loading messages (Unix ms).
        /// Default is long.MaxValue, meaning start from the latest message.
        /// </summary>
        public long StartingPoint { get; set; } = long.MaxValue;

        /// <summary>
        /// Creates MessageCollectionCreateParams with default parameters.
        /// </summary>
        public GimMessageCollectionCreateParams()
        {
            MessageListParams = new GimMessageListParams();
        }

        /// <summary>
        /// Creates with specified MessageListParams.
        /// </summary>
        /// <param name="messageListParams">Message list params. Uses default if null.</param>
        /// <param name="startingPoint">Starting time point. Default is long.MaxValue.</param>
        public GimMessageCollectionCreateParams(
            GimMessageListParams messageListParams,
            long startingPoint = long.MaxValue)
        {
            MessageListParams = messageListParams ?? new GimMessageListParams();
            StartingPoint = startingPoint;
        }
    }
}
