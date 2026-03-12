namespace Gamania.GIMChat
{
    /// <summary>
    /// Initialization parameters for GIMChat SDK
    /// </summary>
    public class GimInitParams
    {
        /// <summary>
        /// Application ID (required)
        /// </summary>
        public string AppId { get; }

        /// <summary>
        /// Determines to use local caching
        /// </summary>
        public bool IsLocalCachingEnabled { get; }

        /// <summary>
        /// Log level
        /// </summary>
        public GimLogLevel LogLevel { get; }

        /// <summary>
        /// Host app version
        /// </summary>
        public string AppVersion { get; }

        /// <summary>
        /// Initialize GimInitParams with required and optional parameters
        /// </summary>
        /// <param name="appId">Application ID (required)</param>
        /// <param name="isLocalCachingEnabled">Enable local caching (default: false)</param>
        /// <param name="logLevel">Log level (default: Warning)</param>
        /// <param name="appVersion">Host app version (optional)</param>
        public GimInitParams(
            string appId,
            bool isLocalCachingEnabled = false,
            GimLogLevel logLevel = GimLogLevel.Warning,
            string appVersion = null)
        {
            AppId = appId;
            IsLocalCachingEnabled = isLocalCachingEnabled;
            LogLevel = logLevel;
            AppVersion = appVersion;
        }
    }
}