namespace Gamania.GIMChat.Internal.Data.DTOs
{
    /// <summary>
    /// DTO for message gap (Huge Gap) check request.
    /// Used during reconnection to verify if the amount of missed messages is too large.
    /// </summary>
    internal class MessageGapCheckRequestDTO
    {
        public long prev_start_ts { get; set; }
        public long prev_end_ts { get; set; }
        public int prev_cache_count { get; set; }
        public long next_start_ts { get; set; }
        public long next_end_ts { get; set; }
        public int next_cache_count { get; set; }
        public bool reverse { get; set; }
    }
}
