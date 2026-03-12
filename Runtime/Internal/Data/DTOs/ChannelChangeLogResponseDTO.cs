using System.Collections.Generic;

namespace Gamania.GIMChat.Internal.Data.DTOs
{
    /// <summary>
    /// DTO for channel changelog API response
    ///
    /// API endpoint: GET /api/users/{user_id}/my_group_channels/changelogs?ts={timestamp}
    ///
    /// Used to sync channel changes (added, updated, deleted) missed during offline period
    /// </summary>
    public class ChannelChangeLogResponseDTO
    {
        /// <summary>
        /// List of updated channels (includes newly added and modified channels)
        /// </summary>
        public List<ChannelDTO> updated { get; set; }

        /// <summary>
        /// List of deleted channel URLs
        /// </summary>
        public List<string> deleted { get; set; }

        /// <summary>
        /// Next page token for pagination (null if no more pages)
        /// </summary>
        public string next { get; set; }
    }
}
