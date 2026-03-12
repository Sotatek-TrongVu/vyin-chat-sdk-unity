using System.Collections.Generic;

namespace Gamania.GIMChat.Internal.Data.DTOs
{
    /// <summary>
    /// DTO (Data Transfer Object) for server response containing historical message list.
    /// </summary>
    internal class MessageListResponseDTO
    {
        /// <summary>Historical message list.</summary>
        public List<MessageDTO> messages { get; set; }

        /// <summary>Whether there are older messages.</summary>
        public bool? has_prev { get; set; }

        /// <summary>Whether there are newer messages.</summary>
        public bool? has_next { get; set; }
    }
}
