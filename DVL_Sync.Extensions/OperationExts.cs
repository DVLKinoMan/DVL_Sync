using DVL_Sync.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Extensions;

namespace DVL_Sync.Extensions
{
    public static class OperationExts
    {
        public static void ExecuteAll(this IEnumerable<Operation> operations, string folderPath)
        {
            foreach (var op in operations)
                op.Execute(folderPath);
        }

        public static IEnumerable<Operation> FilterOperations(this IEnumerable<Operation> operations)
        {
            var copyOperations = operations.Where(op => op is CopyOperation copy && !File.Exists(copy.FilePathToCopy)).Cast<CopyOperation>().ToList();

            var newCopyOperations = new List<CopyOperation>();
            var renameOperationsToIgnore = new List<RenameOperation>();

            foreach (var op in operations)
            {
                if(op is RenameOperation renameOp)
                {
                    var copyOp = copyOperations.FirstOrDefault(copOp => copOp.FilePathFromRoot == renameOp.FilePathFromRoot);
                    if(copyOp != null)
                    {
                        newCopyOperations.Add(
                            new CopyOperation
                            {
                                FilePathToCopy = Path.Combine(copyOp.FilePathToCopy.GetDirectoryPath(), renameOp.NewName),
                                FilePathFromRoot = Path.Combine(copyOp.FilePathFromRoot.GetDirectoryPath(), renameOp.NewName)
                            });
                        renameOperationsToIgnore.Add(renameOp);
                    }
                }
            }

            return operations.Except(copyOperations).Except(renameOperationsToIgnore).Concat(newCopyOperations);
        }
    }
}
