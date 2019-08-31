using Microsoft.VisualStudio.TestTools.UnitTesting;
using DVL_Sync.Implementations;
using DVL_Sync_FileEventsLogger.Models;

namespace DVL_Sync.Tester
{
    [TestClass]
    public class TwoFoldersSyncerTester
    {
        [TestMethod]
        public void TestMethod1()
        {
            var folderOperationEventsReader = new FolderOperationEventsReaderFromJsonFile();
            var sync = new FoldersSyncer(folderOperationEventsReader);
            sync.SyncFolders(new FolderConfig
            {
                FolderPath = "D:\\New Folder",
                IncludeSubDirectories = true,
                WatchHiddenFiles = false
            },
            new FolderConfig
            {
                FolderPath = "D:\\New Folder (2)",
                IncludeSubDirectories = true,
                WatchHiddenFiles = false
            });

            Assert.AreEqual(1, 2);
        }
    }
}
