using System.Collections.Generic;
using DVL_Sync_FileEventsLogger.Models;

namespace DVL_Sync.Models
{
    public class FolderViewModel
    {
        public string Name { get; set; }
        public List<FolderViewModel> Folders { get; set; }
        public List<FileViewModel> Files { get; set; }
        public OperationEvent OperationEvent { get; set; }

        public FolderViewModel(string name)
        {
            Name = name;
            Folders = new List<FolderViewModel>();
            Files = new List<FileViewModel>();
        }
    }
}
