using DVL_Sync_FileEventsLogger.Models;

namespace DVL_Sync.Abstractions
{
    public interface IFoldersSyncer
    {
        void SyncFolders(FolderConfig folderConfig1, FolderConfig folderConfig2);
    }
}
