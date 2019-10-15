using DVL_Sync.Abstractions;
using DVL_Sync.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace DVL_Sync.Implementations
{
    public class FoldersSyncReaderFromJsonFile : IFoldersSyncReader
    {
        public IEnumerable<FoldersSyncConfig> ReadFoldersSyncConfigs(string path)
        {
            using var streamReader = new StreamReader(path);
            while (!streamReader.EndOfStream)
            {
                string json = streamReader.ReadLine();
                yield return JsonConvert.DeserializeObject<FoldersSyncConfig>(json);
            }
        }
    }
}
