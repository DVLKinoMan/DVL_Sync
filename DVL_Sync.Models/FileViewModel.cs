using DVL_Sync_FileEventsLogger.Models;
using System.Collections.Generic;

namespace DVL_Sync.Models
{
    public class FileViewModel
    {
        public string Name { get; set; }

        public FileViewModel(string name) =>
            (Name, OperationEvents) = (name, new List<OperationEvent>());

        public List<OperationEvent> OperationEvents { get; set; }

        public FileViewModel AddOperationEvent(OperationEvent operationEvent)
        {
            OperationEvents.Add(operationEvent);
            return this;
        }
    }
}
