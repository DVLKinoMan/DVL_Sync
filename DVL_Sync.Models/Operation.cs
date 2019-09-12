using System.IO;
using SystemIOFile = System.IO.File;

namespace DVL_Sync.Models
{
    public abstract class Operation
    {
        /// <summary>
        /// Performs Operation on folderPath folder
        /// </summary>
        /// <param name="folderPath"></param>
        public abstract void Execute(string folderPath);
    }

    public class CreateDirectoryOperation : Operation
    {
        public string DirectoryPathFromRoot { get; set; }

        /// <summary>
        /// Create Directory in folderPath
        /// </summary>
        /// <param name="folderPath"></param>
        public override void Execute(string folderPath) => Directory.CreateDirectory(Path.Combine(folderPath, DirectoryPathFromRoot));

        public override int GetHashCode() => DirectoryPathFromRoot.GetHashCode();
    }

    public class DeleteOperation : Operation
    {
        public string FilePathFromRoot { get; set; }

        /// <summary>
        /// Deletes File in folderPath which is located from root with FilePathFromRoot
        /// </summary>
        /// <param name="folderPath"></param>
        public override void Execute(string folderPath) => SystemIOFile.Delete(Path.Combine(folderPath, FilePathFromRoot));

        public override int GetHashCode() => FilePathFromRoot.GetHashCode();
    }

    public class CopyOperation : Operation
    {
        public string FilePathToCopy { get; set; }
        public string FilePathFromRoot { get; set; }

        /// <summary>
        /// Copys File to folderPath
        /// </summary>
        /// <param name="folderPath"></param>
        public override void Execute(string folderPath) => SystemIOFile.Copy(FilePathToCopy, Path.Combine(folderPath, FilePathFromRoot), true);

        public override bool Equals(object obj) => obj is CopyOperation copyOp && 
            copyOp.FilePathFromRoot == this.FilePathFromRoot && 
            copyOp.FilePathToCopy == this.FilePathToCopy;

        public override int GetHashCode() => FilePathToCopy.GetHashCode() + FilePathFromRoot.GetHashCode();
    }

    public class RenameOperation : Operation
    {
        public string FilePathFromRoot { get; set; }
        public string NewName { get; set; }

        /// <summary>
        /// Renames File which is in folderPath directory
        /// </summary>
        /// <param name="folderPath"></param>
        public override void Execute(string folderPath) => SystemIOFile.Move(Path.Combine(folderPath, FilePathFromRoot), NewName);

        public override bool Equals(object obj) => obj is RenameOperation renameOp &&
            renameOp.FilePathFromRoot == this.FilePathFromRoot &&
            renameOp.NewName == this.NewName;

        public override int GetHashCode() => FilePathFromRoot.GetHashCode() + NewName.GetHashCode();
    }
}
