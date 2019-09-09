using DVL_Sync.Extensions;
using DVL_Sync.Models;
using DVL_Sync_FileEventsLogger.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DVL_Sync.Tester
{
    [TestClass]
    public class OperationExtsTester
    {
        #region GetRootPath
        private OperationEvent CreateOperationEventWithFilePath(string filePath) => new CreateOperationEvent
        {
            FilePath = filePath
        };

        private IEnumerable<OperationEvent> CreateManyOperationEventsWithFilePaths(params string[] filePaths)
        {
            foreach (var filePath in filePaths)
                yield return CreateOperationEventWithFilePath(filePath);
        }

        [TestMethod]
        public void GetRootPathTester1()
        {
            var list = new List<OperationEvent>();
            list.AddRange(CreateManyOperationEventsWithFilePaths(
                @"D:\DVL_Sync_WatcherTestingFile\SomeFolder1\inside.txt", 
                @"D:\DVL_Sync_WatcherTestingFile\SomeFolder2\insideFolder\some.txt", 
                @"D:\DVL_Sync_WatcherTestingFile\SomeFolder1",
                @"D:\DVL_Sync_WatcherTestingFile\SomeFolder1\insideFolder"
            ));

            Assert.AreEqual(@"D:\DVL_Sync_WatcherTestingFile", list.GetRootPath());
        }

        [TestMethod]
        public void GetRootPathTester2()
        {
            var list = new List<OperationEvent>();
            list.AddRange(CreateManyOperationEventsWithFilePaths(
                @"D:\DVL_Sync_WatcherTestingFile\SomeFolder1\inside.txt",
                @"D:\DVL_Sync_WatcherTestingFile\SomeFolder1\inside.txt",
                @"D:\DVL_Sync_WatcherTestingFile\SomeFolder1\inside.txt"
            ));

            string path = list.GetRootPath();
            Assert.AreEqual(@"D:\DVL_Sync_WatcherTestingFile\SomeFolder1", path);
        }

        [TestMethod]
        public void GetRootPathTester3()
        {
            var list = new List<OperationEvent>();
            list.AddRange(CreateManyOperationEventsWithFilePaths(
                @"D:\DVL_Sync_WatcherTestingFile\SomeFolder1\insideFolder",
                @"D:\DVL_Sync_WatcherTestingFile\SomeFolder1\insideFolder",
                @"D:\DVL_Sync_WatcherTestingFile\SomeFolder1\insideFolder\SomeOtherFolder"
            ));

            string path = list.GetRootPath();
            Assert.AreEqual(@"D:\DVL_Sync_WatcherTestingFile\SomeFolder1\insideFolder", path);
        }

        [TestMethod]
        public void GetRootPathTester4()
        {
            var list = new List<OperationEvent>();
            list.AddRange(CreateManyOperationEventsWithFilePaths(
                @"D:\DVL_Sync_WatcherTestingFile\SomeFolder1\insideFolder",
                @"C:\",
                @"D:\DVL_Sync_WatcherTestingFile\SomeFolder1\insideFolder"
            ));

            string path = list.GetRootPath();
            Assert.AreEqual(@"", path);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "Was expected InvalidOperationException")]
        public void GetRootPathTester5()
        {
            var list = new List<OperationEvent>();
            list.GetRootPath();
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException), "Was expected NullReferenceException")]
        public void GetRootPathTester6()
        {
            List<OperationEvent> list = null;
            list.GetRootPath();
        }
        #endregion

        /// <summary>
        /// Creates chain of folders with folderNames
        /// </summary>
        /// <param name="folderNames"></param>
        /// <returns>root folder</returns>
        private FolderViewModel CreateChainOfFolders(params string[] folderNames)
        {
            var root = new FolderViewModel(folderNames[0]);
            var currFolder = root;
            for (int i = 1; i < folderNames.Length; i++)
            {
                var folder = new FolderViewModel(folderNames[i]);
                currFolder.Folders.Add(folder);
                currFolder = folder;
            }

            return root;
        }

        #region AddFileToRootFolder

        private bool ContainsFileInPath(FolderViewModel rootFolder, string filePath)
        {
            var splitted = filePath.Split(new[] { "\\" }, StringSplitOptions.None);
            if (rootFolder.Name != splitted[0])
                return false;

            var currFolder = rootFolder;
            for(int i = 1; i < splitted.Length - 1; i++)
            {
                var fold = currFolder.Folders.FirstOrDefault(fld => fld.Name == splitted[i]);
                if (fold == null)
                    return false;
                currFolder = fold;
            }

            return currFolder.Files.Any(fl => fl.Name == splitted[splitted.Length - 1]);
        }

        [TestMethod]
        public void AddFileToRootFolderTester1()
        {
            var rootFolder = CreateChainOfFolders("D:", "Folder1", "InsideFolder1", "InsideInsideFolder1");
            string filePath = @"D:\Folder1\InsideFolder1\InsideInsideFolder1\newTextDocument.txt";
            rootFolder.AddFileToRootFolder(new CreateOperationEvent { FilePath = filePath });
            Assert.AreEqual(true, ContainsFileInPath(rootFolder, filePath));
        }

        [TestMethod]
        public void AddFileToRootFolderTester2()
        {
            var rootFolder = CreateChainOfFolders("D:", "Folder1", "InsideFolder1", "InsideInsideFolder1");
            string filePath = @"D:\Folder1\InsideFolder1\InsideInsideFolder1\InsideInsideFolder2\newTextDocument.txt";
            rootFolder.AddFileToRootFolder(new CreateOperationEvent { FilePath = filePath });
            Assert.AreEqual(true, ContainsFileInPath(rootFolder, filePath));
        }

        [TestMethod]
        public void AddFileToRootFolderTester3()
        {
            var rootFolder = CreateChainOfFolders("D:", "Folder1", "InsideFolder1", "InsideInsideFolder1");
            string filePath = @"D:\Folder1\InsideFolder2\InsideInsideFolder1\InsideInsideFolder2\newTextDocument.txt";
            rootFolder.AddFileToRootFolder(new CreateOperationEvent { FilePath = filePath });
            Assert.AreEqual(true, ContainsFileInPath(rootFolder, filePath));
        }

        [TestMethod]
        public void AddFileToRootFolderTester4()
        {
            var rootFolder = CreateChainOfFolders("D:", "Folder1", "InsideFolder1", "InsideInsideFolder1");
            string filePath = @"C:\Folder1\InsideFolder2\InsideInsideFolder1\InsideInsideFolder2\newTextDocument.txt";
            bool ifAdded = rootFolder.AddFileToRootFolder(new CreateOperationEvent { FilePath = filePath });
            Assert.AreEqual(false, ifAdded);
        }
        #endregion

        #region AddFolderToRootFolder

        private bool ContainsFolderInPath(FolderViewModel rootFolder, string folderPath)
        {
            var splitted = folderPath.Split(new[] { "\\" }, StringSplitOptions.None);
            if (rootFolder.Name != splitted[0])
                return false;

            var currFolder = rootFolder;
            for (int i = 1; i < splitted.Length; i++)
            {
                var fold = currFolder.Folders.FirstOrDefault(fld => fld.Name == splitted[i]);
                if (fold == null)
                    return false;
                currFolder = fold;
            }

            return true;
        }

        [TestMethod]
        public void AddFolderToRootFolderTester1()
        {
            var rootFolder = CreateChainOfFolders("D:", "Folder1", "InsideFolder1", "InsideInsideFolder1");
            string folderPath = @"D:\Folder1\InsideFolder1\InsideInsideFolder1";
            rootFolder.AddFolderToRootFolder(new RenameOperationEvent { FilePath = folderPath });
            Assert.AreEqual(true, ContainsFolderInPath(rootFolder, folderPath));
        }

        [TestMethod]
        public void AddFolderToRootFolderTester2()
        {
            var rootFolder = CreateChainOfFolders("D:", "Folder1", "InsideFolder1", "InsideInsideFolder1");
            string folderPath = @"D:\Folder1\InsideFolder1\InsideInsideFolder1\SomeNewFolder";
            rootFolder.AddFolderToRootFolder(new RenameOperationEvent { FilePath = folderPath, OldFilePath = @"D:\Folder1\InsideFolder1\InsideInsideFolder1\SomeOldFolderName" });
            Assert.AreEqual(true, ContainsFolderInPath(rootFolder, folderPath));
        }

        [TestMethod]
        public void AddFolderToRootFolderTester3()
        {
            var rootFolder = CreateChainOfFolders("D:", "Folder1", "InsideFolder1", "InsideInsideFolder1");
            string folderPath = @"D:\Folder1\SomeNewFolder";
            rootFolder.AddFolderToRootFolder(new RenameOperationEvent { FilePath = folderPath, OldFilePath = "" });
            Assert.AreEqual(true, ContainsFolderInPath(rootFolder, folderPath));
        }

        [TestMethod]
        public void AddFolderToRootFolderTester4()
        {
            var rootFolder = CreateChainOfFolders("D:", "Folder1", "InsideFolder1", "InsideInsideFolder1");
            string folderPath = @"C:\Folder1\SomeNewFolder";
            bool isAdded = rootFolder.AddFolderToRootFolder(new RenameOperationEvent { FilePath = folderPath });
            Assert.AreEqual(false, isAdded);
        }

        #endregion
    }
}
