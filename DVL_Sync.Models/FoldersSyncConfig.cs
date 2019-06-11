using DVL_Sync_FileEventsLogger.Models;
using System.Collections.Generic;

namespace DVL_Sync.Models
{
    public class FoldersSyncConfig
    {
        public IEnumerable<FolderConfig> FolderConfigs { get; set; }
        //It can have some default config
        public FoldersWatcherConfig FoldersWatcherConfig { get; set; }
    }
}
