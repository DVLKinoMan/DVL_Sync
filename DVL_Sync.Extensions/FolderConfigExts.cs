using DVL_Sync_FileEventsLogger.Implementations;
using DVL_Sync_FileEventsLogger.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Extensions;
using System.IO;
using System.Linq;
using System.Threading;

namespace DVL_Sync.Extensions
{
    public static class FolderConfigExts
    {
        public static IEnumerable<string> GetFolderConfigJsonLogs(this FolderConfig folderConfig) =>
            Directory.GetFiles(folderConfig.FolderPath).Where(filePath =>
                folderConfig.IsLogFileWithLogName(filePath, Constants.JsonLogFileName));

        /// <summary>
        /// Working on json files only
        /// </summary>
        /// <param name="folderConfig"></param>
        /// <param name="restorePointDirectoryPath"></param>
        /// <param name="logger"></param>
        public static void CreateRestorePoint(this FolderConfig folderConfig, string restorePointDirectoryPath, ILogger logger)
        {
            logger.LogDebug("Creating restore point for {folderConfig}", folderConfig.FolderPath);
            foreach (var filePath in folderConfig.GetFolderConfigJsonLogs())
            {
                string path = Path.Combine(restorePointDirectoryPath, Path.GetFileName(filePath));
                string destinationFilePath = path.Substring(0, path.Length - 5);
                int i = 1;
                while (File.Exists($"{ destinationFilePath }.json"))
                    destinationFilePath = $"{ path } { i++ }";
                File.Copy(filePath, $"{ destinationFilePath }.json");
                logger.LogDebug("Copied {filePath} to {restorePointDirectoryPath}", filePath, restorePointDirectoryPath);
            }
            folderConfig.RemoveJsonLogContents();
            logger.LogDebug("Removed json log file contents successfully");
        }

        public static void RemoveJsonLogs(this FolderConfig folderConfig)
        {
            Thread.Sleep(1000);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            foreach (var filePath in folderConfig.GetFolderConfigJsonLogs())
                File.Delete(filePath);
        }

        public static void RemoveJsonLogContents(this FolderConfig folderConfig)
        {
            Thread.Sleep(1000);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            foreach (var filePath in folderConfig.GetFolderConfigJsonLogs())
            {
                if (Path.GetFileName(filePath).GetCustomDateTime() < DateTime.Now.Date)
                    File.Delete(filePath);
                else
                {
                    var fileStream = File.Open(filePath, FileMode.Open);
                    fileStream.SetLength(0);
                    fileStream.Close();
                }
            }
        }
    }
}
