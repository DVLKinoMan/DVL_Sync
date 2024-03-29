﻿using DVL_Sync.Abstractions;
using DVL_Sync_FileEventsLogger.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace DVL_Sync.Implementations
{
    public class FolderOperationEventsReaderFromJsonFile : IFolderOperationEventsReader
    {
        public IEnumerable<OperationEvent> ReadOperationEvents(string path)
        {
            using var streamReader = new StreamReader(path);
            while (!streamReader.EndOfStream)
            {
                string json = streamReader.ReadLine();
                yield return JsonConvert.DeserializeObject<OperationEvent>(json);
            }
        }
    }
}
