namespace Gamania.GIMChat
{
    /// <summary>
    /// Message type filter for filtering specific types when loading messages.
    /// </summary>
    public enum GimMessageTypeFilter
    {
        /// <summary>All message types.</summary>
        All = 0,

        /// <summary>User messages only.</summary>
        User,

        /// <summary>File messages only.</summary>
        File,

        /// <summary>Admin messages only.</summary>
        Admin
    }

    /// <summary>
    /// GimMessageTypeFilter extension methods.
    /// </summary>
    internal static class GimMessageTypeFilterExtensions
    {
        /// <summary>
        /// Converts to API query parameter value.
        /// </summary>
        public static string ToApiValue(this GimMessageTypeFilter filter)
        {
            return filter switch
            {
                GimMessageTypeFilter.User => "MESG",
                GimMessageTypeFilter.File => "FILE",
                GimMessageTypeFilter.Admin => "ADMM",
                _ => "" // All
            };
        }
    }
}
