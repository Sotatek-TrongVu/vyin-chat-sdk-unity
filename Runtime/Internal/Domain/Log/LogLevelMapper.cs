namespace Gamania.GIMChat.Internal.Domain.Log
{
    /// <summary>
    /// Maps between public GimLogLevel and internal LogLevel
    /// </summary>
    internal static class LogLevelMapper
    {
        public static LogLevel FromVcLogLevel(GimLogLevel vcLevel)
        {
            return vcLevel switch
            {
                GimLogLevel.Verbose => LogLevel.Verbose,
                GimLogLevel.Debug => LogLevel.Debug,
                GimLogLevel.Info => LogLevel.Info,
                GimLogLevel.Warning => LogLevel.Warning,
                GimLogLevel.Error => LogLevel.Error,
                GimLogLevel.None => LogLevel.None,
                _ => LogLevel.Info
            };
        }

        public static GimLogLevel ToVcLogLevel(LogLevel level)
        {
            return level switch
            {
                LogLevel.Verbose => GimLogLevel.Verbose,
                LogLevel.Debug => GimLogLevel.Debug,
                LogLevel.Info => GimLogLevel.Info,
                LogLevel.Warning => GimLogLevel.Warning,
                LogLevel.Error => GimLogLevel.Error,
                LogLevel.None => GimLogLevel.None,
                _ => GimLogLevel.Info
            };
        }
    }
}
