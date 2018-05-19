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

        public AzureStorageDirectory(string dirName)
        {
            FullName = dirName;
        }

        public AzureStorageDirectory(CloudFileDirectory dir)
        {
            FullName = dir.Uri.LocalPath.Substring(1); // Remove starting '/'
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
                        return FullName.Substring(startIndex + 1);
                }
                return FullName;
            }
        }

        public Task<bool> ExistsAsync => AzureStorageAPI.DirectoryExists(FullName);

        public Task<DateTime> LastWriteTimeUtcAsync => AzureStorageAPI.DirectoryLastModifiedTimeUtc(FullName);

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

        public FileAttributes Attributes
		{
			get => Name.StartsWith(".") ? FileAttributes.Hidden : FileAttributes.Directory;
			set => value = FileAttributes.Directory; // Azure Storage doesn't support setting attributes
		}

        public Task CreateAsync() => AzureStorageAPI.CreateDirectory(FullName);

        public Task DeleteAsync() => AzureStorageAPI.DeleteDirectory(FullName);

        public async Task<IEnumerable<IDirectory>> GetDirectoriesAsync()
        {
            var result = new List<IDirectory>();

            var files = (await AzureStorageAPI.ListFilesAndDirectories(FullName)).Where(f => f is CloudFileDirectory);
            result.AddRange(files.Select(f => new AzureStorageDirectory(((CloudFileDirectory)f).Name)));

            return result;
        }

        public async Task<IEnumerable<IFile>> GetFilesAsync()
        {
            var result = new List<IFile>();

            var files = (await AzureStorageAPI.ListFilesAndDirectories(FullName)).Where(f => f is CloudFile);
            result.AddRange(files.Select(f => new AzureStorageFile(((CloudFile)f).Name)));

            return result;
        }
    }
}