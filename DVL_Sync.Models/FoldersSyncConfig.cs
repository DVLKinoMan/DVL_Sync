using DVL_Sync_FileEventsLogger.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace DVL_Sync.Models
{
    public class FoldersSyncConfig
    {
        //[JsonProperty("FolderConfigs", ItemConverterType = typeof(StringEnumConverter))]
        public FolderConfig[] FolderConfigs { get; set; }
        //It can have some default config
        //public FoldersWatcherConfig FoldersWatcherConfig { get; set; }
    }
}
