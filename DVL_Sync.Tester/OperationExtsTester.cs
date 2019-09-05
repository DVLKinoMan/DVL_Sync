using DVL_Sync.Extensions;
using DVL_Sync_FileEventsLogger.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace DVL_Sync.Tester
{
    [TestClass]
    public class OperationExtsTester
    {
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
        public void GetRootPathTester5()
        {
            var list = new List<OperationEvent>();

            Assert.ThrowsException(list.GetRootPath());
        }

    }
}
