using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Say32.SimpleFileLogger;

namespace Say32
{
    public class SimpleFileLoggerConfig
    {
        public SimpleFileLoggerConfig( string path, LogFileNameChangeOption fileNameChangeOption )
        {
            OriginalPath = path;
            FileNameChangeOption = fileNameChangeOption;
        }

        public LogFileNameChangeOption FileNameChangeOption {get;}

        public string OriginalPath { get; set; }
        public string DirectoryPath => Path.GetDirectoryName(OriginalPath);

        public string BasePath => GetBasePath(OriginalPath);

        public string FileExtention => Path.GetExtension(OriginalPath).TrimStart('.');

        private string GetBasePath( string path ) => Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));


        public const string DEFAULT_TIME_FORMAT = "yyMMdd-HHmmss.ffff";


        private string _timeFormat = DEFAULT_TIME_FORMAT;
        public string TimeFormat { get => _timeFormat ?? DEFAULT_TIME_FORMAT; set => _timeFormat = value; }

        public int MaxFileCount { get; set; } = 0;

        public bool KeepFirstFile { get; set; } = true;

        public double FileSizeInMB { get; set; }
        public int FileSizeInBytes => (int) (FileSizeInMB* 1000 * 1000);

        public ChangeByTimeOption ChangeByTime { get; set; } = ChangeByTimeOption.None;
        
    }

    public enum LogFileNameChangeOption
    {
        KeepFileName_AddNumberCount, ChangeTimeInFileName
    }

    public class SimpleFileLogger
    {
        public enum ChangeByTimeOption
        {
            None, PerSecond, PerMinute, PerHour, PerDay
        }

        public SimpleFileLogger( string path, LogFileNameChangeOption fileNameChangeOption,
            double fileSizeInMB = 0, int maxFileCount = 0, 
            ChangeByTimeOption changeByTimeOption = ChangeByTimeOption.None, 
            string timeFormat= SimpleFileLoggerConfig.DEFAULT_TIME_FORMAT )
            : this(new SimpleFileLoggerConfig(path, fileNameChangeOption)
            {
                FileSizeInMB = fileSizeInMB,
                MaxFileCount = maxFileCount,
                ChangeByTime = changeByTimeOption,
                TimeFormat = timeFormat
            })
        {
        }

        public SimpleFileLogger( SimpleFileLoggerConfig config )
        {
            Config = config;

            TimeBasePath = $"{Config.BasePath}_{DateTime.Now.ToString(Config.TimeFormat)}";
            CurrentPath = $"{TimeBasePath}.{Config.FileExtention}";
            FirstLogPath = CurrentPath;
        }

        public SimpleFileLoggerConfig Config { get; }

        
        public string TimeBasePath { get; internal set; }
        public string CurrentPath { get; internal set; }
        public string FirstLogPath { get; }

        public int FileIndex { get; internal set; } = 0;
        
        internal DateTime PrevLogTime = DateTime.MinValue;

        private readonly AsyncActionQueue _actionQueue = new AsyncActionQueue();

        public void LogLine( string line ) => Log(w =>
        {
            try { w.WriteLine(line); }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to log: {line}");
                Debug.WriteLine(ex);
            }
        });

        public void Log( Action<TextWriter> logAction )
        {
            new LogFileChanger(this).CreateNewLogFileIfNeeded();

            PrevLogTime = DateTime.Now;

            _actionQueue.Add(() => LogCore(logAction));
        }

        private void LogCore( Action<TextWriter> logAction )
        {   
            using (StreamWriter w = File.AppendText(CurrentPath))
            {
                logAction(w);
            }
        }


        public void Wait() => _actionQueue.Wait();

        public void DumpLog()
        {
            using (StreamReader r = File.OpenText(CurrentPath))
            {
                string line;
                while ((line = r.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                }
            }
        }
    }

    class LogFileChanger
    {
        private SimpleFileLogger _logger;

        public LogFileChanger( SimpleFileLogger simpleFileLogger ) => _logger = simpleFileLogger;

        public void CreateNewLogFileIfNeeded()
        {
            if (CreateNewFileByTime())
                return;

            CreateNewFileBySize();
        }

        private bool CreateNewFileByTime()
        {
            var cf = _logger.Config;

            var prevLogTime = _logger.PrevLogTime;

            // by time
            if (cf.ChangeByTime == ChangeByTimeOption.None) return false;
            if (prevLogTime == DateTime.MinValue) return false;

            var now = DateTime.Now;

            var change = cf.ChangeByTime switch
            {
                ChangeByTimeOption.None => false,
                ChangeByTimeOption.PerSecond => prevLogTime.Second != now.Second,
                ChangeByTimeOption.PerMinute => prevLogTime.Minute != now.Minute,
                ChangeByTimeOption.PerHour => prevLogTime.Hour != now.Hour,
                ChangeByTimeOption.PerDay => prevLogTime.Day != now.Day,
                _ => throw new Exception("This can't happen")
            };

            if (!change) return false;

            CreateNewFile(now, true);

            return true;
        }

        private void CreateNewFileBySize()
        {
            var cf = _logger.Config;

            // by size
            if (cf.FileSizeInBytes <= 0) return;

            var f = new FileInfo(_logger.CurrentPath);
            if (!f.Exists || f.Length >= cf.FileSizeInBytes)
            {
                CreateNewFile(DateTime.Now, false);
            }
        }

        private void CreateNewFile(DateTime now, bool byTimeChangeOption)
        {
            var cf = _logger.Config;

            if (cf.FileNameChangeOption == LogFileNameChangeOption.KeepFileName_AddNumberCount)
            {
                _logger.CurrentPath = $"{_logger.TimeBasePath}_{++_logger.FileIndex:D3}.{cf.FileExtention}";
            }
            else if (cf.FileNameChangeOption == LogFileNameChangeOption.ChangeTimeInFileName)
            {
                DateTime time;

                if (byTimeChangeOption)
                {
                    var unitTicks = cf.ChangeByTime switch
                    {
                        ChangeByTimeOption.PerSecond => TimeSpan.TicksPerSecond,
                        ChangeByTimeOption.PerMinute => TimeSpan.TicksPerMinute,
                        ChangeByTimeOption.PerHour => TimeSpan.TicksPerHour,
                        ChangeByTimeOption.PerDay => TimeSpan.TicksPerDay,
                        _ => throw new Exception("This can't happen")
                    };

                    time = new DateTime((long)(now.Ticks / unitTicks) * unitTicks);
                }
                else
                {
                    time = now;
                }

                _logger.TimeBasePath = $"{cf.BasePath}_{time.ToString(cf.TimeFormat)}";
                _logger.CurrentPath = $"{_logger.TimeBasePath}.{cf.FileExtention}";
            }

            DeleteOldFiles();
        }

        private bool _deleting = false;

        private void DeleteOldFiles()
        {
            if (_deleting) return;

            var cf = _logger.Config;

            if (cf.MaxFileCount <= 0) return;

            _ = Task.Run(() =>
            {
                if (_deleting) return;

                _deleting = true;

                try
                {
                    // time is always same
                    if (cf.FileNameChangeOption == LogFileNameChangeOption.KeepFileName_AddNumberCount)
                    {
                        var firstFileNameWithTime = Path.GetFileNameWithoutExtension(_logger.FirstLogPath);
                        var fileList = Directory.GetFiles(cf.DirectoryPath, $"{firstFileNameWithTime}*.{cf.FileExtention}").OrderBy(f => f).ToList();
                        DeleteFiles(cf, fileList);

                    }
                    else // time changes
                    {
                        var baseFileName = Path.GetFileNameWithoutExtension(cf.BasePath);
                        var fileList = Directory.GetFiles(cf.DirectoryPath, $"{baseFileName}*.{cf.FileExtention}").OrderBy(f => f).ToList();
                        DeleteFiles(cf, fileList);
                    }
                }
                catch { }
                finally
                {
                    _deleting = false;
                }

            });
        }

        private void DeleteFiles( SimpleFileLoggerConfig cf, List<string> fileList)
        {
            if (fileList.Count <= cf.MaxFileCount) return;

            var fileArray = fileList.ToArray();

            foreach (var file in fileArray)
            {
                if (file == _logger.FirstLogPath && cf.KeepFirstFile) continue;

                Run.Safely(() =>
                {
                    File.Delete(file);
                    fileList.Remove(file);
                });

                if (fileList.Count <= cf.MaxFileCount) return;
            }
            return;
        }
    }
}
