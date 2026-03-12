// Runtime/Public/Message/GimMessageListParams.cs
// Message list parameters: controls pagination size, filters, etc.

using System.Collections.Generic;

namespace Gamania.GIMChat
{
    /// <summary>
    /// Message list query parameters for controlling pagination size and filters.
    /// </summary>
    public class GimMessageListParams
    {
        // ══════════════════════════════════════════════════════════════════════════════
        // Pagination Parameters
        // ══════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Number of messages to load in backward (older) direction. Default is 20.
        /// </summary>
        public int PreviousResultSize { get; set; } = 20;

        /// <summary>
        /// Number of messages to load in forward (newer) direction. Default is 0.
        /// </summary>
        public int NextResultSize { get; set; } = 0;

        /// <summary>
        /// Whether to include the message at the specified timestamp or message ID. Default is false.
        /// </summary>
        public bool IsInclusive { get; set; } = false;

        /// <summary>
        /// Whether to reverse the sort order. If false, results are sorted ascending by time. Default is false.
        /// </summary>
        public bool Reverse { get; set; } = false;

        // ══════════════════════════════════════════════════════════════════════════════
        // Filter Parameters
        // ══════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Message type filter. Default is All (no filtering).
        /// </summary>
        public GimMessageTypeFilter MessageTypeFilter { get; set; } = GimMessageTypeFilter.All;

        /// <summary>
        /// Custom type filter (multiple). Set to null for no filtering.
        /// </summary>
        public List<string> CustomTypes { get; set; }

        /// <summary>
        /// Sender ID filter. Set to null for no filtering.
        /// </summary>
        public List<string> SenderUserIds { get; set; }

        // ══════════════════════════════════════════════════════════════════════════════
        // Include Options
        // ══════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Whether to include message MetaArray. Default is false.
        /// </summary>
        public bool IncludeMetaArray { get; set; } = false;

        /// <summary>
        /// Whether to include message Reactions. Default is false.
        /// </summary>
        public bool IncludeReactions { get; set; } = false;

        /// <summary>
        /// Whether to include message thread info. Default is false.
        /// </summary>
        public bool IncludeThreadInfo { get; set; } = false;

        /// <summary>
        /// Whether to include parent message info. Default is false.
        /// </summary>
        public bool IncludeParentMessageInfo { get; set; } = false;

        /// <summary>
        /// Reply type filter. Default is None.
        /// </summary>
        public GimReplyType ReplyType { get; set; } = GimReplyType.None;

        // ══════════════════════════════════════════════════════════════════════════════
        // Methods
        // ══════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Checks if a message matches the filter criteria of this params.
        /// </summary>
        /// <param name="message">The message to check.</param>
        /// <returns>True if the message matches the criteria.</returns>
        public bool BelongsTo(GimBaseMessage message)
        {
            if (message == null) return false;

            // Check message type
            if (MessageTypeFilter != GimMessageTypeFilter.All)
            {
                var messageType = GetMessageType(message);
                if (messageType != MessageTypeFilter)
                    return false;
            }

            // Check custom type
            if (CustomTypes != null && CustomTypes.Count > 0)
            {
                if (string.IsNullOrEmpty(message.CustomType) || !CustomTypes.Contains(message.CustomType))
                    return false;
            }

            // Check sender
            if (SenderUserIds != null && SenderUserIds.Count > 0)
            {
                if (message.Sender == null || !SenderUserIds.Contains(message.Sender.UserId))
                    return false;
            }

            // Check reply type
            if (ReplyType == GimReplyType.OnlyReplyToChannel)
            {
                // If it's a reply but not ReplyToChannel, exclude it
                // Note: GimBaseMessage may not have ParentMessageId and IsReplyToChannel yet
                // Logic preserved here for future message model extension
            }

            return true;
        }

        /// <summary>
        /// Creates a copy of this params.
        /// </summary>
        public GimMessageListParams Clone()
        {
            return new GimMessageListParams
            {
                PreviousResultSize = PreviousResultSize,
                NextResultSize = NextResultSize,
                IsInclusive = IsInclusive,
                Reverse = Reverse,
                MessageTypeFilter = MessageTypeFilter,
                CustomTypes = CustomTypes != null ? new List<string>(CustomTypes) : null,
                SenderUserIds = SenderUserIds != null ? new List<string>(SenderUserIds) : null,
                IncludeMetaArray = IncludeMetaArray,
                IncludeReactions = IncludeReactions,
                IncludeThreadInfo = IncludeThreadInfo,
                IncludeParentMessageInfo = IncludeParentMessageInfo,
                ReplyType = ReplyType
            };
        }

        // ══════════════════════════════════════════════════════════════════════════════
        // Private Methods
        // ══════════════════════════════════════════════════════════════════════════════

        private static GimMessageTypeFilter GetMessageType(GimBaseMessage message)
        {
            // Currently only supports GimUserMessage, other types to be extended
            return message switch
            {
                GimUserMessage => GimMessageTypeFilter.User,
                // GimFileMessage => GimMessageTypeFilter.File,    // To be implemented
                // GimAdminMessage => GimMessageTypeFilter.Admin,  // To be implemented
                _ => GimMessageTypeFilter.All
            };
        }
    }
}
