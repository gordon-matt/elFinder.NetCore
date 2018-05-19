using System;
using System.IO;
using System.Threading.Tasks;

namespace elFinder.NetCore.Drivers.FileSystem
{
    public class FileSystemFile : IFile
    {
        private FileInfo fileInfo;

        #region Constructors

        public FileSystemFile(string fileName)
        {
            fileInfo = new FileInfo(fileName);
        }

        public FileSystemFile(FileInfo fileInfo)
        {
            this.fileInfo = fileInfo;
        }

        #endregion Constructors

        #region IFile Members

        public FileAttributes Attributes { get => fileInfo.Attributes; set => fileInfo.Attributes = value; }

        public IDirectory Directory => new FileSystemDirectory(fileInfo.Directory);

        public string DirectoryName => fileInfo.DirectoryName;

        public Task<bool> ExistsAsync => Task.FromResult(fileInfo.Exists);

        public string Extension => fileInfo.Extension;

        public string FullName => fileInfo.FullName;

        public Task<DateTime> LastWriteTimeUtcAsync => Task.FromResult(fileInfo.LastWriteTimeUtc);

        public Task<long> LengthAsync => Task.FromResult(fileInfo.Length);

        public string Name => fileInfo.Name;

        public IFile Clone(string path)
        {
            return new FileSystemFile(path);
        }

        public Task<Stream> CreateAsync()
        {
            return Task.FromResult(fileInfo.Create() as Stream);
        }

        public Task DeleteAsync()
        {
            fileInfo.Delete();
            return Task.FromResult(0);
        }

        public Task<Stream> OpenReadAsync()
        {
            return Task.FromResult(fileInfo.OpenRead() as Stream);
        }

        public Task PutAsync(string content)
        {
            File.WriteAllText(FullName, content);
            return Task.FromResult(0);
        }

        public Task PutAsync(Stream stream)
        {
            using (var dest = fileInfo.OpenWrite())
            {
                stream.CopyTo(dest);
            }

            return Task.FromResult(0);
        }

        #endregion IFile Members
    }
}