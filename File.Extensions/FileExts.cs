using System.IO;

namespace File.Extensions
{
    public static class CustomFile
    {
        public static void CopyToFolder(string filePathToCopy, string destinationFolderPath, bool overWrite)
        {
            string fileName = Path.GetFileName(filePathToCopy);
            System.IO.File.Copy(filePathToCopy, $"{destinationFolderPath}\\{fileName}", overWrite);
        }
    }
}
