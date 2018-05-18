using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace elFinder.NetCore.Drivers.FileSystem
{
    public class FileSystemDirectory : IDirectory
    {
        private DirectoryInfo directoryInfo;

        public FileSystemDirectory(string dirName)
        {
            directoryInfo = new DirectoryInfo(dirName);
        }

        public FileSystemDirectory(DirectoryInfo directoryInfo)
        {
            this.directoryInfo = directoryInfo;
        }

        public string Name => directoryInfo.Name;

        public string FullName => directoryInfo.FullName;

        public IDirectory Parent => new FileSystemDirectory(directoryInfo.Parent);

        public Task<bool> ExistsAsync => Task.FromResult(directoryInfo.Exists);

        public Task<DateTime> LastWriteTimeUtcAsync => Task.FromResult(directoryInfo.LastWriteTimeUtc);

        public Task<FileAttributes> AttributesAsync => Task.FromResult(directoryInfo.Attributes);

        public Task CreateAsync()
        {
            directoryInfo.Create();
            return Task.FromResult(0);
        }

        public Task DeleteAsync()
        {
            directoryInfo.Delete(true);
            return Task.FromResult(0);
        }

        public Task<IEnumerable<IFile>> GetFilesAsync()
        {
            var files = directoryInfo.GetFiles().Select(f => new FileSystemFile(f) as IFile);
            return Task.FromResult(files);
        }

        public Task<IEnumerable<IDirectory>> GetDirectoriesAsync()
        {
            var dirs = directoryInfo.GetDirectories().Select(d => new FileSystemDirectory(d) as IDirectory);
            return Task.FromResult(dirs);
        }
    }
}