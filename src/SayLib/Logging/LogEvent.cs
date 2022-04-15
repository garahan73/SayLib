using System;
using System.Diagnostics;
using System.Reflection;


namespace Say32.Logging
{
    public enum LogLevel { ERROR, WARNING, INFO, VERBOSE, DEBUG }


    public class LogEvent
    {
        #region FACTORY

        public static LogEvent Info(StackFrame stackFrame, string message, string detail, object? properties = null)
        {
            return new LogEvent(LogLevel.INFO, stackFrame, message, detail, null, properties);
        }

        public static LogEvent Warning(StackFrame stackFrame, string message, string detail, Exception? exception = null, object? properties = null)
        {
            return new LogEvent(LogLevel.WARNING, stackFrame, message, detail, exception, properties);
        }

        public static LogEvent Error(StackFrame stackFrame, Exception exception, string message, string? detail = null, object? properties = null)
        {
            return new LogEvent(LogLevel.ERROR, stackFrame, message, detail, exception, properties);
        }

        #endregion

        public LogEvent(LogLevel type, StackFrame stackFrame, string message, string? detail, Exception? exception, object? properties)
        {
            Time = DateTime.Now;
            Level = type;

            _stackFrame = stackFrame;

            Message = message;

            Detail = detail;

            Properties = properties;

            /*
            if (Detail == null && exception != null)
                Detail = $"{exception.GetType().Name}, {exception.Message}";
                */

            if (exception != null)
            {
                Exception = exception;
            }
        }

        private const string SRC_ROOT = "\\src";

        private string? GetSourceFile(StackFrame stackFrame)
        {
            var fileName = stackFrame.GetFileName();
            if (fileName == null)
            {
                return null;
            }

            if (!fileName.Contains(SRC_ROOT))
            {
                return fileName;
            }

            return fileName.Split(new string[] { SRC_ROOT }, 2, StringSplitOptions.None)[1];
        }

        private readonly StackFrame _stackFrame;

        public LogLevel Level { get; private set; }

        public string? SrcFile => GetSourceFile(_stackFrame);
        public int SrcLineNo => _stackFrame.GetFileLineNumber();
        public MethodBase SrcMethod => _stackFrame.GetMethod();

        //public DateTime Date => Time.Date;
        public DateTime Time { get; private set; }

        //public string Module { get; }
        //public string CodeLetters { get; }
        //public string Code => $"{Module}-{CodeLetters}";

        public virtual string Message { get; }
        public string? Detail { get; }
        public Exception? Exception { get; }
        public string? ExceptionSummary => Exception == null ? null : $"{Exception.GetType().Name}, {Exception.Message}";

        public object? Properties { get; }


    }
}
