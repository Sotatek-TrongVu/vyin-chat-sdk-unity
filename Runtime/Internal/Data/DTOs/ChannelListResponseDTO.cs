using System.Collections.Generic;

namespace Gamania.GIMChat.Internal.Data.DTOs
{
    /// <summary>
    /// DTO for channel list API response.
    /// Channel list response (channels + next token).
    /// </summary>
    public class ChannelListResponseDTO
    {
        public List<ChannelDTO> channels { get; set; }
        public string next { get; set; }
    }
}
