using System.Collections.Generic;

namespace Gamania.GIMChat.Internal.Data.DTOs
{
    /// <summary>
    /// DTO (Data Transfer Object) for server response containing message changelogs.
    /// Includes added/updated messages and deleted message IDs.
    /// </summary>
    internal class MessageChangeLogResponseDTO
    {
        /// <summary>Added or updated message list.</summary>
        public List<MessageDTO> updated { get; set; }

        /// <summary>Deleted message ID list.</summary>
        public List<long> deleted { get; set; }

        /// <summary>Indicates if server has more unreturned changelog entries.</summary>
        public bool has_more { get; set; }

        /// <summary>Token for paginated fetching of next changelog batch.</summary>
        public string next { get; set; }
    }
}
