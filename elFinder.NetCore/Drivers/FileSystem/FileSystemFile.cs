using System;
using System.IO;
using System.Threading.Tasks;
using elFinder.NetCore.Helpers;
using elFinder.NetCore.Models;

namespace elFinder.NetCore.Drivers.FileSystem
{
    public class FileSystemFile : IFile
    {
        private FileInfo fileInfo;
        private IDirectory directory;

        #region Constructors

        public FileSystemFile(string filePath) : this(new FileInfo(filePath))
        {
        }

        public FileSystemFile(FileInfo fileInfo)
        {
            this.fileInfo = fileInfo;
        }

        #endregion Constructors

        #region IFile Members

        public FileAttributes Attributes
        {
            get => fileInfo.Attributes;
            set => fileInfo.Attributes = value;
        }

        public IDirectory Directory
        {
            get
            {
                if (directory == null)
                {
                    directory = new FileSystemDirectory(fileInfo.Directory);
                }
                return directory;
            }
        }

        public string DirectoryName => Path.TrimEndingDirectorySeparator(fileInfo.DirectoryName);

        public Task<bool> ExistsAsync => Task.FromResult(fileInfo.Exists);

        public string Extension => fileInfo.Extension;

        public string FullName => Path.TrimEndingDirectorySeparator(fileInfo.FullName);

        public Task<DateTime> LastWriteTimeUtcAsync => Task.FromResult(fileInfo.LastWriteTimeUtc);

        public Task<long> LengthAsync => Task.FromResult(fileInfo.Length);

        public string Name => fileInfo.Name;

        public MimeType MimeType => MimeHelper.GetMimeType(Extension);

        public IFile Open(string path)
        {
            return new FileSystemFile(path);
        }

        public Task<Stream> CreateAsync()
        {
            EnsureGarbageCollectorCalled();
            using var stream = File.Create(fileInfo.FullName);
            return Task.FromResult(stream as Stream);
        }

        public Task DeleteAsync()
        {
            EnsureGarbageCollectorCalled();
            fileInfo.Delete();
            return Task.CompletedTask;
        }

        public Task<Stream> OpenReadAsync()
        {
            EnsureGarbageCollectorCalled();
            return Task.FromResult<Stream>(fileInfo.OpenRead());
        }

        public Task PutAsync(string content)
        {
            EnsureGarbageCollectorCalled();
            File.WriteAllText(FullName, content);
            return Task.CompletedTask;
        }

        public Task PutAsync(Stream stream)
        {
            EnsureGarbageCollectorCalled();
            using (var destination = fileInfo.OpenWrite())
            {
                stream.CopyTo(destination);
            }

            return Task.CompletedTask;
        }

        #endregion IFile Members

        public void MoveTo(string destFileName, bool overwrite = false)
        {
            fileInfo.MoveTo(destFileName, overwrite);
        }

        // Bug Fix: https://stackoverflow.com/questions/13262548/delete-a-file-being-used-by-another-process/21137207#21137207
        private static void EnsureGarbageCollectorCalled()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public Task RefreshAsync()
        {
            fileInfo = new FileInfo(fileInfo.FullName);
            directory = null;
            return Task.CompletedTask;
        }
    }
}