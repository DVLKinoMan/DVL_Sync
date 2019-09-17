using DVL_Sync_FileEventsLogger.Models;
using System.Threading;
using System.Threading.Tasks;

namespace DVL_Sync.Abstractions
{
    public interface IFoldersSyncer
    {
        void SyncFolders(FolderConfig folderConfig1, FolderConfig folderConfig2);
        Task SyncFoldersAsync(CancellationToken cancellationToken, FolderConfig folderConfig1, FolderConfig folderConfig2);
    }
}
