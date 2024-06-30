using Colossal.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;

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
        internal static QLoggerCustom s_Instance;

        internal static void Init(bool isDebug)
        {
            s_Instance = new(isDebug);
        }

        #region Redirect to instance
        /// <summary>
        /// Set whether or not to save debug messages
        /// </summary>
        public static bool IsDebug
        {
            get => (s_Instance is not null) && s_Instance.IsDebug;
            set
            {
                if (s_Instance is not null) s_Instance.IsDebug = value;
            }
        }

        public static void Bundle(string key, string message)
        {
            s_Instance?.Bundle(key, message);
        }

        public static void FlushBundle()
        {
            s_Instance?.FlushBundles();
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

        private string _IntroMessage = string.Empty;
        private bool _HasLogged = false;

        /// <summary>
        /// Create QDebug instance
        /// </summary>
        /// <param name="isDebug">Override should debug messages be logged?</param>
        public QLoggerCustom(bool isDebug = true) : base(isDebug)
        {
            Timer = Stopwatch.StartNew();
            string fileNameBase = Path.Combine(Path.GetDirectoryName(UnityEngine.Application.consoleLogPath), "Logs", AssemblyName + "_debug");
            LogFile = fileNameBase + ".log";
            string prevLogFile = fileNameBase + "-prev.log";

            if (File.Exists(prevLogFile))
            {
                File.Delete(prevLogFile);
            }
            if (File.Exists(LogFile))
            {
                File.Move(LogFile, prevLogFile);
            }

            AssemblyName details = AssemblyObject.GetName();
            _IntroMessage = $"{details.Name} v{details.Version} at {GetFormattedTimeNow()}";
        }

        ~QLoggerCustom()
        {
            if (_HasLogged) Info($"{AssemblyName} closing {GetFormattedTimeNow()}");
        }

        #region Debug
        public override void Debug(string message, string code = "")
        {
            Do(message, LogLevel.Debug, code);
        }

        public override void Debug(Exception exception, string code = "")
        {
            Do(exception.ToString(), LogLevel.Debug, code);
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
            if (!(exception.StackTrace is null || exception.StackTrace == "")) message += NL + new StackTrace().ToString();
            Do(message, LogLevel.Error, code);
        }
        #endregion

        private void Do(string message, LogLevel logLevel, string code)
        {
            if (!IsDebug && logLevel == LogLevel.Debug)
            {
                return;
            }
            _HasLogged = true;
            if (_IntroMessage != string.Empty)
            {
                DoImpl(_IntroMessage, LogLevel.Info, string.Empty);
                _IntroMessage = string.Empty;
            }

            DoImpl(message, logLevel, code);
        }

        private void DoImpl(string message, LogLevel logLevel, string code)
        {
            Game.SceneFlow.GameManager.instance.RunOnMainThread(() =>
            {
                DoOnMain(message, logLevel, code);
            });
        }

        private void DoOnMain(string message, LogLevel logLevel, string code)
        {
            try
            {
                lock (LogFile)
                {
                    var ticks = Timer.ElapsedTicks;
                    string msg = string.Empty;
                    if (code != string.Empty) code += " ";
                    string frameCount = string.Empty;
                    try
                    {
                        if (logLevel == LogLevel.Debug) frameCount = $"|{UnityEngine.Time.frameCount}";
                    }
                    catch
                    {
                        frameCount = $"|???";
                    }

                    long secs = ticks / Stopwatch.Frequency;
                    long fraction = ticks % Stopwatch.Frequency;
                    string fracStr = fraction.ToString();
                    string timeStr = $"{secs:n0}.{fracStr.Substring(0, Math.Min(fracStr.Length, 3))}";
                    msg += $"[{logLevel}|{timeStr}{frameCount}] {code}{message}{NL}";

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
        private readonly bool _MirrorToStatic;

        public QLoggerCO(bool isDebug = true, string filename = "", bool mirrorToStatic = true) : base(isDebug)
        {
            _MirrorToStatic = mirrorToStatic;

            filename = filename.Equals(string.Empty) ? AssemblyName : filename;
            string fileNameBase = Path.Combine(Path.GetDirectoryName(UnityEngine.Application.consoleLogPath), "Logs", filename);
            string logFile = fileNameBase + ".log";
            string prevLogFile = fileNameBase + "-prev.log";
            string deleteBuggedFileName = fileNameBase + ".Mod.log";

            if (File.Exists(prevLogFile))
            {
                File.Delete(prevLogFile);
            }
            if (File.Exists(logFile))
            {
                File.Move(logFile, prevLogFile);
            }
            if (File.Exists(deleteBuggedFileName))
            {
                File.Delete(deleteBuggedFileName);
            }

            Logger = LogManager.GetLogger(filename);
            Logger.SetEffectiveness(Level.All);

            AssemblyName details = AssemblyObject.GetName();
            Logger.Info($"{details.Name} v{details.Version} at {GetFormattedTimeNow()}");
        }

        ~QLoggerCO()
        {
            Logger.Info($"{AssemblyName} closing at {GetFormattedTimeNow()}");
        }

        #region Debug
        public override void Debug(string message, string code = "")
        {
            if (IsDebug)
            { 
                if (_MirrorToStatic) QLog.Debug(message, code);
                if (code != string.Empty) code += " ";
                Logger.Debug(code + message);
            }
        }

        public override void Debug(Exception exception, string code = "")
        {
            if (IsDebug)
            {
                if (_MirrorToStatic) QLog.Debug(exception, code);
                Logger.Debug(exception, code);
            }
        }
        #endregion

        #region Info
        public override void Info(string message, string code = "")
        {
            if (_MirrorToStatic) QLog.Info(message, code);
            if (code != string.Empty) code += " ";
            Logger.Info(code + message);
        }

        public override void Info(Exception exception, string code = "")
        {
            if (_MirrorToStatic) QLog.Info(exception, code);
            Logger.Info(exception, code);
        }
        #endregion

        #region Warning
        public override void Warning(string message, string code = "")
        {
            if (_MirrorToStatic) QLog.Warning(message, code);
            if (code != string.Empty) code += " ";
            Logger.Warn(code + message);
        }

        public override void Warning(Exception exception, string code = "")
        {
            if (_MirrorToStatic) QLog.Warning(exception, code);
            Logger.Warn(exception, code);
        }
        #endregion

        #region Error
        public override void Error(string message, string code = "")
        {
            if (_MirrorToStatic) QLog.Error(message, code);
            if (code != string.Empty) code += " ";
            Logger.Error(code + message + NL + new StackTrace().ToString() + NL);
        }

        public override void Error(Exception exception, string code = "")
        {
            if (_MirrorToStatic) QLog.Error(exception, code);
            if (code != string.Empty) code += " ";
            string message = exception.ToString();
            if (!(exception.StackTrace is null || exception.StackTrace == string.Empty)) message += NL + new StackTrace().ToString();
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
        /// Should debug messages be logged?
        /// </summary>
        public bool IsDebug { get; set; }
        /// <summary>
        /// Storage for bundled messages
        /// </summary>
        protected Dictionary<string, (string Message, int Count)> _BundledMsgs;

        public QLoggerBase(bool isDebug = false)
        {
            AssemblyObject = Assembly.GetCallingAssembly() ?? throw new ArgumentNullException("QLogger: Failed to find calling assembly");
            IsDebug = isDebug;
            _BundledMsgs = new();
        }

        public abstract void Debug(string message, string code = "");
        public abstract void Debug(Exception exception, string code = "");
        public abstract void Info(string message, string code = "");
        public abstract void Info(Exception exception, string code = "");
        public abstract void Warning(string message, string code = "");
        public abstract void Warning(Exception exception, string code = "");
        public abstract void Error(string message, string code = "");
        public abstract void Error(Exception exception, string code = "");

        #region Bundled messages
        public void Bundle(string key, string msg)
        {
            if (!_BundledMsgs.ContainsKey(key))
            {
                _BundledMsgs.Add(key, ("", 0));
            }

            (string prevMsg, int count) = _BundledMsgs[key];
            if (prevMsg.Equals(msg))
            {
                count++;
            }
            else
            {
                OutputBundleFooter(key, prevMsg, count);
                Debug($"*{key}* {msg}");
                prevMsg = msg;
                count = 1;
            }

            _BundledMsgs[key] = (prevMsg, count);
        }

        public void FlushBundles()
        {
            foreach ((string key, (string prevMsg, int count)) in _BundledMsgs)
            {
                OutputBundleFooter(key, prevMsg, count);
            }
            _BundledMsgs.Clear();
        }

        private void OutputBundleFooter(string key, string prevMsg, int count)
        {
            if (count > 1)
            {
                Debug($"*{key}* Repeated {count} times{(prevMsg.Length < 40 ? $": \"{prevMsg}\"" : "")}");
            }
        }
        #endregion


        public static string GetFormattedTimeNow()
        {
            string timezone = "Unknown";
            try
            {
                TimeSpan ts = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
                timezone = string.Format("{0}:{1:D2}", ts.Hours, ts.Minutes);
                if (ts.Hours > 0 || (ts.Hours == 0 && ts.Minutes > 0))
                {
                    timezone = "+" + timezone;
                }
            }
            catch (Exception) { }

            return DateTime.UtcNow.ToString(new CultureInfo("en-GB")) + $" ({timezone})";
        }
    }
}
