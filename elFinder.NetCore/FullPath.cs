using System;
using System.IO;

namespace elFinder.NetCore
{
    public class FullPath
    {
        private FileSystemInfo fileSystemInfo;
        private bool isDirectory;
        private string relativePath;
        private RootVolume root;

        public FullPath(RootVolume root, FileSystemInfo fileSystemInfo)
        {
            if (root == null)
            {
                throw new ArgumentNullException("root", "Root cannot be null");
            }

            if (fileSystemInfo == null)
            {
                throw new ArgumentNullException("fileSystemInfo", "FileSystemInfo cannot be null");
            }

            this.root = root;
            this.fileSystemInfo = fileSystemInfo;
            isDirectory = this.fileSystemInfo is DirectoryInfo;

            if (fileSystemInfo.FullName.StartsWith(root.Directory.FullName))
            {
                if (fileSystemInfo.FullName.Length == root.Directory.FullName.Length)
                {
                    relativePath = string.Empty;
                }
                else
                {
                    relativePath = fileSystemInfo.FullName.Substring(root.Directory.FullName.Length + 1);
                }
            }
            else
            {
                throw new InvalidOperationException("FileSystemInfo must be in the root directory or a subdirectory thereof");
            }
        }

        public DirectoryInfo Directory
        {
            get { return isDirectory ? (DirectoryInfo)fileSystemInfo : null; }
        }

        public FileInfo File
        {
            get { return !isDirectory ? (FileInfo)fileSystemInfo : null; }
        }

        public bool IsDirectory => isDirectory;

        public string RelativePath => relativePath;

        public RootVolume RootVolume => root;
    }
}