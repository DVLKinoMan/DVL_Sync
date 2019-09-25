using DVL_Sync_FileEventsLogger.Implementations;
using DVL_Sync_FileEventsLogger.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Extensions;
using System.IO;

namespace DVL_Sync.Extensions
{
    public static class FolderConfigExts
    {
        public static IEnumerable<string> GetFolderConfigJsonLogs(this FolderConfig folderConfig)
        {
            foreach (var filePath in Directory.GetFiles(folderConfig.FolderPath))
                if (folderConfig.IsLogFileWithLogName(filePath, Constants.JsonLogFileName))
                    yield return filePath;
        }

        public static void CreateRestorePoint(this FolderConfig folderConfig, string restorePointDirectoryPath, ILogger logger)
        {
            logger.LogDebug("Creating restore point for {folderConfig}", folderConfig.FolderPath);
            foreach (var filePath in folderConfig.GetFolderConfigJsonLogs())
            {
                File.Copy(filePath, Path.Combine(restorePointDirectoryPath, Path.GetFileName(filePath)));
                logger.LogDebug("Copyed {filePath} to {restorePointDirectoryPath}", filePath, restorePointDirectoryPath);
            }
            folderConfig.RemoveJsonLogContents();
            logger.LogDebug("Removed json log file contents successfully");
        }

        public static void RemoveJsonLogs(this FolderConfig folderConfig)
        {
            foreach (var filePath in folderConfig.GetFolderConfigJsonLogs())
                File.Delete(filePath);
        }

        public static void RemoveJsonLogContents(this FolderConfig folderConfig)
        {
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
