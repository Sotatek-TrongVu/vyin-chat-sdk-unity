using System.Collections.Generic;

namespace Gamania.GIMChat
{
    /// <summary>
    /// Callback handler for user-related operations
    /// </summary>
    /// <param name="user">The user object if the operation succeeded, null otherwise</param>
    /// <param name="error">The exception if the operation failed, null otherwise</param>
    public delegate void GimUserHandler(GimUser user, GimException error);

    /// <summary>
    /// Callback handler for message-related operations
    /// </summary>
    /// <param name="message">The message object if the operation succeeded, null otherwise</param>
    /// <param name="error">The exception if the operation failed, null otherwise</param>
    public delegate void GimUserMessageHandler(GimUserMessage message, GimException error);

    /// <summary>
    /// Callback handler for group channel operations
    /// </summary>
    /// <param name="channel">The group channel object if the operation succeeded, null otherwise</param>
    /// <param name="error">The exception if the operation failed, null otherwise</param>
    public delegate void GimGroupChannelCallbackHandler(GimGroupChannel channel, GimException error);

    // ── Collection callbacks ─────────────────────────────────────────────────────

    /// <summary>
    /// Callback for message list operations (pagination, startCollection).
    /// </summary>
    /// <param name="messages">Loaded messages, or null on error.</param>
    /// <param name="error">The exception if the operation failed, null on success.</param>
    public delegate void GimMessageListHandler(IReadOnlyList<GimBaseMessage> messages, GimException error);

    /// <summary>
    /// Callback for channel list operations (LoadMore).
    /// </summary>
    /// <param name="channels">Loaded channels, or null on error.</param>
    /// <param name="error">The exception if the operation failed, null on success.</param>
    public delegate void GimGroupChannelListHandler(IReadOnlyList<GimGroupChannel> channels, GimException error);

    /// <summary>
    /// Generic error-only callback (e.g., RemoveFailed, RemoveAllFailed).
    /// </summary>
    /// <param name="error">The exception if the operation failed, null on success.</param>
    public delegate void GimErrorHandler(GimException error);
}
