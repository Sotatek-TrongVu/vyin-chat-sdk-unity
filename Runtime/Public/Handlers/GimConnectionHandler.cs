using System;

namespace Gamania.GIMChat
{
    /// <summary>
    /// Handler for connection state change events
    /// </summary>
    public class GimConnectionHandler
    {
        /// <summary>
        /// Invoked when connection is established
        /// </summary>
        public Action<string> OnConnected;

        /// <summary>
        /// Invoked when connection is closed
        /// </summary>
        public Action<string> OnDisconnected;

        /// <summary>
        /// Invoked when reconnection attempt starts
        /// </summary>
        public Action OnReconnectStarted;

        /// <summary>
        /// Invoked when reconnection succeeds
        /// </summary>
        public Action OnReconnectSucceeded;

        /// <summary>
        /// Invoked when reconnection fails
        /// </summary>
        public Action OnReconnectFailed;
    }
}
