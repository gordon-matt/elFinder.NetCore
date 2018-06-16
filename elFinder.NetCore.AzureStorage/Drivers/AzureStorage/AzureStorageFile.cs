using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.File;

namespace elFinder.NetCore.Drivers.AzureStorage
{
    public class AzureStorageFile : IFile
    {
        public static readonly char PathSeparator = '/';

        #region Constructors

        public AzureStorageFile(string fileName)
        {
            FullName = fileName;
        }

        public AzureStorageFile(CloudFile file)
        {
            FullName = file.Uri.LocalPath.Substring(1); // Remove starting '/'
        }

        #endregion Constructors

        #region IFile Members

        public FileAttributes Attributes
        {
            get => Name.StartsWith(".") ? FileAttributes.Hidden : FileAttributes.Normal;
            set { } // Azure Storage doesn't support setting attributes
        }

        public IDirectory Directory => new AzureStorageDirectory(DirectoryName);

        public string DirectoryName
        {
            get
            {
                int length = FullName.Length;
                int startIndex = length;

                while (--startIndex >= 0)
                {
                    char ch = FullName[startIndex];
                    if (ch == PathSeparator)
                    {
                        return FullName.Substring(0, startIndex);
                    }
                }
                return string.Empty;
            }
        }

        public Task<bool> ExistsAsync => AzureStorageAPI.FileExistsAsync(FullName);

        public string Extension
        {
            get
            {
                int length = FullName.Length;
                int startIndex = length;
                while (--startIndex >= 0)
                {
                    char ch = FullName[startIndex];
                    if (ch == '.')
                    {
                        return FullName.Substring(startIndex);
                    }
                }
                return string.Empty;
            }
        }

        public string FullName { get; }

        public Task<DateTime> LastWriteTimeUtcAsync => AzureStorageAPI.FileLastModifiedTimeUtcAsync(FullName);

        public Task<long> LengthAsync => AzureStorageAPI.FileLengthAsync(FullName);

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
                    {
                        return FullName.Substring(startIndex + 1);
                    }
                }
                return FullName;
            }
        }

        public IFile Clone(string path) => new AzureStorageFile(path);

        public async Task<Stream> CreateAsync()
        {
            await AzureStorageAPI.MakeFileAsync(FullName);
            return await AzureStorageAPI.FileStreamAsync(FullName);
        }

        public Task DeleteAsync() => AzureStorageAPI.DeleteFileAsync(FullName);

        public Task<Stream> OpenReadAsync() => AzureStorageAPI.FileStreamAsync(FullName);

        public Task PutAsync(string content) => AzureStorageAPI.PutAsync(FullName, content);

        public Task PutAsync(Stream stream) => AzureStorageAPI.PutAsync(FullName, stream);

        #endregion IFile Members
    }
}