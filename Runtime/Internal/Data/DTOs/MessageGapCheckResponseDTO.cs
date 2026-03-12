using System.Collections.Generic;

namespace Gamania.GIMChat.Internal.Data.DTOs
{
    /// <summary>
    /// DTO for server response of message gap (Huge Gap) check result.
    /// If is_huge_gap is true, too many messages are missing, should clear and reload;
    /// if false, can directly use prev_messages and next_messages to fill the gap.
    /// </summary>
    internal class MessageGapCheckResponseDTO
    {
        public bool is_huge_gap { get; set; }
        public List<MessageDTO> prev_messages { get; set; }
        public bool prev_has_more { get; set; }
        public List<MessageDTO> next_messages { get; set; }
        public bool next_has_more { get; set; }
    }
}
