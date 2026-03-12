namespace Gamania.GIMChat.Internal
{
    /// <summary>
    /// Internal event routing hub for SDK Collections.
    ///
    /// Collections register themselves as internal channel event listeners
    /// without exposing the mechanism to external app code.
    ///
    /// Usage in a concrete Collection:
    /// <code>
    ///   DelegateManager.AddChannelHandler(_handlerId, new GimGroupChannelHandler {
    ///       OnMessageReceived = HandleMessageReceived,
    ///       OnMessageUpdated  = HandleMessageUpdated,
    ///   });
    ///   // On dispose:
    ///   DelegateManager.RemoveChannelHandler(_handlerId);
    /// </code>
    /// </summary>
    public class GimSdkDelegateManager
    {
        public static GimSdkDelegateManager Instance { get; } = new GimSdkDelegateManager();

        // ── Channel events ────────────────────────────────────────────────────────

        /// <summary>
        /// Registers a handler to receive channel events (OnMessageReceived, OnMessageUpdated).
        /// If a handler with the same identifier already exists it is replaced.
        /// </summary>
        public void AddChannelHandler(string identifier, GimGroupChannelHandler handler)
        {
            // Remove first to allow re-registration / replacement
            GimGroupChannel.RemoveGroupChannelHandler(identifier);
            GimGroupChannel.AddGroupChannelHandler(identifier, handler);
        }

        /// <summary>
        /// Returns the channel handler registered with the given identifier, or null.
        /// </summary>
        public GimGroupChannelHandler GetChannelHandler(string identifier)
        {
            return GimGroupChannel.GetGroupChannelHandler(identifier);
        }

        /// <summary>
        /// Unregisters the channel event handler with the given identifier.
        /// </summary>
        public void RemoveChannelHandler(string identifier)
        {
            GimGroupChannel.RemoveGroupChannelHandler(identifier);
        }

        /// <summary>
        /// Unregisters all channel event handlers.
        /// Used during disconnect/logout to clean up all listeners.
        /// </summary>
        public void RemoveAllChannelHandlers()
        {
            GimGroupChannel.RemoveAllGroupChannelHandlers();
        }
    }
}
