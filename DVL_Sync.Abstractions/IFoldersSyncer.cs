using DVL_Sync.Models;
using System.Collections.Generic;

namespace DVL_Sync.Abstractions
{
    public interface IFoldersSyncer
    {
        void SyncFolders(IEnumerable<FoldersSyncConfig> foldersSyncConfigs);
    }
}
