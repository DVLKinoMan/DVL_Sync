using System.IO;
using SystemIOFile = System.IO.File;
//using static System.Extensions.SystemExts;

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
        public override void Execute(string folderPath) => Directory.CreateDirectory(Path.Combine(folderPath, DirectoryPathFromRoot));//.Attributes = 
            //SystemIOFile.GetAttributes(Path.Combine(folderPath, DirectoryPathFromRoot)) | FileAttributes.ReadOnly;

        public override int GetHashCode() => DirectoryPathFromRoot.GetHashCode();
    }

    public class DeleteOperation : Operation
    {
        public string FilePathFromRoot { get; set; }

        /// <summary>
        /// Deletes File in folderPath which is located from root with FilePathFromRoot
        /// </summary>
        /// <param name="folderPath"></param>
        public override void Execute(string folderPath)
        {
            string path = Path.Combine(folderPath, FilePathFromRoot);
            if (SystemIOFile.Exists(path))
                SystemIOFile.Delete(path);
        }

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
        public override void Execute(string folderPath)
        {
            //FilePathToCopy.AddFileAttribute(FileAttributes.ReadOnly);
            SystemIOFile.Copy(FilePathToCopy, Path.Combine(folderPath, FilePathFromRoot), true);
        }

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
        public override void Execute(string folderPath)
        {
            //Path.Combine(folderPath, FilePathFromRoot).AddFileAttribute(FileAttributes.ReadOnly);
            SystemIOFile.Move(Path.Combine(folderPath, FilePathFromRoot), NewName);
            //SystemIOFile.SetAttributes(Path.Combine(Path.GetDirectoryName(Path.Combine(folderPath, FilePathFromRoot)), NewName), FileAttributes.ReadOnly);
        }

        public override bool Equals(object obj) => obj is RenameOperation renameOp &&
            renameOp.FilePathFromRoot == this.FilePathFromRoot &&
            renameOp.NewName == this.NewName;

        public override int GetHashCode() => FilePathFromRoot.GetHashCode() + NewName.GetHashCode();
    }

}
