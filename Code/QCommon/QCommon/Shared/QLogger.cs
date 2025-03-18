using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using Colossal.Logging;

namespace QCommonLib
{
    // Log file location %AppData%\..\LocalLow\Colossal Order\Cities Skylines II\Logs\

    /// <summary>
    /// Static log for quick debugging, goes to {AssemblyName}_debug.log
    /// </summary>
    public static class QLog
    {
        /// <summary>
        /// The debug logger object
        /// </summary>
        private static QLoggerCustom _Instance;

        internal static void Init(bool isDebug)
        {
            _Instance = new(isDebug);
        }

        #region Redirect to instance
        /// <summary>
        /// Set whether to save debug messages
        /// </summary>
        public static bool IsDebug
        {
            get => (_Instance is not null) && _Instance.IsDebug;
            set
            {
                if (_Instance is not null) _Instance.IsDebug = value;
            }
        }

        public static void Bundle(string key, string message)
        {
            _Instance?.Bundle(key, message);
        }

        public static void FlushBundle()
        {
            _Instance?.FlushBundles();
        }

        public static void Debug(string message, string code = "")
        {
            _Instance?.Debug(message, code);
        }

        public static void Debug(Exception exception, string code = "")
        {
            _Instance?.Debug(exception, code);
        }

        public static void Info(string message, string code = "")
        {
            _Instance?.Info(message, code);
        }

        public static void Info(Exception exception, string code = "")
        {
            _Instance?.Info(exception, code);
        }

        public static void Warning(string message, string code = "")
        {
            _Instance?.Warning(message, code);
        }

        public static void Warning(Exception exception, string code = "")
        {
            _Instance?.Warning(exception, code);
        }

        public static void Error(string message, string code = "")
        {
            _Instance?.Error(message, code);
        }

        public static void Error(Exception exception, string code = "")
        {
            _Instance?.Error(exception, code);
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

        private string _IntroMessage;
        private bool _HasLogged = false;

        /// <summary>
        /// Create QDebug instance
        /// </summary>
        /// <param name="isDebug">Override should debug messages be logged?</param>
        public QLoggerCustom(bool isDebug = true) : base(isDebug)
        {
            Timer = Stopwatch.StartNew();
            string fileNameBase = Path.Combine(Path.GetDirectoryName(UnityEngine.Application.consoleLogPath) ?? throw new InvalidOperationException(), "Logs", AssemblyName + "_debug");
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
                long ticks = Timer.ElapsedTicks;
                var msg = string.Empty;
                if (code != string.Empty) code += " ";
                var frameCount = string.Empty;
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
                var fracStr = fraction.ToString();
                var timeStr = $"{secs:n0}.{fracStr.Substring(0, Math.Min(fracStr.Length, 3))}";
                msg += $"[{logLevel}|{timeStr}{frameCount}] {code}{message}{NL}";

                lock (LogFile)
                {
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
        private ILog _Logger { get; set; }
        private readonly bool _MirrorToStatic;
        private readonly bool _IsBetaBuild;

        public QLoggerCO(bool isDebug = true, string filename = "", bool mirrorToStatic = true, bool isBetaBuild = false) : base(isDebug)
        {
            _MirrorToStatic = mirrorToStatic;
            _IsBetaBuild = isBetaBuild;

            filename = filename.Equals(string.Empty) ? AssemblyName : filename;
            string fileNameBase = Path.Combine(Path.GetDirectoryName(UnityEngine.Application.consoleLogPath) ?? throw new InvalidOperationException(), "Logs", filename);
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

            _Logger = LogManager.GetLogger(filename);
            _Logger.SetEffectiveness(Level.All);

            AssemblyName details = AssemblyObject.GetName();
            _Logger.Info($"{details.Name} v{details.Version}{(_IsBetaBuild ? "-beta" : "")} at {GetFormattedTimeNow()}");
        }

        public void Shutdown()
        {
            _Logger.Info($"{AssemblyName} closing at {GetFormattedTimeNow()}");
        }

        #region Debug
        public override void Debug(string message, string code = "")
        {
            if (IsDebug)
            { 
                if (_MirrorToStatic) QLog.Debug(message, code);
                if (code != string.Empty) code += " ";
                _Logger.Debug(code + message);
            }
        }

        public override void Debug(Exception exception, string code = "")
        {
            if (IsDebug)
            {
                if (_MirrorToStatic) QLog.Debug(exception, code);
                _Logger.Debug(exception, code);
            }
        }
        #endregion

        #region Info
        public override void Info(string message, string code = "")
        {
            if (_MirrorToStatic) QLog.Info(message, code);
            if (code != string.Empty) code += " ";
            _Logger.Info(code + message);
        }

        public override void Info(Exception exception, string code = "")
        {
            if (_MirrorToStatic) QLog.Info(exception, code);
            _Logger.Info(exception, code);
        }
        #endregion

        #region Warning
        public override void Warning(string message, string code = "")
        {
            if (_MirrorToStatic) QLog.Warning(message, code);
            if (code != string.Empty) code += " ";
            _Logger.Warn(code + message);
        }

        public override void Warning(Exception exception, string code = "")
        {
            if (_MirrorToStatic) QLog.Warning(exception, code);
            _Logger.Warn(exception, code);
        }
        #endregion

        #region Error
        public override void Error(string message, string code = "")
        {
            if (_MirrorToStatic) QLog.Error(message, code);
            if (code != string.Empty) code += " ";
            _Logger.Error(code + message + NL + new StackTrace().ToString() + NL);
        }

        public override void Error(Exception exception, string code = "")
        {
            if (_MirrorToStatic) QLog.Error(exception, code);
            if (code != string.Empty) code += " ";
            string message = exception.ToString();
            if (!(exception.StackTrace is null || exception.StackTrace == string.Empty)) message += NL + new StackTrace().ToString();
            _Logger.Error(code + message + NL);
        }
        #endregion
    }

    public abstract class QLoggerBase
    {
        /// <summary>
        /// The calling assembly
        /// </summary>
        internal Assembly AssemblyObject { get; set; }
        internal string AssemblyName => AssemblyObject.GetName().Name;

        /// <summary>
        /// NewLine for the player's environment
        /// </summary>
        internal readonly string NL = Environment.NewLine;
        /// <summary>
        /// Should debug messages be logged?
        /// </summary>
        public bool IsDebug { get; set; }
        /// <summary>
        /// Storage for bundled messages
        /// </summary>
        private readonly Dictionary<string, (string Message, int Count)> _BundledMsgs;

        protected QLoggerBase(bool isDebug = false)
        {
            AssemblyObject = Assembly.GetCallingAssembly();
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
            string timezone = "-Unknown";
            try
            {
                TimeSpan ts = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
                timezone = $"{ts.Hours}:{ts.Minutes:D2}";
                if (ts.Hours > 0 || (ts.Hours == 0 && ts.Minutes > 0))
                {
                    timezone = "+" + timezone;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return DateTime.UtcNow.ToString(new CultureInfo("en-GB")) + $" ({DateTime.Now:HH:mm:ss}, UTC{timezone})";
        }
    }
}
