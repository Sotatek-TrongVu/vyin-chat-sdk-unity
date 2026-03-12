namespace Gamania.GIMChat
{
    /// <summary>
    /// Defines the role of a user in a channel
    /// </summary>
    public enum GimRole
    {
        /// <summary>
        /// Regular user with no special permissions
        /// </summary>
        None,

        /// <summary>
        /// Channel operator with administrative permissions
        /// </summary>
        Operator
    }

    /// <summary>
    /// Represents the sender of a message, extending <see cref="GimUser"/> with role information
    /// </summary>
    public class GimSender : GimUser
    {
        /// <summary>
        /// Role of the sender in the channel
        /// </summary>
        public GimRole Role { get; set; } = GimRole.None;
    }
}
