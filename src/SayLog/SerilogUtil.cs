using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.File;
using System;
using System.IO;

namespace Say32.Logging
{
    public static class SerilogUtil
    {
        public static Logger CreateAsyncLogger( bool showTime, string filePath, LogEventLevel minimumLevel, int maxFileSizeInMB, int maxFileCount, bool consoleOutput = false )
        {
            var conf = new LoggerConfiguration()
                        //.WriteTo.ColoredConsole(outputTemplate: "{Message:l}{NewLine}{Exception}")
                        .WriteTo.Async(
                            conf => conf.File(
                                filePath,
                                restrictedToMinimumLevel: minimumLevel,
                                rollingInterval: RollingInterval.Day,
                                rollOnFileSizeLimit: true,
                                addTimeInfo: true,
                                fileSizeLimitBytes: maxFileSizeInMB * 1024 * 1024,
                                retainedFileCountLimit: maxFileCount == 0 ? (int?)null : maxFileCount,
                                //outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u4}] {Message:lj}-{Properties:j}{NewLine}{Exception}"
                                outputTemplate: showTime ? "{Timestamp:yyyy-MM-dd HH:mm:ss.ffff} {Message:l}{NewLine}" : "{Message:l}{NewLine}"
                            ));

            if (consoleOutput)
                conf.WriteTo.Console();

            return conf.CreateLogger();
        }

        public static LogLevel ToLogLevel( this LogEventLevel level ) => level switch
        {
            LogEventLevel.Fatal => LogLevel.ERROR,
            LogEventLevel.Error => LogLevel.ERROR,
            LogEventLevel.Warning => LogLevel.WARNING,
            LogEventLevel.Information => LogLevel.INFO,
            LogEventLevel.Debug => LogLevel.DEBUG,
            LogEventLevel.Verbose => LogLevel.VERBOSE,
            _ => throw new System.NotImplementedException(),
        };

        public static LogEventLevel ToLogEventLevel( this LogLevel level ) => level switch
        {

            LogLevel.ERROR => LogEventLevel.Error,
            LogLevel.WARNING => LogEventLevel.Warning,
            LogLevel.INFO => LogEventLevel.Information,
            LogLevel.DEBUG => LogEventLevel.Debug,
            LogLevel.VERBOSE => LogEventLevel.Verbose,
            _ => throw new System.NotImplementedException(),
            
        };

        public static void SetLogPath( string path, Action<string> setPath )
        {
            SerilogEvents.LogFileCreated += handleLogEvent;

            void handleLogEvent( LogEventArgs args )
            {
                var fileName = Path.GetFileNameWithoutExtension(path);

                if (args.Name == fileName && args.Sequence == 0)
                {
                    //SerilogEvents.LogFileCreated -= handleLogEvent;
                    setPath(args.Path);
                }
            }
        }
    }


}
