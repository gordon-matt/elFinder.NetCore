using System;
using System.IO;
using System.Threading.Tasks;
using elFinder.NetCore.Models;

namespace elFinder.NetCore.Drivers
{
    public interface IFile
    {
        FileAttributes Attributes { get; set; }

        IDirectory Directory { get; }

        string DirectoryName { get; }

        Task<bool> ExistsAsync { get; }

        string Extension { get; }

        string FullName { get; }

        Task<DateTime> LastWriteTimeUtcAsync { get; }

        Task<long> LengthAsync { get; }

        string Name { get; }

        MimeType MimeType { get; }

        IFile Open(string path);

        Task<Stream> CreateAsync();

        Task DeleteAsync();

        Task<Stream> OpenReadAsync();

        Task PutAsync(string content);

        Task PutAsync(Stream stream);

        Task RefreshAsync();
    }
}