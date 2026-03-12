// Runtime/Public/Message/GimReplyType.cs
// Reply type filter for filtering reply messages when loading messages.

namespace Gamania.GIMChat
{
    /// <summary>
    /// Reply type filter for filtering reply messages when loading messages.
    /// </summary>
    public enum GimReplyType
    {
        /// <summary>Do not include any replies.</summary>
        None = 0,

        /// <summary>Include all replies.</summary>
        All,

        /// <summary>Only include replies with IsReplyToChannel = true.</summary>
        OnlyReplyToChannel
    }

    /// <summary>
    /// GimReplyType extension methods.
    /// </summary>
    internal static class GimReplyTypeExtensions
    {
        /// <summary>
        /// Converts to API query parameter value.
        /// </summary>
        public static string ToApiValue(this GimReplyType replyType)
        {
            return replyType switch
            {
                GimReplyType.All => "all",
                GimReplyType.OnlyReplyToChannel => "only_reply_to_channel",
                _ => "none"
            };
        }
    }
}
