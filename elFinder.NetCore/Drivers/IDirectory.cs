using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace elFinder.NetCore.Drivers
{
    public interface IDirectory
    {
        FileAttributes Attributes { get; set; }

        Task<bool> ExistsAsync { get; }

        string FullName { get; }

        Task<DateTime> LastWriteTimeUtcAsync { get; }

        // Properties
        string Name { get; }

        IDirectory Parent { get; }

        // Functions
        Task CreateAsync();

        Task DeleteAsync();

        Task<IEnumerable<IDirectory>> GetDirectoriesAsync();

        Task<IEnumerable<IDirectory>> GetDirectoriesAsync(string pattern);

        Task<IEnumerable<IFile>> GetFilesAsync(IEnumerable<string> mimeTypes);

        Task<IEnumerable<IFile>> GetFilesAsync(IEnumerable<string> mimeTypes, string pattern);

        Task RefreshAsync();
    }
}