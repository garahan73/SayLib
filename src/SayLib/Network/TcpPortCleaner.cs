using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Say32.Network
{
    class PRC
    {
        public int PID { get; set; }
        public int Port { get; set; }
        public string? Protocol { get; set; }
    }

    public class TcpPortCleaner
    {
        private readonly List<PRC> _processes;

        public TcpPortCleaner()
        {
            _processes = GetAllProcesses();
        }

        public void KillProcesses(int port)
        {            
            if (_processes.Any(p => p.Port == port))
            {
                var currentProc = Process.GetCurrentProcess();

                foreach (var prc in _processes.Where(p => p.Port == port))
                {
                    try
                    {
                        if (prc.PID == currentProc.Id)
                            continue;

                        var process = Process.GetProcessById(prc.PID);

                        if (process.ProcessName.ToLower().StartsWith("idle"))
                            continue;

                        Console.WriteLine($"Killing process '{process.ProcessName}' using TCP port {port}");
                        process.Kill();
                    }

                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed killing process using port {port}");
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            else
            {
                Console.WriteLine("No process to kill!");
            }
        }


        private static List<PRC> GetAllProcesses()
        {
            var pStartInfo = new ProcessStartInfo();
            pStartInfo.FileName = "netstat.exe";
            pStartInfo.Arguments = "-a -n -o";
            pStartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            pStartInfo.UseShellExecute = false;
            pStartInfo.RedirectStandardInput = true;
            pStartInfo.RedirectStandardOutput = true;
            pStartInfo.RedirectStandardError = true;

            var process = new Process()
            {
                StartInfo = pStartInfo
            };
            process.Start();

            var soStream = process.StandardOutput;

            var output = soStream.ReadToEnd();
            if (process.ExitCode != 0)
                throw new Exception("somethign broke");

            var result = new List<PRC>();

            var lines = Regex.Split(output, "\r\n");
            foreach (var line in lines)
            {
                if (line.Trim().StartsWith("Proto") || line.Trim().StartsWith("프로토콜"))
                    continue;

                var parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                var len = parts.Length;
                if (len > 2)
                {
                    //Console.WriteLine(line);
                    result.Add(new PRC
                    {
                        Protocol = parts[0],
                        Port = int.Parse(parts[1].Split(':').Last()),
                        PID = int.Parse(parts[len - 1])
                    });
                }

            }
            return result;
        }
    }
}

