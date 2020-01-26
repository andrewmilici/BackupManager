using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Linq;
using FluentFTP;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BackupManager
{
    public class BackupManager
    {
        private readonly IConfiguration _configuration;
        private readonly Logger _logger;

        public BackupManager(IConfiguration configuration, Logger logger)
        {
            this._configuration = configuration;
            this._logger = logger;
        }

        private Settings GetSettings()
        {
            var retVal = new Settings();
            retVal.Hostname = _configuration["FTPHostname"];
            retVal.Port = Convert.ToInt32(_configuration["FTPPort"]);
            retVal.Username = _configuration["FTPUsername"];
            retVal.Password = _configuration["FTPPassword"];
            retVal.RemotePath = _configuration["RemotePath"];
            retVal.LocalPath = _configuration["LocalPath"];
            retVal.CompareDateTime = Convert.ToBoolean(_configuration["CompareDateTime"]);
            retVal.CompareFileSize = Convert.ToBoolean(_configuration["CompareFileSize"]);
            retVal.EmailNotification = Convert.ToBoolean(_configuration["EmailNotification"]);
            return retVal;
        }

        public void Run()
        {
            _logger.InitLog();
            var settings = GetSettings();

            using (var client = new FtpClient(settings.Hostname))
            {
                client.Port = settings.Port;
                client.Credentials = new NetworkCredential(settings.Username, settings.Password);
                _logger.AddLog($"Connecting to remote server {settings.Hostname}:{settings.Port}");
                client.Connect();
                _logger.AddLog($"Successfully connected to remote server {settings.Hostname}:{settings.Port}");

                var files = client.GetListing(settings.RemotePath, FtpListOption.Modify);
                
                if (files.Length == 0)
                {
                    _logger.AddLog("No remote files found");
                }
                else
                {
                    var fileList = new List<FtpListItem>();
                    fileList.AddRange(files); //CONVERTING TO LIST TO MAKE IT EASY TO SEARCH LATER

                    _logger.AddLog("Found " + files.Length + " remote file" + (files.Length > 1 ? "s" : ""));

                    foreach (FtpListItem item in fileList)
                    {
                        if (item.Type != FtpFileSystemObjectType.File)
                            continue;

                        var downloadFile = false;

                        var localFile = Path.Combine(settings.LocalPath, item.Name);

                        if (!File.Exists(localFile))
                        {
                            _logger.AddLog($"NEW FILE DETECTED: Downloading { item.Name }");
                            downloadFile = true;
                        }
                        else
                        {
                            var localFileInfo = new FileInfo(localFile);
                            if (settings.CompareDateTime)
                            {
                                if (item.Modified > localFileInfo.LastWriteTime)
                                    downloadFile = true;
                            }
                            if (settings.CompareFileSize)
                            {
                                if (item.Size != localFileInfo.Length)
                                    downloadFile = true;
                            }

                            if (downloadFile)
                                _logger.AddLog($"FILE CHANGE DETECTED: Downloading { item.Name }");
                            else
                                _logger.AddLog($"SKIPPING SAME FILE: {item.Name}");

                        }

                        if (downloadFile)
                        {
                            client.DownloadFile(localFile, item.FullName, FtpLocalExists.Overwrite, FluentFTP.FtpVerify.Retry, null);
                            SetLastModifiedTime(localFile, item.Modified); //NEED TO DO THIS TO MIMIC FTP SERVER MODIFIED DATE, NOT LOCAL WRITE DATE
                            _logger.AddLog($"Completed download { item.Name }");
                        }
                    }

                    var localFileList = Directory.GetFiles(settings.LocalPath);
                    foreach (var localFile in localFileList)
                    {
                        if (!fileList.Select(m => m.Name).Contains(Path.GetFileName(localFile))){
                            _logger.AddLog($"EXTRA LOCAL FILE { Path.GetFileName(localFile) } deleted");
                            File.Delete(localFile);
                        }
                    }



                }
                               
                _logger.AddLog($"Backup Completed");

                if (settings.EmailNotification)
                    new EmailHelper(_configuration, _logger).SendEmailNotification();



                //NEED TO ALSO CHECK LOCAL FILES
            }
        }

        private void SetLastModifiedTime(string localFile, DateTime newDate)
        {
            var fi = new FileInfo(localFile);
            fi.LastWriteTime = newDate;

        }


    }
}
