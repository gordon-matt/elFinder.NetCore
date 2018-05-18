using System;
using System.IO;
using System.Threading.Tasks;

namespace elFinder.NetCore.Drivers
{
    public interface IFile
    {
        // Properties
        string Name { get; }

        string FullName { get; }

        string DirectoryName { get; }

        IDirectory Directory { get; }

        string Extension { get; }

        Task<bool> ExistsAsync { get; }

        Task<long> LengthAsync { get; }

        Task<DateTime> LastWriteTimeUtcAsync { get; }

        Task<FileAttributes> AttributesAsync { get; }

        // Functions
        IFile Clone(string path);

        Task<Stream> CreateAsync();

        Task<Stream> OpenReadAsync();

        Task PutAsync(string content);

        Task PutAsync(Stream stream);

        Task DeleteAsync();
    }
}