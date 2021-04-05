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
        private IDirectory parent;

        #region Constructors

        public FileSystemDirectory(string dirName) : this(new DirectoryInfo(dirName))
        {
        }

        public FileSystemDirectory(DirectoryInfo directoryInfo)
        {
            this.directoryInfo = directoryInfo;
        }

        #endregion Constructors

        #region IDirectory Members

        public FileAttributes Attributes
        {
            get => directoryInfo.Attributes;
            set => directoryInfo.Attributes = value;
        }

        public Task<bool> ExistsAsync => Task.FromResult(directoryInfo.Exists);

        public string FullName => Path.TrimEndingDirectorySeparator(directoryInfo.FullName);

        public Task<DateTime> LastWriteTimeUtcAsync => Task.FromResult(directoryInfo.LastWriteTimeUtc);

        public string Name => directoryInfo.Name;

        public IDirectory Parent
        {
            get
            {
                if (parent == null && directoryInfo.Parent != null)
                {
                    parent = new FileSystemDirectory(directoryInfo.Parent);
                }
                return parent;
            }
        }

        public Task CreateAsync()
        {
            directoryInfo.Create();
            directoryInfo.Refresh();
            return Task.CompletedTask;
        }

        public Task DeleteAsync()
        {
            directoryInfo.Delete(true);
            return Task.CompletedTask;
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

        public Task RefreshAsync()
        {
            directoryInfo = new DirectoryInfo(directoryInfo.FullName);
            parent = null;
            return Task.CompletedTask;
        }
    }
}