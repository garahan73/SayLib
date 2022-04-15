using Microsoft.VisualStudio.TestTools.UnitTesting;
using Say32.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Say32
{
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class LogTest
    {
        [Ignore]
        [TestMethod]
        public void LogFilePathTest()
        {
            var logger = new SayLog();
            var fileLogPrinter = new FileLogPrinter("c:/tmp/test_.log", 1, 2);
            logger.RegisterPrinter("file", fileLogPrinter);

            logger.WriteLog("test1");
            Thread.Sleep(100);
            Console.WriteLine($"log path = '{fileLogPrinter.LogPath}'");
            Assert.IsTrue(fileLogPrinter.LogPath.StartsWith("c:\\tmp\\test_"));
        }

        [TestMethod]
        public void SimpleFileLoggerTest()
        {
            const string path = "c:/tmp/test.log";

            Directory.GetFiles("c:/tmp", "test*.log")
                .ForEach(f => Run.Safely(()=> File.Delete(f)));

            var logger = new SimpleFileLogger(path, LogFileNameChangeOption.KeepFileName_AddNumberCount, 0.1, 0, SimpleFileLogger.ChangeByTimeOption.PerSecond);

            for (int i = 0; i < 10000; i++)
            {
                var j = i;
                logger.Log(w=> Log($"Test{j}", w));
            }

            logger.Wait();
        }

        public static void Log( string logMessage, TextWriter w )
        {
            w.Write("\r\nLog Entry : ");
            w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
            w.WriteLine("  :");
            w.WriteLine($"  :{logMessage}");
            w.WriteLine("-------------------------------");
        }
    }
}
