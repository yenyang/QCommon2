using Colossal.Logging;
using Colossal.Rendering;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace QCommonLib
{
    // Log file location %AppData%\..\LocalLow\Colossal Order\Cities Skylines II\Logs\

    public class QLoggerStatic
    {
        internal static readonly QLogger s_Instance;

        /// <summary>
        /// Static wrapper for QLogger, instansiate with:
        /// public class Log : QLoggerStatic { }
        /// </summary>
        static QLoggerStatic()
        {
            s_Instance = new QLogger();
        }

        #region Redirect to instance
        /// <summary>
        /// Set whether or not to save debug messages
        /// </summary>
        public static bool IsDebug
        {
            get => s_Instance.IsDebug;
            set => s_Instance.IsDebug = value;
        }

        public static void Debug(string message, string code = "")
        {
            s_Instance?.Debug(message, code);
        }

        public static void Debug(Exception exception, string code = "")
        {
            s_Instance?.Debug(exception, code);
        }

        public static void Info(string message, string code = "")
        {
            s_Instance?.Info(message, code);
        }

        public static void Info(Exception exception, string code = "")
        {
            s_Instance?.Info(exception, code);
        }

        public static void Warning(string message, string code = "")
        {
            s_Instance?.Warning(message, code);
        }

        public static void Warning(Exception exception, string code = "")
        {
            s_Instance?.Warning(exception, code);
        }

        public static void Error(string message, string code = "")
        {
            s_Instance?.Error(message, code);
        }

        public static void Error(Exception exception, string code = "")
        {
            s_Instance?.Error(exception, code);
        }
        #endregion
    }

    public class QLogger
    {
        /// <summary>
        /// The calling assembly
        /// </summary>
        internal Assembly AssemblyObject { get; private set; }
        internal string AssemblyName { get => AssemblyObject.GetName().Name; }
        /// <summary>
        /// The full path and name of the log file
        /// </summary>
        internal string LogFile { get; private set; }
        /// <summary>
        /// The ColossalOrder ILog instance
        /// </summary>
        //internal ILog Logger { get; private set; }
        /// <summary>
        /// NewLine for the player's environment
        /// </summary>
        internal string NL = Environment.NewLine;
        /// <summary>
        /// Runtime counter
        /// </summary>
        internal Stopwatch Timer { get; private set; }
        /// <summary>
        /// Log levels. Also output in log file.
        /// </summary>
        public enum LogLevel
        {
            Debug,
            Info,
            Error,
        }
        /// <summary>
        /// Should debug messages be logged?
        /// </summary>
        public bool IsDebug { get; set; }

        /// <summary>
        /// Create QLogger instance
        /// </summary>
        /// <param name="isDebug">Override should debug messages be logged?</param>
        /// <param name="fileName">Override the generated path/file name</param>
        /// <param name="location">Override which log(s) minor messages are logged to</param>
        /// <exception cref="ArgumentNullException"></exception>
        public QLogger(bool isDebug = true, string fileName = "")
        {
            AssemblyObject = Assembly.GetCallingAssembly() ?? throw new ArgumentNullException("QLogger: Failed to find calling assembly");
            LogFile = Path.Combine(Path.GetDirectoryName(Application.consoleLogPath), "Logs", (fileName == "" ? AssemblyName : fileName) + ".log") ;
            //Logger = LogManager.GetLogger(fileName == "" ? AssemblyName : fileName);
            IsDebug = isDebug;
            Timer = Stopwatch.StartNew();

            if (File.Exists(LogFile))
            {
                File.Delete(LogFile);
            }

            AssemblyName details = AssemblyObject.GetName();
            string offset;
            try
            {
                TimeSpan ts = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
                offset = string.Format("{0}:{1:D2}", ts.Hours, ts.Minutes);
                if (ts.Hours > 0 || (ts.Hours == 0 && ts.Minutes > 0))
                {
                    offset = "+" + offset;
                }
            }
            catch (Exception)
            {
                offset = "Unknown";
            }
            Info($"{details.Name} v{details.Version} at " + DateTime.UtcNow.ToString(new CultureInfo("en-GB")) + $" ({offset})");
        }

        ~QLogger()
        {
            Info($"{AssemblyName} closing (" + DateTime.UtcNow.ToString(new CultureInfo("en-GB")) + ")");
        }

        internal void Do(string message, LogLevel logLevel, string code)
        {
            try
            {
                lock (LogFile)
                {
                    var ticks = Timer.ElapsedTicks;
                    string msg = "";
                    if (code != "") code += " ";

                    int maxLen = Enum.GetNames(typeof(LogLevel)).Select(str => str.Length).Max();
                    msg += string.Format($"{{0, -{maxLen}}}", $"[{logLevel}] ");

                    long secs = ticks / Stopwatch.Frequency;
                    long fraction = ticks % Stopwatch.Frequency;
                    msg += string.Format($"{secs:n0}.{fraction:D7} | {code}{message}{NL}");

                    using (StreamWriter w = File.AppendText(LogFile))
                    {
                        w.Write(msg);
                    }
                    //Logger.Info(msg);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("QLogger failed to log!\n" + e.ToStringNoTrace());
            }
        }

        #region Debug
        // Print the message to LogLocation only if IsDebug is true
        public void Debug(string message, string code = "")
        {
            if (IsDebug)
            {
                Do(message, LogLevel.Debug, code);
            }
        }

        public void Debug(Exception exception, string code = "")
        {
            if (IsDebug)
            {
                Do(exception.ToString(), LogLevel.Debug, code);
            }
        }
        #endregion

        #region Info
        // Print the message to LogLocation
        public void Info(string message, string code = "")
        {
            Do(message, LogLevel.Info, code);
        }

        public void Info(Exception exception, string code = "")
        {
            Do(exception.ToString(), LogLevel.Info, code);
        }
        #endregion

        #region Warning
        // Print the message everywhere
        public void Warning(string message, string code = "")
        {
            Do(message, LogLevel.Error, code);
        }

        public void Warning(Exception exception, string code = "")
        {
            Do(exception.ToStringNoTrace(), LogLevel.Error, code);
        }
        #endregion

        #region Error
        // Print the message everywhere, include stacktrace
        public void Error(string message, string code = "")
        {
            Do(message + NL + new StackTrace().ToString() + NL, LogLevel.Error, code);
        }

        public void Error(Exception exception, string code = "")
        {
            string message = exception.ToString();
            if (exception.StackTrace is null || exception.StackTrace == "") message += NL + new StackTrace().ToString();
            Do(message, LogLevel.Error, code);
        }
        #endregion
    }

    public static class QExtensions
    {
        public static string ToStringNoTrace(this Exception e)
        {
            StringBuilder stringBuilder = new StringBuilder(e.GetType().ToString());
            stringBuilder.Append(": ").Append(e.Message);
            return stringBuilder.ToString();
        }

        public static string RemoveWhitespace(this string input)
        {
            return new string(input.ToCharArray()
                .Where(c => !Char.IsWhiteSpace(c))
                .ToArray());
        }
    }
}




