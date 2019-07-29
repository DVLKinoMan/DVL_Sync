using DVL_Sync.Models;
using System.Collections.Generic;

namespace DVL_Sync.Extensions
{
    public static class OperationExts
    {
        public static void ExecuteAll(this IEnumerable<Operation> operations, string folderPath)
        {
            foreach (var op in operations)
                op.Execute(folderPath);
        }
    }
}
