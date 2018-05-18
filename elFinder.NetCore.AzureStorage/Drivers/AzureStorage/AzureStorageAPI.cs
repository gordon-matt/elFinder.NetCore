using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.File;

namespace elFinder.NetCore.Drivers.AzureStorage
{
    public static class AzureStorageAPI
    {
        public static string AccountName { get; set; }
        public static string AccountKey { get; set; }

        public static async Task<IEnumerable<IListFileItem>> ListFilesAndDirectories(string dir)
        {
            var rootDir = GetRootDirectoryReference(dir);
            var sampleDir = IsRoot(dir) ? rootDir : rootDir.GetDirectoryReference(RelativePath(dir));

            var results = new List<IListFileItem>();
            FileContinuationToken token = null;
            try
            {
                do
                {
                    var resultSegment = await sampleDir.ListFilesAndDirectoriesSegmentedAsync(token);
                    results.AddRange(resultSegment.Results);
                    token = resultSegment.ContinuationToken;
                }
                while (token != null);
            }
            catch (Exception)
            {
                return new IListFileItem[0];
            }

            return results;
        }

        public static async Task<bool> RootDirectoryExists(string root)
        {
            try
            {
                var rootDir = GetRootDirectoryReference(root);
                return await rootDir.ExistsAsync();
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async Task<bool> DirectoryExists(string dir)
        {
            try
            {
                var rootDir = GetRootDirectoryReference(dir);

                if (IsRoot(dir))
                {
                    return await rootDir.ExistsAsync();
                }

                // Get a reference to the directory we created previously.
                var sampleDir = rootDir.GetDirectoryReference(RelativePath(dir));

                return await sampleDir.ExistsAsync();
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async Task<bool> FileExists(string file)
        {
            try
            {
                var rootDir = GetRootDirectoryReference(file);

                // Get a reference to the directory we created previously.
                var sampleFile = rootDir.GetFileReference(RelativePath(file));

                return await sampleFile.ExistsAsync();
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async Task CreateDirectory(string dir)
        {
            var rootDir = GetRootDirectoryReference(dir);

            // Get relative path
            var relativePath = RelativePath(dir);

            // Create directories
            var subdir = string.Empty;
            foreach (var path in relativePath.Split('/'))
            {
                subdir = UrlCombine(subdir, path);
                var sampleDir = rootDir.GetDirectoryReference(subdir);
                await sampleDir.CreateIfNotExistsAsync();
            }
        }

        public static async Task DeleteDirectory(string dir)
        {
            var rootDir = GetRootDirectoryReference(dir);

            // Get a reference to the directory we created previously.
            var sampleDir = rootDir.GetDirectoryReference(RelativePath(dir));

            // Delete subdirectories and files
            foreach (var item in await ListFilesAndDirectories(dir))
            {
                string relativePath = RelativePath(item.Uri.LocalPath);

                if (item is CloudFileDirectory)
                {
                    await DeleteDirectory(relativePath);
                }
                else if (item is CloudFile)
                {
                    await DeleteFile(relativePath);
                }
            }
            await sampleDir.DeleteAsync();
        }

        public static async Task DeleteDirectoryIfExists(string dir)
        {
            var rootDir = GetRootDirectoryReference(dir);

            // Get a reference to the directory we created previously.
            var sampleDir = rootDir.GetDirectoryReference(RelativePath(dir));

            await sampleDir.DeleteIfExistsAsync();
        }

        public static async Task DeleteFile(string file)
        {
            var rootDir = GetRootDirectoryReference(file);

            // Get a reference to the directory we created previously.
            var sampleFile = rootDir.GetFileReference(RelativePath(file));

            // Create directory
            await sampleFile.DeleteAsync();
        }

        public static async Task DeleteFileIfExists(string file)
        {
            var rootDir = GetRootDirectoryReference(file);

            // Get a reference to the directory we created previously.
            var sampleFile = rootDir.GetFileReference(RelativePath(file));

            // Create directory
            await sampleFile.DeleteIfExistsAsync();
        }

        public static async Task MakeFile(string file)
        {
            var rootDir = GetRootDirectoryReference(file);

            // Get a reference to the directory we created previously.
            var sampleFile = rootDir.GetFileReference(RelativePath(file));

            // Create directory
            await sampleFile.CreateAsync(0);
        }

        public static async Task Get(string file, Stream stream)
        {
            var rootDir = GetRootDirectoryReference(file);

            // Get a reference to the directory we created previously.
            var sampleFile = rootDir.GetFileReference(RelativePath(file));

            // Create directory
            await sampleFile.DownloadToStreamAsync(stream);
        }

        public static async Task CopyFile(string source, string destination)
        {
            var rootDir = GetRootDirectoryReference(source);

            // Get a reference to the directory we created previously.
            var sourceFile = rootDir.GetFileReference(RelativePath(source));

            // Get a reference to the directory we created previously.
            var destFile = rootDir.GetFileReference(RelativePath(destination));

            // Create directory
            await destFile.Parent.CreateIfNotExistsAsync();

            // Copy file
            var result = await destFile.StartCopyAsync(sourceFile);
            while (destFile.CopyState.Status == CopyStatus.Pending)
            {
                Thread.Sleep(500);
            }

            if (destFile.CopyState.Status != CopyStatus.Success)
            {
                throw new Exception("Copy failed: " + destFile.CopyState.Status);
            }
        }

        public static async Task MoveFile(string source, string destination)
        {
            await CopyFile(source, destination);

            var rootDir = GetRootDirectoryReference(source);
            var sourceFile = rootDir.GetFileReference(RelativePath(source));

            await sourceFile.DeleteAsync();
        }

        public static async Task CopyDirectory(string source, string destination)
        {
            var rootDir = GetRootDirectoryReference(source);

            foreach (var item in await ListFilesAndDirectories(source))
            {
                var src = RelativePath(item.Uri.LocalPath);
                var dest = UrlCombine(destination, Path.GetFileName(item.Uri.LocalPath));
                if (item is CloudFileDirectory)
                {
                    await CopyDirectory(src, dest);
                }
                else if (item is CloudFile)
                {
                    await CopyFile(src, dest);
                }
            }
        }

        public static async Task MoveDirectory(string source, string destination)
        {
            await CopyDirectory(source, destination);

            var rootDir = GetRootDirectoryReference(source);
            var sourceDir = rootDir.GetDirectoryReference(RelativePath(source));

            await sourceDir.DeleteAsync();
        }

        public static async Task Put(string file, string content)
        {
            var rootDir = GetRootDirectoryReference(file);

            var sampleFile = rootDir.GetFileReference(RelativePath(file));

            await sampleFile.UploadTextAsync(content);
        }

        public static async Task Put(string file, Stream stream)
        {
            var rootDir = GetRootDirectoryReference(file);

            var sampleFile = rootDir.GetFileReference(RelativePath(file));

            await sampleFile.UploadFromStreamAsync(stream);
        }

        public static async Task Upload(IFormFile file, string path)
        {
            var rootDir = GetRootDirectoryReference(path);

            var sampleFile = rootDir.GetFileReference(RelativePath(path));

            using (var dest = await sampleFile.OpenWriteAsync(file.Length))
            {
                using (var s = file.OpenReadStream())
                {
                    s.CopyTo(dest);
                }
            }
        }

        public static async Task<Stream> FileStream(string file)
        {
            var rootDir = GetRootDirectoryReference(file);

            var sampleFile = rootDir.GetFileReference(RelativePath(file));

            return await sampleFile.OpenReadAsync();
        }

        public static async Task<byte[]> FileBytes(string file)
        {
            var rootDir = GetRootDirectoryReference(file);
            var sampleFile = rootDir.GetFileReference(RelativePath(file));
            await sampleFile.FetchAttributesAsync();

            var result = new byte[sampleFile.Properties.Length];
            await sampleFile.DownloadToByteArrayAsync(result, 0);
            return result;
        }

        public static async Task<long> FileLength(string file)
        {
            var rootDir = GetRootDirectoryReference(file);

            var sampleFile = rootDir.GetFileReference(RelativePath(file));
            await sampleFile.FetchAttributesAsync();

            return sampleFile.Properties.Length;
        }

        public static async Task<DateTime> FileLastModifiedTimeUtc(string file)
        {
            var rootDir = GetRootDirectoryReference(file);

            var sampleFile = rootDir.GetFileReference(RelativePath(file));
            await sampleFile.FetchAttributesAsync();

            return sampleFile.Properties.LastModified?.DateTime ?? DateTime.UtcNow;
        }

        public static async Task<DateTime> DirectoryLastModifiedTimeUtc(string dir)
        {
            var rootDir = GetRootDirectoryReference(dir);

            var sourceDir = rootDir.GetDirectoryReference(RelativePath(dir));

            await sourceDir.FetchAttributesAsync();
            return sourceDir.Properties.LastModified?.DateTime ?? DateTime.UtcNow;
        }

        public static CloudFileDirectory GetRootDirectoryReference(string path)
        {
            var storageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(AccountName, AccountKey), true);

            // Create a CloudFileClient object for credentialed access to File storage.
            var fileClient = storageAccount.CreateCloudFileClient();

            // Get a reference to the file share we created previously.

            var fileshare = fileClient.GetShareReference(GetRoot(path));

            // Get a reference to the root directory for the share.
            return fileshare.GetRootDirectoryReference();
        }

        public static async Task<bool> CreateShare(string share)
        {
            try
            {
                var storageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(AccountName, AccountKey), true);
                var fileClient = storageAccount.CreateCloudFileClient();
                var fileshare = fileClient.GetShareReference(share);
                await fileshare.CreateIfNotExistsAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static string GetRoot(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");

            int length = 0;
            while (length < path.Length)
            {
                char ch = path[length];
                if (ch == '/')
                {
                    return path.Substring(0, length);
                }
                length++;
            }
            return path;
        }

        private static bool IsRoot(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;

            return !path.Contains("/");
        }

        public static string RelativePath(string path)
        {
            int length = 0;
            while (length < path.Length)
            {
                char ch = path[length];
                if (ch == '/')
                {
                    return path.Substring(length + 1);
                }
                length++;
            }
            return string.Empty;
        }

        public static string UrlCombine(string path1, string path2)
        {
            // Force forward slash as path separator
            var result = Path.Combine(path1, path2);
            return Path.DirectorySeparatorChar == '/' ? result : result.Replace(Path.DirectorySeparatorChar, '/');
        }

        /// <summary>
        /// Get name for duplicating file
        /// </summary>
        /// <param name="file">File</param>
        /// <returns>Duplicated name</returns>
        public static async Task<string> GetDuplicatedName(IFile file)
        {
            var parentPath = file.Directory.Name;
            var name = Path.GetFileNameWithoutExtension(file.FullName);
            var ext = file.Extension;

            var newName = $@"{parentPath}/{name} copy{ext}";
            if (!await FileExists(newName))
            {
                return newName;
            }
            else
            {
                var found = false;
                for (var i = 1; i < 10 && !found; i++)
                {
                    newName = $@"{parentPath}/{name} copy {i}{ext}";
                    if (!await FileExists(newName))
                        found = true;
                }
                if (!found)
                {
                    newName = $@"{parentPath}/{name} copy {Guid.NewGuid()}{ext}";
                }
            }

            return newName;
        }
    }
}