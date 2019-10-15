using System.Collections.Generic;
using DVL_Sync_FileEventsLogger.Models;

namespace DVL_Sync.Models
{
    public class FolderViewModel
    {
        public string Name { get; set; }
        public List<FolderViewModel> Folders { get; set; }
        public List<FileViewModel> Files { get; set; }
        public List<OperationEvent> OperationEvents { get; set; }

        public FolderViewModel(string name) =>
            (Name, Folders, Files, OperationEvents) = (name, new List<FolderViewModel>(), new List<FileViewModel>(),
                new List<OperationEvent>());

        public FolderViewModel AddOperationEvent(OperationEvent operationEvent)
        {
            OperationEvents.Add(operationEvent);
            return this;
        }
    }
}
