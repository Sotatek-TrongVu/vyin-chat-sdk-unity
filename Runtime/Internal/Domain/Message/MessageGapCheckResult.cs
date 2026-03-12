using System.Collections.Generic;
using Gamania.GIMChat.Internal.Domain.Models;

namespace Gamania.GIMChat.Internal.Domain.Message
{
    /// <summary>
    /// Message gap (Huge Gap) check result.
    /// </summary>
    internal class MessageGapCheckResult
    {
        /// <summary>Whether it's a huge gap; if true, recommend resetting Collection.</summary>
        public bool IsHugeGap { get; set; }

        /// <summary>Missing messages for backward fill.</summary>
        public IReadOnlyList<MessageBO> PrevMessages { get; set; }

        /// <summary>Whether backward fill has more data (Gap API doesn't auto-paginate, usually handled by caller).</summary>
        public bool PrevHasMore { get; set; }

        /// <summary>Missing messages for forward fill.</summary>
        public IReadOnlyList<MessageBO> NextMessages { get; set; }

        /// <summary>Whether forward fill has more data.</summary>
        public bool NextHasMore { get; set; }
    }
}
