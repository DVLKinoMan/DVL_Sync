using DVL_Sync.Models;
using System.Collections.Generic;

namespace DVL_Sync.Abstractions
{
    public interface IFoldersSyncReader
    {
        IEnumerable<FoldersSyncConfig> ReadFoldersSyncConfigs(string path);
    }
}
