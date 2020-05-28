using System;
using System.IO;
using System.Threading.Tasks;
using elFinder.NetCore.Helpers;
using elFinder.NetCore.Models;

namespace elFinder.NetCore.Drivers.FileSystem
{
    public class FileSystemFile : IFile
    {
        private readonly string filePath;

        #region Constructors

        public FileSystemFile(string filePath)
            : this(new FileInfo(filePath))
        {
        }

        public FileSystemFile(FileInfo fileInfo)
        {
            filePath = fileInfo.FullName;
            Name = fileInfo.Name;
            FullName = fileInfo.FullName;
            DirectoryName = fileInfo.DirectoryName;
            Extension = fileInfo.Extension;
        }

        #endregion Constructors

        #region IFile Members

        public FileAttributes Attributes
        {
            get => File.GetAttributes(filePath);
            set => File.SetAttributes(filePath, value);
        }

        public IDirectory Directory => new FileSystemDirectory(new FileInfo(filePath).Directory);

        public string DirectoryName { get; private set; }

        public Task<bool> ExistsAsync => Task.FromResult(File.Exists(filePath));

        public string Extension { get; private set; }

        public string FullName { get; private set; }

        public Task<DateTime> LastWriteTimeUtcAsync => Task.FromResult(File.GetLastWriteTimeUtc(filePath));

        public Task<long> LengthAsync => Task.FromResult(new FileInfo(filePath).Length);

        public string Name { get; private set; }

        public MimeType MimeType => MimeHelper.GetMimeType(Extension);

        public IFile Open(string path)
        {
            return new FileSystemFile(path);
        }

        public Task<Stream> CreateAsync()
        {
            EnsureGarbageCollectorCalled();
            using var stream = File.Create(filePath);
            return Task.FromResult(stream as Stream);
        }

        public Task DeleteAsync()
        {
            EnsureGarbageCollectorCalled();
            File.Delete(filePath);
            return Task.FromResult(0);
        }

        public Task<Stream> OpenReadAsync()
        {
            EnsureGarbageCollectorCalled();
            return Task.FromResult(File.OpenRead(filePath) as Stream);
        }

        public Task PutAsync(string content)
        {
            EnsureGarbageCollectorCalled();
            File.WriteAllText(FullName, content);
            return Task.FromResult(0);
        }

        public Task PutAsync(Stream stream)
        {
            EnsureGarbageCollectorCalled();
            using (var destination = File.OpenWrite(filePath))
            {
                stream.CopyTo(destination);
            }

            return Task.FromResult(0);
        }

        #endregion IFile Members

        public void MoveTo(string destFileName)
        {
            File.Move(filePath, destFileName);
        }

        // Bug Fix: https://stackoverflow.com/questions/13262548/delete-a-file-being-used-by-another-process/21137207#21137207
        private static void EnsureGarbageCollectorCalled()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}