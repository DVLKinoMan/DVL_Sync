using DVL_Sync.Abstractions;
using DVL_Sync.Models;
using System;
using System.Collections.Generic;
using System.Text;
using DVL_Sync_FileEventsLogger.Models;

namespace DVL_Sync.Implementations
{
    public class FoldersSyncer : IFoldersSyncer
    {
        public void SyncFolders(IEnumerable<FoldersSyncConfig> foldersSyncConfigs)
        {

        }

        private void SyncFolders(FolderConfig folderConfig1, FolderConfig folderConfig2)
        {
            //get log files of this configs
            //get operationEvents from logfiles
            //filter operationEvents
            //getoperations from filtered operationevents
            //doing this operations
        }
    }
}
