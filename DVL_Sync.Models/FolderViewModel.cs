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

        public FolderViewModel(string name)
        {
            Name = name;
            Folders = new List<FolderViewModel>();
            Files = new List<FileViewModel>();
            OperationEvents = new List<OperationEvent>();
        }

        public FolderViewModel AddOperationEvent(OperationEvent operationEvent)
        {
            OperationEvents.Add(operationEvent);
            return this;
        }
    }
}
