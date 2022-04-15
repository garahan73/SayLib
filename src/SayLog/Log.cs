using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.File;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Say32.Logging
{
    public class SayLog : IDisposable
    {
        public SayLog()
        {            
            RegisterPrinter(MemoryLogPrinter.KEY, MemoryLogPrinter);
            DisablePrinter(MemoryLogPrinter.KEY);

            MemoryLogPrinter.Updated += ()=> LogUpdated?.Invoke();
        }

        public bool Enabled { get; set; } = true;

        private object _logQueueLock = new object();

        private object _printerLock = new object();

        private object _printLock = new object();

        private List<LogItem> _logQueue = new List<LogItem>();
        private Dictionary<string, ILogPrinter> _printers = new Dictionary<string, ILogPrinter>();
        private ThreadSafeList<string> _enabledPrinterKeys = new ThreadSafeList<string>();

        public bool EnableConsole { get; set; }
        public bool EnableDebug { get; set; }

        public bool EnableMemoryLog
        {
            get => _enableMemoryLog;
            set
            {
                _enableMemoryLog = value;
                if (_enableMemoryLog)
                    EnablePrinter(MemoryLogPrinter.KEY);
                else
                    DisablePrinter(MemoryLogPrinter.KEY);
            }
        }

        public int MemoryLogLevel
        {
            get => MemoryLogPrinter.Level;
            set => MemoryLogPrinter.Level = value;
        }

        public MemoryLogPrinter MemoryLogPrinter { get; } = new MemoryLogPrinter();

        public string RecentLog
        {
            get
            {
                if (!EnableMemoryLog)
                    return "[ILLEGAL LOGGING ACCESS]-- Memory log is not enabled";
                else
                {
                    //Flush();
                    return MemoryLogPrinter.RecentLog;
                }
            }
        }

        public event Action? LogUpdated;

        public bool CanLog => Enabled && (EnableConsole || EnableDebug || _hasEnabledCustomPrinter);

        private bool _hasEnabledCustomPrinter = false;

        

        private string _indent = "";
        private bool _enableMemoryLog;

        public void RegisterPrinter( string printerKey, ILogPrinter printer )
        {
            lock (_printerLock)
            {
                _printers.Add(printerKey, printer);
                _enabledPrinterKeys.Add(printerKey);
                _hasEnabledCustomPrinter = true;
            }
        }

        public void EnablePrinter( string printerKey )
        {
            lock (_printerLock)
            {
                if (!_enabledPrinterKeys.Contains(printerKey))
                    _enabledPrinterKeys.Add(printerKey);

                _hasEnabledCustomPrinter = true;
            }
        }

        public void DisablePrinter( string printerKey )
        {
            lock (_printerLock)
            {
                _enabledPrinterKeys.Remove(printerKey);

                _hasEnabledCustomPrinter = _enabledPrinterKeys.Count != 0;
            }
        }

        public void WriteLog( object message, int level = 0 )
        {
            if (!CanLog) return;

            WriteLog(o => o?.ToString() ?? "", message, level);
        }

        public void WriteLog( Func<object?, string> formatMessage, LazyValue<object>? messageBody, int level = 0 )
        {
            if (!CanLog) return;

            var logItem = new LogItem(_indent, formatMessage, messageBody, level);
            lock (_printerLock)
            {
                logItem.EnabledPrinters = _enabledPrinterKeys.Where(k => _printers.ContainsKey(k)).Select(k => _printers[k]).ToArray();
            }

            lock (_logQueueLock)
            {
                _logQueue.Add(logItem);
            }

            Task.Run(PrintItemsInQueue);
        }


        private void PrintItemsInQueue()
        {
            lock (_printLock)
            {
                var items = _logQueueLock.Lock(() =>
                {
                    if (_logQueue.Count == 0)
                        return null;

                    var list = _logQueue;
                    _logQueue = new List<LogItem>();
                    return list;
                });

                if (items == null || items.Count == 0) return;

                foreach (var item in items)
                {                    
                    PrintLogItem(item);
                }
            }
        }


        private void PrintLogItem( LogItem item )
        {
            //if (!CanLog) return; // it should already be checked before

            if(!CodeUtil.Try(()=>item.PrepareMessage(), out var ex))
            {
                //LogFormattingError?.Invoke(item);
                Console.WriteLine($"Log item formatting error: {ex}");
                return;
            }

            var message = item.Message;

            if (EnableConsole)
                Console.WriteLine(message);

            if (EnableDebug)
                Debug.WriteLine(message);

            if (item.EnabledPrinters == null)
                return;

            foreach (var printer in item.EnabledPrinters)
            {
                CodeUtil.SafeRun(() => printer.PrintLog(message ?? "", item.Level));
            }
        }


        public void IncreaseIndent()
        {
            if (Enabled)
                _indent += '\t';
        }

        public void DecreaseIndent()
        {
            if (Enabled)
                _indent = _indent.TrimEnd("\t");
        }

        public void Dispose()
        {
            Flush();

            IEnumerable<ILogPrinter>? printers = null;

            lock (_printerLock)
            {
                printers = _printers.Values;
                //_printers.Clear();
                //_enabledPrinterKeys.Clear();
            }

            foreach (var printer in printers)
            {
                if (printer is IDisposable disposable)
                {
                    Task.Run(() => disposable.Dispose());
                }
            }

        }

        public void Flush()
        {
            Wait.Until(() => _logQueue.Lock(() => _logQueue.Count == 0));
        }

    }

    public interface ILogPrinter
    {
        int Level { get; set; }
        void PrintLog( string message, int level );        
    }


    class LogItem
    {
        private readonly string _indent;

        public int Level { get; }

        public LogItem( string indent, Func<object?, string> formatMessage, LazyValue<object>? messageBody )
        {
            _indent = indent;
            FormatMessageMethod = formatMessage;
            MessageBody = messageBody;

            PrepareMessage();
        }

        public LogItem( string indent, Func<object?, string> formatMessage, LazyValue<object>? messageBody, int level ) : this(indent, formatMessage, messageBody)
        {
            this.Level = level;
        }

        internal void PrepareMessage()
        {
            try
            {
                var text = FormatMessageMethod.Invoke(MessageBody?.Value);
                Message = HandleMultipleLinesAndIndent(text);
            }
            catch (Exception ex)
            {
                Message = $"Error while creating log message: {ex.Message}";
            }
        }

        public LazyValue<object>? MessageBody { get; set; }
        public Func<object?, string> FormatMessageMethod { get; set; }

        public string? Message { get; private set; }
        public ILogPrinter[]? EnabledPrinters { get; internal set; }


        private string HandleMultipleLinesAndIndent( string text )
        {
            //if (text == null) return null;

            var lines = text.Split('\n');
            if (lines.Length == 1)
                return _indent + text;

            var sb = new StringBuilder();
            var i = 0;
            foreach (var line in lines)
                if (i++ == 0)
                    sb.AppendLine(_indent + line);
                else
                    sb.AppendLine($"{_indent + '\t'}{line}");

            return sb.ToString();
        }
    }

   
    public class FileLogPrinter : ILogPrinter, IDisposable
    {
        //private readonly string _path;
        private SimpleFileLogger _logger;

        //private bool _autoCloseTriggered = false;
        public string? LogPath { get; private set; }

        public int AutoCloseTimeInMilliSeconds { get; set; } = 0;
        public int Level { get; set; } = 0;

        public FileLogPrinter( string path, int maxFileSizeInMB = 10, int maxFileCount = 20)
        {
            //_path = path;
            _logger = new SimpleFileLogger(path, LogFileNameChangeOption.ChangeTimeInFileName, maxFileSizeInMB, maxFileCount);
            SerilogUtil.SetLogPath(path, p => LogPath = p);

            var dirPath = Path.GetDirectoryName(path);
            FileUtil.EnsureFolderPath(dirPath);

        }

        public void PrintLog( string message, int level )
        {
            if (level < (int)Level)
                return;

            _logger.LogLine(message);
        }

        public void Dispose()
        {
            //_logger.Dispose();
        }
    }


    public class MemoryLogPrinter : ILogPrinter
    {
        public const string KEY = "__MEMORY_LOG__";

        public int Level { get; set; } = 0;

        public event Action? Updated;

        public const int DEFAULT_MAX_LOG_SIZE = 300;
        public int MaxLogSize { get; set; } = DEFAULT_MAX_LOG_SIZE;

        private Queue<string> _lines = new Queue<string>();

        public void PrintLog( string message, int level )
        {
            if (level < (int)Level)
                return;
            
            lock (_lines)
            {
                _lines.Enqueue(message);

                while(_lines.Count > MaxLogSize)
                {
                    _lines.Dequeue();
                }
            }

            Updated?.Invoke();
            
        }

        public override string ToString() => RecentLog;

        public string RecentLog
        {
            get
            {
                var lines = _lines.Lock(() => _lines.ToArray());
                return string.Join("\r\n", lines);
            }
        }
    }

    
}
