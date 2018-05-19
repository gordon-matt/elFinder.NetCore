using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.File;

namespace elFinder.NetCore.Drivers.AzureStorage
{
    public class AzureStorageFile : IFile
    {
        public static readonly char PathSeparator = '/';

        public AzureStorageFile(string fileName)
        {
            FullName = fileName;
        }

        public AzureStorageFile(CloudFile file)
        {
            FullName = file.Uri.LocalPath.Substring(1); // Remove starting '/'
        }

        public string FullName { get; }

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

        public IDirectory Directory => new AzureStorageDirectory(DirectoryName);

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

        public Task<bool> ExistsAsync => AzureStorageAPI.FileExists(FullName);

        public Task<long> LengthAsync => AzureStorageAPI.FileLength(FullName);

        public Task<DateTime> LastWriteTimeUtcAsync => AzureStorageAPI.FileLastModifiedTimeUtc(FullName);

		public FileAttributes Attributes
		{
			get => Name.StartsWith(".") ? FileAttributes.Hidden : FileAttributes.Normal;
			set => value = FileAttributes.Normal; // Azure Storage doesn't support setting attributes
		}

        public IFile Clone(string path) => new AzureStorageFile(path);

        public async Task<Stream> CreateAsync()
        {
            await AzureStorageAPI.MakeFile(FullName);
            return await AzureStorageAPI.FileStream(FullName);
        }

        public Task<Stream> OpenReadAsync() => AzureStorageAPI.FileStream(FullName);

        public Task PutAsync(string content) => AzureStorageAPI.Put(FullName, content);

        public Task PutAsync(Stream stream) => AzureStorageAPI.Put(FullName, stream);

        public Task DeleteAsync() => AzureStorageAPI.DeleteFile(FullName);
    }
}