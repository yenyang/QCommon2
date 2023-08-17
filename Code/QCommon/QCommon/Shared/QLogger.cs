using Colossal.Logging;
using System;
using System.Diagnostics;

namespace QCommonLib
{
    // Log file location %AppData%\..\LocalLow\Colossal Order\Cities Skylines II\

    internal abstract class QLogger
    {
        internal static string Name { get; set; }
        public static ILog ILogger = LogManager.GetLogger(Name);

        [Conditional("DEBUG")]
        public static void LogDebugInfo(string message)
        {
            ILogger.Log(Level.Debug, message, new DebugException("CS2Test DebugException 03"));
        }

        public static void LogInfo(string message)
        {
            ILogger.Log(Level.Debug, message, new DebugException("CS2Test DebugException 04"));
        }
    }

    public class DebugException : Exception
    {
        public DebugException(string message) : base(message) { }
    }
}
