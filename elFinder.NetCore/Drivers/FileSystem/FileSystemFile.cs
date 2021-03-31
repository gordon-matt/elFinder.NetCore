using System;
using System.IO;
using System.Threading.Tasks;
using elFinder.NetCore.Helpers;
using elFinder.NetCore.Models;

namespace elFinder.NetCore.Drivers.FileSystem
{
    public class FileSystemFile : IFile
    {
        private readonly FileInfo fileInfo;

        #region Constructors

        public FileSystemFile(string filePath) : this(new FileInfo(filePath))
        {
        }

        public FileSystemFile(FileInfo fileInfo)
        {
            this.fileInfo = fileInfo;
            Name = fileInfo.Name;
            FullName = Path.TrimEndingDirectorySeparator(fileInfo.FullName);
            DirectoryName = Path.TrimEndingDirectorySeparator(fileInfo.DirectoryName);
            Extension = fileInfo.Extension;
            Directory = new FileSystemDirectory(fileInfo.Directory);
            ExistsAsync = Task.FromResult(fileInfo.Exists);
            LastWriteTimeUtcAsync = Task.FromResult(fileInfo.LastWriteTimeUtc);
            LengthAsync = Task.FromResult(fileInfo.Length);
            MimeType = MimeHelper.GetMimeType(Extension);
        }

        #endregion Constructors

        #region IFile Members

        public FileAttributes Attributes
        {
            get => fileInfo.Attributes;
            set => fileInfo.Attributes = value;
        }

        public IDirectory Directory { get; }

        public string DirectoryName { get; private set; }

        public Task<bool> ExistsAsync { get; }

        public string Extension { get; private set; }

        public string FullName { get; private set; }

        public Task<DateTime> LastWriteTimeUtcAsync { get; }

        public Task<long> LengthAsync { get; }

        public string Name { get; private set; }

        public MimeType MimeType { get; }

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

            return Task.FromResult(0);
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
    }
}