using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace elFinder.NetCore.Drivers.FileSystem
{
    public class FileSystemDirectory : IDirectory
    {
        private readonly DirectoryInfo directoryInfo;

        #region Constructors

        public FileSystemDirectory(string dirName) : this(new DirectoryInfo(dirName))
        {
        }

        // Init properties values to prevent additional calls for each request. 
        // It is safe since DirectoryInfo only reflects information about the directory at requested time.
        public FileSystemDirectory(DirectoryInfo directoryInfo)
        {
            this.directoryInfo = directoryInfo;
            FullName = Path.TrimEndingDirectorySeparator(directoryInfo.FullName);
            ExistsAsync = Task.FromResult(directoryInfo.Exists);
            LastWriteTimeUtcAsync = Task.FromResult(directoryInfo.LastWriteTimeUtc);
            Name = directoryInfo.Name;
        }

        #endregion Constructors

        #region IDirectory Members

        public FileAttributes Attributes
        {
            get => directoryInfo.Attributes;
            set => directoryInfo.Attributes = value;
        }

        public Task<bool> ExistsAsync { get; }

        public string FullName { get; }

        public Task<DateTime> LastWriteTimeUtcAsync { get; }

        public string Name { get; }

        private IDirectory _parent;
        public IDirectory Parent
        {
            get
            {
                if (_parent == null && directoryInfo.Parent != null)
                    _parent = new FileSystemDirectory(directoryInfo.Parent);
                return _parent;
            }
        }

        public Task CreateAsync()
        {
            directoryInfo.Create();
            directoryInfo.Refresh();
            return Task.FromResult(0);
        }

        public Task DeleteAsync()
        {
            directoryInfo.Delete(true);
            return Task.FromResult(0);
        }

        public Task<IEnumerable<IDirectory>> GetDirectoriesAsync()
        {
            var dirs = directoryInfo.GetDirectories().Select(d => new FileSystemDirectory(d) as IDirectory);
            return Task.FromResult(dirs);
        }

        public Task<IEnumerable<IDirectory>> GetDirectoriesAsync(string pattern)
        {
            var dirs = directoryInfo.GetDirectories(pattern, SearchOption.AllDirectories).Select(d => new FileSystemDirectory(d) as IDirectory);
            return Task.FromResult(dirs);
        }

        public Task<IEnumerable<IFile>> GetFilesAsync(IEnumerable<string> mimeTypes)
        {
            var files = directoryInfo.GetFiles().Select(f => new FileSystemFile(f) as IFile);

            if (mimeTypes != null && mimeTypes.Count() > 0)
            {
                files = files.Where(f => mimeTypes.Contains(f.MimeType) || mimeTypes.Contains(f.MimeType.Type));
            }

            return Task.FromResult(files);
        }

        public Task<IEnumerable<IFile>> GetFilesAsync(IEnumerable<string> mimeTypes, string pattern)
        {
            var files = directoryInfo.GetFiles(pattern, SearchOption.AllDirectories).Select(f => new FileSystemFile(f) as IFile);

            if (mimeTypes != null && mimeTypes.Count() > 0)
            {
                files = files.Where(f => mimeTypes.Contains(f.MimeType) || mimeTypes.Contains(f.MimeType.Type));
            }

            return Task.FromResult(files);
        }

        #endregion IDirectory Members

        public void MoveTo(string destDirName)
        {
            directoryInfo.MoveTo(destDirName);
        }
    }
}