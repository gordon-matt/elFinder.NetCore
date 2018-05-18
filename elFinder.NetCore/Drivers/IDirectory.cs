using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace elFinder.NetCore.Drivers
{
    public interface IDirectory
    {
        // Properties
        string Name { get; }

        string FullName { get; }

        IDirectory Parent { get; }

        Task<bool> ExistsAsync { get; }

        Task<DateTime> LastWriteTimeUtcAsync { get; }

        Task<FileAttributes> AttributesAsync { get; }

        // Functions
        Task CreateAsync();

        Task DeleteAsync();

        Task<IEnumerable<IFile>> GetFilesAsync();

        Task<IEnumerable<IDirectory>> GetDirectoriesAsync();
    }
}