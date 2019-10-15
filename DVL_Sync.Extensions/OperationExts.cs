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
            //Copy operations which files do not exists
            var copyOperations = operations.Where(op => op is CopyOperation copy && !File.Exists(copy.FilePathToCopy)).Cast<CopyOperation>().ToList();

            var newCopyOperations = new List<CopyOperation>();
            var renameOperationsToIgnore = new List<Operation>();

            foreach (var op in operations)
                if(op is RenameOperation renameOp)
                {
                    var copyOp = copyOperations.FirstOrDefault(copOp => copOp.FilePathFromRoot == renameOp.FilePathFromRoot);
                    if (copyOp != null)
                    {
                        //If copyOp!=null means file do not exists because it was renamed
                        newCopyOperations.Add(
                        new CopyOperation
                        {
                            FilePathToCopy = Path.Combine(copyOp.FilePathToCopy.GetDirectoryPath(), renameOp.NewName),
                            FilePathFromRoot = Path.Combine(copyOp.FilePathFromRoot.GetDirectoryPath(), renameOp.NewName)
                        });
                        renameOperationsToIgnore.Add(renameOp);
                    }
                }

            var equalityComparer = new OperationEqualityComparer();
            return operations.Except(copyOperations, equalityComparer)
                             .Except(renameOperationsToIgnore, equalityComparer)
                             .Concat(newCopyOperations);
        }
    }

    public class OperationEqualityComparer : IEqualityComparer<Operation>
    {
        private readonly CopyOperationEqualityComparer copyOperationEqualityComparer;
        private readonly RenameOperationEqualityComparer renameOperationEqualityComparer;

        public OperationEqualityComparer()
        {
            copyOperationEqualityComparer = new CopyOperationEqualityComparer();
            renameOperationEqualityComparer = new RenameOperationEqualityComparer();
        }

        public bool Equals(Operation x, Operation y) => x is CopyOperation copyOp1 && y is CopyOperation copyOp2 ?
                 copyOperationEqualityComparer.Equals(copyOp1, copyOp2) :
                 (x is RenameOperation renOp1 && y is RenameOperation renOp2 && renameOperationEqualityComparer.Equals(renOp1, renOp2));

        public int GetHashCode(Operation obj) => obj.GetHashCode();
    }

    public class CopyOperationEqualityComparer : IEqualityComparer<CopyOperation>
    {
        public bool Equals(CopyOperation x, CopyOperation y) => x.Equals(y);

        public int GetHashCode(CopyOperation obj) => obj.GetHashCode();
    }

    public class RenameOperationEqualityComparer : IEqualityComparer<RenameOperation>
    {
        public bool Equals(RenameOperation x, RenameOperation y) => x.Equals(y);

        public int GetHashCode(RenameOperation obj) => obj.GetHashCode();
    }
}
