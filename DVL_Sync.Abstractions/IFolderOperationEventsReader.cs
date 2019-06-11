using DVL_Sync_FileEventsLogger.Models;
using System.Collections.Generic;

namespace DVL_Sync.Abstractions
{
    public interface IFolderOperationEventsReader
    {
        IEnumerable<OperationEvent> ReadOperationEvents(string path);
    }
}
