using System;
using System.Collections.Generic;
using System.Text;

namespace BackupManager
{
    public class Settings
    {
        public string Hostname { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string RemotePath { get; set; }
        public string LocalPath { get; set; }
        public bool CompareDateTime { get; set; }
        public bool CompareFileSize { get; set; }
        public bool EmailNotification { get; set; }

    }
}
