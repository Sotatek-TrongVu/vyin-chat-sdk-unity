namespace Gamania.GIMChat
{
    /// <summary>
    /// Represents a user in GIMChat
    /// </summary>
    public class GimUser
    {
        /// <summary>
        /// Unique identifier of the user
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Display name of the user
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// URL of the user's profile image
        /// </summary>
        public string ProfileUrl { get; set; }
    }
}
