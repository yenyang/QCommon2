using Colossal.Logging;
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

    /// <summary>
    /// Static log for quick debugging, goes to {AssemblyName}_debug.log
    /// </summary>
    public class QLog
    {
        /// <summary>
        /// The debug logger object
        /// </summary>
        internal static QLoggerCustom s_Instance = new(true);

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

    public class QLoggerCustom : QLoggerBase
    {
        /// <summary>
        /// Runtime counter
        /// </summary>
        internal Stopwatch Timer { get; set; }
        /// <summary>
        /// The full path and name of the log file
        /// </summary>
        internal string LogFile { get; private set; }
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
        /// Create QDebug instance
        /// </summary>
        /// <param name="isDebug">Override should debug messages be logged?</param>
        public QLoggerCustom(bool isDebug = true) : base(isDebug)
        {
            Timer = Stopwatch.StartNew();
            LogFile = Path.Combine(Path.GetDirectoryName(Application.consoleLogPath), "Logs", AssemblyName + "_debug.log");

            if (File.Exists(LogFile))
            {
                File.Delete(LogFile);
            }

            AssemblyName details = AssemblyObject.GetName();
            Info($"{details.Name} v{details.Version} at " + DateTime.UtcNow.ToString(new CultureInfo("en-GB")) + $" ({m_TimezoneOffset})");
        }

        ~QLoggerCustom()
        {
            Info($"{AssemblyName} closing (" + DateTime.UtcNow.ToString(new CultureInfo("en-GB")) + $" ({m_TimezoneOffset})");
        }

        #region Debug
        public override void Debug(string message, string code = "")
        {
            if (IsDebug)
            {
                Do(message, LogLevel.Debug, code);
            }
        }

        public override void Debug(Exception exception, string code = "")
        {
            if (IsDebug)
            {
                Do(exception.ToString(), LogLevel.Debug, code);
            }
        }
        #endregion

        #region Info
        public override void Info(string message, string code = "")
        {
            Do(message, LogLevel.Info, code);
        }

        public override void Info(Exception exception, string code = "")
        {
            Do(exception.ToString(), LogLevel.Info, code);
        }
        #endregion

        #region Warning
        public override void Warning(string message, string code = "")
        {
            Do(message, LogLevel.Error, code);
        }

        public override void Warning(Exception exception, string code = "")
        {
            Do(exception.ToStringNoTrace(), LogLevel.Error, code);
        }
        #endregion

        #region Error
        public override void Error(string message, string code = "")
        {
            Do(message + NL + new StackTrace().ToString() + NL, LogLevel.Error, code);
        }

        public override void Error(Exception exception, string code = "")
        {
            string message = exception.ToString();
            if (exception.StackTrace is null || exception.StackTrace == "") message += NL + new StackTrace().ToString();
            Do(message, LogLevel.Error, code);
        }
        #endregion

        private void Do(string message, LogLevel logLevel, string code)
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

                    using StreamWriter w = File.AppendText(LogFile);
                    w.Write(msg);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("QLogger failed to log!\n" + e.ToStringNoTrace());
            }
        }
    }

    public class QLoggerCO : QLoggerBase
    {
        /// <summary>
        /// The ColossalOrder ILog instance
        /// </summary>
        internal ILog Logger { get; set; }
        private readonly bool _mirrorToStatic;

        public QLoggerCO(bool isDebug = true, string filename = "", bool mirrorToStatic = true) : base(isDebug)
        {
            _mirrorToStatic = mirrorToStatic;
            Logger = LogManager.GetLogger(filename == "" ? AssemblyName : filename);

            AssemblyName details = AssemblyObject.GetName();
            Logger.Info($"{details.Name} v{details.Version} at " + DateTime.UtcNow.ToString(new CultureInfo("en-GB")) + $" ({m_TimezoneOffset})");
        }

        ~QLoggerCO()
        {
            Logger.Info($"{AssemblyName} closing (" + DateTime.UtcNow.ToString(new CultureInfo("en-GB")) + $" ({m_TimezoneOffset})");
        }

        #region Debug
        public override void Debug(string message, string code = "")
        {
            if (IsDebug)
            { 
                if (_mirrorToStatic) QLog.Debug(message, code);
                if (code != "") code += " ";
                Logger.Debug(code + message);
            }
        }

        public override void Debug(Exception exception, string code = "")
        {
            if (IsDebug)
            {
                if (_mirrorToStatic) QLog.Debug(exception, code);
                Logger.Debug(exception, code);
            }
        }
        #endregion

        #region Info
        public override void Info(string message, string code = "")
        {
            if (_mirrorToStatic) QLog.Info(message, code);
            if (code != "") code += " ";
            Logger.Info(code + message);
        }

        public override void Info(Exception exception, string code = "")
        {
            if (_mirrorToStatic) QLog.Info(exception, code);
            Logger.Info(exception, code);
        }
        #endregion

        #region Warning
        public override void Warning(string message, string code = "")
        {
            if (_mirrorToStatic) QLog.Warning(message, code);
            if (code != "") code += " ";
            Logger.Warn(code + message);
        }

        public override void Warning(Exception exception, string code = "")
        {
            if (_mirrorToStatic) QLog.Warning(exception, code);
            Logger.Warn(exception, code);
        }
        #endregion

        #region Error
        public override void Error(string message, string code = "")
        {
            if (_mirrorToStatic) QLog.Error(message, code);
            if (code != "") code += " ";
            Logger.Error(code + message + NL + new StackTrace().ToString() + NL);
        }

        public override void Error(Exception exception, string code = "")
        {
            if (_mirrorToStatic) QLog.Error(exception, code);
            if (code != "") code += " ";
            string message = exception.ToString();
            if (exception.StackTrace is null || exception.StackTrace == "") message += NL + new StackTrace().ToString();
            Logger.Error(code + message + NL);
        }
        #endregion
    }

    public abstract class QLoggerBase
    {
        /// <summary>
        /// The calling assembly
        /// </summary>
        internal Assembly AssemblyObject { get; set; }
        internal string AssemblyName { get => AssemblyObject.GetName().Name; }
        /// <summary>
        /// NewLine for the player's environment
        /// </summary>
        internal string NL = Environment.NewLine;
        /// <summary>
        /// String for the timezone offset, eg "+1:00"
        /// </summary>
        internal static string m_TimezoneOffset;
        /// <summary>
        /// Should debug messages be logged?
        /// </summary>
        public bool IsDebug { get; set; }

        public QLoggerBase(bool isDebug = true)
        {
            AssemblyObject = Assembly.GetCallingAssembly() ?? throw new ArgumentNullException("QLogger: Failed to find calling assembly");
            m_TimezoneOffset = GetTimezoneOffset();
            IsDebug = isDebug;
        }

        public abstract void Debug(string message, string code = "");
        public abstract void Debug(Exception exception, string code = "");
        public abstract void Info(string message, string code = "");
        public abstract void Info(Exception exception, string code = "");
        public abstract void Warning(string message, string code = "");
        public abstract void Warning(Exception exception, string code = "");
        public abstract void Error(string message, string code = "");
        public abstract void Error(Exception exception, string code = "");

        public static string GetTimezoneOffset()
        {
            string offset = "Unknown";
            try
            {
                TimeSpan ts = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
                offset = string.Format("{0}:{1:D2}", ts.Hours, ts.Minutes);
                if (ts.Hours > 0 || (ts.Hours == 0 && ts.Minutes > 0))
                {
                    offset = "+" + offset;
                }
            }
            catch (Exception) { }

            return offset;
        }

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
