﻿using System.IO;

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

    public class DeleteOperation : Operation
    {
        public string FilePathFromRoot { get; set; }

        /// <summary>
        /// Deletes File in folderPath which is located from root with FilePathFromRoot
        /// </summary>
        /// <param name="folderPath"></param>
        public override void Execute(string folderPath) => File.Delete(Path.Combine(folderPath, FilePathFromRoot));
    }

    public class CopyOperation : Operation
    {
        public string FilePathToCopy { get; set; }

        /// <summary>
        /// Copys File to folderPath
        /// </summary>
        /// <param name="folderPath"></param>
        public override void Execute(string folderPath) => File.Copy(FilePathToCopy, folderPath, true);
    }

    public class RenameOperation : Operation
    {
        public string FilePathFromRoot { get; set; }
        public string NewName { get; set; }

        /// <summary>
        /// Renames File which is in folderPath directory
        /// </summary>
        /// <param name="folderPath"></param>
        public override void Execute(string folderPath) => File.Move(Path.Combine(folderPath, FilePathFromRoot), NewName);
    }
}