using System;
using System.Collections.Generic;
using System.Text;

namespace BackupManager
{
    public class Logger
    {
        public List<string> LogMessages { get; set; }

        public Logger()
        {
            this.LogMessages = new List<string>();
        }

        public void InitLog()
        {
            AddLog(new string('-', 50));
            AddLog(@"Backup Manager - New Instance");
            AddLog(new string('-', 50));
        }

        public void AddLog(string logItem)
        {
            var logFile = @"C:\Users\Andrew\Desktop\log.txt";
            var outputLogText = $"{ DateTime.Now.ToString("dd/MM/yy hh:mm:ss tt") } - {logItem}";
            System.IO.File.AppendAllText(logFile, outputLogText + System.Environment.NewLine);
            Console.WriteLine(outputLogText);
            this.LogMessages.Add(outputLogText);
        }
    }

}
