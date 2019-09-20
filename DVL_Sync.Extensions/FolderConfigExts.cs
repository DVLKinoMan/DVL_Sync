using DVL_Sync_FileEventsLogger.Implementations;
using DVL_Sync_FileEventsLogger.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
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
            folderConfig.RemoveJsonLogs();
            logger.LogDebug("Removed json log files successfully");
        }

        public static void RemoveJsonLogs(this FolderConfig folderConfig)
        {
            foreach (var filePath in folderConfig.GetFolderConfigJsonLogs())
                File.Delete(filePath);
        }
    }
}
