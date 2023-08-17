using Colossal.Logging;
using System;
using System.Diagnostics;

namespace QCommonLib
{
    internal class QLogger
    {
        public static ILog ILogger = LogManager.GetLogger("CS2Test"); // Log file location %AppData%\..\LocalLow\Colossal Order\Cities Skylines II\

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
