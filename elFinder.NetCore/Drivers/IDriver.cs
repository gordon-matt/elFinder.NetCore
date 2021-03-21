using elFinder.NetCore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace elFinder.NetCore
{
    public interface IDriver
    {
        Task<ConnectorResult> ArchiveAsync(FullPath parentPath, IEnumerable<FullPath> paths, string filename, string mimeType);

        Task<ConnectorResult> CropAsync(FullPath path, int x, int y, int width, int height);

        Task<ConnectorResult> DimAsync(FullPath path);

        Task<ConnectorResult> DuplicateAsync(IEnumerable<FullPath> paths);

        Task<ConnectorResult> ExtractAsync(FullPath fullPath, bool newFolder);

        Task<ConnectorResult> FileAsync(FullPath path, bool download);

        Task<ConnectorResult> GetAsync(FullPath path);

        Task<ConnectorResult> InitAsync(FullPath path, IEnumerable<string> mimeTypes);

        Task<ConnectorResult> ListAsync(FullPath path, IEnumerable<string> intersect, IEnumerable<string> mimeTypes);

        Task<ConnectorResult> MakeDirAsync(FullPath path, string name, IEnumerable<string> dirs);

        Task<ConnectorResult> MakeFileAsync(FullPath path, string name);

        Task<ConnectorResult> OpenAsync(FullPath path, bool tree, IEnumerable<string> mimeTypes);

        Task<ConnectorResult> ParentsAsync(FullPath path);

        Task<FullPath> ParsePathAsync(string target);

        Task<ConnectorResult> PasteAsync(FullPath dest, IEnumerable<FullPath> paths, bool isCut, IEnumerable<string> renames, string suffix);

        Task<ConnectorResult> PutAsync(FullPath path, string content);

        Task<ConnectorResult> PutAsync(FullPath path, byte[] content);

        Task<ConnectorResult> RemoveAsync(IEnumerable<FullPath> paths);

        Task<ConnectorResult> RenameAsync(FullPath path, string name);

        Task<ConnectorResult> ResizeAsync(FullPath path, int width, int height);

        Task<ConnectorResult> RotateAsync(FullPath path, int degree);

        Task<ConnectorResult> SearchAsync(FullPath path, string query, IEnumerable<string> mimeTypes);

        Task<ConnectorResult> SizeAsync(IEnumerable<FullPath> paths);

        Task<ConnectorResult> ThumbsAsync(IEnumerable<FullPath> paths);

        Task<ConnectorResult> TreeAsync(FullPath path);

        Task<ConnectorResult> UploadAsync(FullPath path, IList<FileContent> files, bool? overwrite, IEnumerable<FullPath> uploadPaths, IEnumerable<string> renames, string suffix);
    }
}