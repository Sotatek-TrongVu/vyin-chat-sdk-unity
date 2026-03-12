// -----------------------------------------------------------------------------
//
// GimConnectionState - Public API
// WebSocket connection state enum with extension methods
//
// -----------------------------------------------------------------------------

namespace Gamania.GIMChat
{
    /// <summary>
    /// WebSocket connection state
    /// </summary>
    public enum GimConnectionState
    {
        /// <summary>
        /// Connection is closed or not yet established
        /// </summary>
        Closed = 0,

        /// <summary>
        /// Connection is being established
        /// </summary>
        Connecting = 1,

        /// <summary>
        /// Connection is being closed gracefully
        /// </summary>
        Closing = 2,

        /// <summary>
        /// Connection is open and ready for communication
        /// </summary>
        Open = 3
    }

    /// <summary>
    /// Extension methods for GimConnectionState
    /// </summary>
    public static class GimConnectionStateExtensions
    {
        /// <summary>
        /// Check if connection is open
        /// </summary>
        public static bool IsConnected(this GimConnectionState state)
            => state == GimConnectionState.Open;

        /// <summary>
        /// Check if connection is being established
        /// </summary>
        public static bool IsConnecting(this GimConnectionState state)
            => state == GimConnectionState.Connecting;

        /// <summary>
        /// Check if connection is being closed
        /// </summary>
        public static bool IsClosing(this GimConnectionState state)
            => state == GimConnectionState.Closing;

        /// <summary>
        /// Check if connection is closed
        /// </summary>
        public static bool IsClosed(this GimConnectionState state)
            => state == GimConnectionState.Closed;
    }
}
