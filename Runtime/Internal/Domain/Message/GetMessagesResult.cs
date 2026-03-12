using System.Collections.Generic;
using Gamania.GIMChat.Internal.Domain.Models;

namespace Gamania.GIMChat.Internal.Domain.Message
{
    /// <summary>
    /// Return result for GetMessagesAsync, containing message list and pagination info.
    /// </summary>
    internal class GetMessagesResult
    {
        /// <summary>Message list.</summary>
        public IReadOnlyList<MessageBO> Messages { get; set; }

        /// <summary>Whether there are older messages.</summary>
        public bool HasPrevious { get; set; }

        /// <summary>Whether there are newer messages.</summary>
        public bool HasNext { get; set; }
    }
}
