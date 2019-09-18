using Microsoft.VisualStudio.TestTools.UnitTesting;
using DVL_Sync.Implementations;
using DVL_Sync_FileEventsLogger.Models;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;

namespace DVL_Sync.Tester
{
    [TestClass]
    public class TwoFoldersSyncerTester
    {
        private string folderPath1 = @"D:\DVL_Sync_WatcherTestingFile";
        private string folderPath2 = @"D:\DVL_Sync_WatcherTestingFile2";

        [TestMethod]
        public void TestMethod1()
        {
            var folderOperationEventsReader = new FolderOperationEventsReaderFromJsonFile();
            var foldersSyncReader = new FoldersSyncReaderFromJsonFile();
            var sync = new FoldersSyncer(folderOperationEventsReader, foldersSyncReader);
            var config1 = new FolderConfig
            {
                FolderPath = folderPath1,
                IncludeSubDirectories = true,
                WatchHiddenFiles = false
            };
            var config2 = new FolderConfig
            {
                FolderPath = folderPath2,
                IncludeSubDirectories = true,
                WatchHiddenFiles = false
            };

            sync.SyncFolders(config1, config2);

            var dir1 = new DirectoryInfo(config1.FolderPath);
            var dir2 = new DirectoryInfo(config2.FolderPath);

            Assert.AreEqual(true, TwoFoldersAreIdentical(dir1, dir2));
        }

        public static bool TwoFoldersAreIdentical(DirectoryInfo dir1, DirectoryInfo dir2)
        {
            var filteredFiles1 = GetFilteredFilesFromDir(dir1).ToList();
            var filteredFiles2 = GetFilteredFilesFromDir(dir2).ToList();

            if (filteredFiles1.Count != filteredFiles2.Count)
                return false;

            foreach (var filterFile in filteredFiles1)
            {
                var fl = filteredFiles2.FirstOrDefault(fltFile => fltFile.Name == filterFile.Name);
                if (fl == null || !FilesAreEqual(fl, filterFile))
                    return false;
            }

            var dir1Directories = GetFilteredDirectoriesFromDir(dir1).ToList();
            var dir2Directories = GetFilteredDirectoriesFromDir(dir2).ToList();

            if (dir1Directories.Count != dir2Directories.Count)
                return false;

            foreach (var directory1 in dir1Directories)
            {
                var directory2 = dir2Directories.FirstOrDefault(dir => dir.Name == directory1.Name);
                if (directory2 == null || !TwoFoldersAreIdentical(directory2, directory1))
                    return false;
            }

            return true;

            IEnumerable<FileInfo> GetFilteredFilesFromDir(DirectoryInfo directory)
            {
                var files = directory.GetFiles();
                foreach (var file in files.Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden)))
                    yield return file;
            }

            IEnumerable<DirectoryInfo> GetFilteredDirectoriesFromDir(DirectoryInfo directory)
            {
                var directories = directory.GetDirectories();
                foreach (var dir in directories.Where(dir => !dir.Attributes.HasFlag(FileAttributes.Hidden)))
                    yield return dir;
            }
        }

        public static bool FilesAreEqual(FileInfo first, FileInfo second) =>
            first.Length == second.Length && string.Equals(first.Name, second.Name, StringComparison.OrdinalIgnoreCase);

    }
}
