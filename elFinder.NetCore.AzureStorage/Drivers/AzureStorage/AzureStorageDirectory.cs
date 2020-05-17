using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.File;

namespace elFinder.NetCore.Drivers.AzureStorage
{
    public class AzureStorageDirectory : IDirectory
    {
        public static readonly char PathSeparator = '/';

        #region Constructors

        public AzureStorageDirectory(string dirName)
        {
            FullName = dirName;
        }

        public AzureStorageDirectory(CloudFileDirectory dir)
        {
            FullName = dir.Uri.LocalPath.Substring(1); // Remove starting '/'
        }

        #endregion Constructors

        #region IDirectory Members

        public FileAttributes Attributes
        {
            get => Name.StartsWith(".") ? FileAttributes.Hidden : FileAttributes.Directory;
            set => value = FileAttributes.Directory; // Azure Storage doesn't support setting attributes
        }

        public Task<bool> ExistsAsync => AzureStorageAPI.DirectoryExistsAsync(FullName);

        public string FullName { get; }

        public Task<DateTime> LastWriteTimeUtcAsync => AzureStorageAPI.DirectoryLastModifiedTimeUtcAsync(FullName);

        public string Name
        {
            get
            {
                int length = FullName.Length;
                int startIndex = length;
                while (--startIndex >= 0)
                {
                    char ch = FullName[startIndex];
                    if (ch == PathSeparator)
                        return FullName.Substring(startIndex + 1);
                }
                return FullName;
            }
        }

        public IDirectory Parent
        {
            get
            {
                int length = FullName.Length;
                int startIndex = length;
                while (--startIndex >= 0)
                {
                    char ch = FullName[startIndex];
                    if (ch == PathSeparator)
                        return new AzureStorageDirectory(FullName.Substring(0, startIndex));
                }
                return null;
            }
        }

        public Task CreateAsync() => AzureStorageAPI.CreateDirectoryAsync(FullName);

        public Task DeleteAsync() => AzureStorageAPI.DeleteDirectoryAsync(FullName);

        public async Task<IEnumerable<IDirectory>> GetDirectoriesAsync()
        {
            var result = new List<IDirectory>();

            var files = (await AzureStorageAPI.ListFilesAndDirectoriesAsync(FullName)).Where(f => f is CloudFileDirectory);
            result.AddRange(files.Select(f => new AzureStorageDirectory(f as CloudFileDirectory)));

            return result;
        }

        // TODO: Make use of the mimeTypes argument
        public async Task<IEnumerable<IFile>> GetFilesAsync(IEnumerable<string> mimeTypes)
        {
            var result = new List<IFile>();

            var files = (await AzureStorageAPI.ListFilesAndDirectoriesAsync(FullName)).Where(f => f is CloudFile);
            result.AddRange(files.Select(f => new AzureStorageFile(f as CloudFile)));

            return result;
        }

        #endregion IDirectory Members
    }
}