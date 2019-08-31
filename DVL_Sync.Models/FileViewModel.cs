using DVL_Sync_FileEventsLogger.Models;

namespace DVL_Sync.Models
{
    public class FileViewModel
    {
        public string Name { get; set; }

        public FileViewModel(string name)
        {
            Name = name;
        }

        public OperationEvent OperationEvent { get; set; }
    }
}
