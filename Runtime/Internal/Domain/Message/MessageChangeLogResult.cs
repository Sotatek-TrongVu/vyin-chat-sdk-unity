using System.Collections.Generic;
using Gamania.GIMChat.Internal.Domain.Models;

namespace Gamania.GIMChat.Internal.Domain.Message
{
    /// <summary>
    /// Message changelog result.
    /// Used to sync message changes missed during offline period after reconnection.
    /// </summary>
    internal class MessageChangeLogResult
    {
        /// <summary>Updated or added message list.</summary>
        public IReadOnlyList<MessageBO> UpdatedMessages { get; set; }

        /// <summary>Deleted message ID list.</summary>
        public IReadOnlyList<long> DeletedMessageIds { get; set; }

        /// <summary>Whether there are more changes to fetch.</summary>
        public bool HasMore { get; set; }

        /// <summary>Token for next page, null if no more data.</summary>
        public string NextToken { get; set; }
    }
}
