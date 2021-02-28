using elFinder.NetCore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace elFinder.NetCore
{
    public interface IDriver
    {
        Task<object> ArchiveAsync(FullPath parentPath, IEnumerable<FullPath> paths, string filename, string mimeType);

        Task<object> CropAsync(FullPath path, int x, int y, int width, int height);

        Task<object> DimAsync(FullPath path);

        Task<object> DuplicateAsync(IEnumerable<FullPath> paths);

        Task<object> ExtractAsync(FullPath fullPath, bool newFolder);

        Task<object> FileAsync(FullPath path, bool download);

        Task<object> GetAsync(FullPath path);

        Task<object> InitAsync(FullPath path, IEnumerable<string> mimeTypes);

        Task<object> ListAsync(FullPath path, IEnumerable<string> intersect, IEnumerable<string> mimeTypes);

        Task<object> MakeDirAsync(FullPath path, string name, IEnumerable<string> dirs);

        Task<object> MakeFileAsync(FullPath path, string name);

        Task<object> OpenAsync(FullPath path, bool tree, IEnumerable<string> mimeTypes);

        Task<object> ParentsAsync(FullPath path);

        Task<FullPath> ParsePathAsync(string target);

        Task<object> PasteAsync(FullPath dest, IEnumerable<FullPath> paths, bool isCut, IEnumerable<string> renames, string suffix);

        Task<object> PutAsync(FullPath path, string content);

        Task<object> PutAsync(FullPath path, byte[] content);

        Task<object> RemoveAsync(IEnumerable<FullPath> paths);

        Task<object> RenameAsync(FullPath path, string name);

        Task<object> ResizeAsync(FullPath path, int width, int height);

        Task<object> RotateAsync(FullPath path, int degree);

        Task<object> SearchAsync(FullPath path, string query, IEnumerable<string> mimeTypes);

        Task<object> SizeAsync(IEnumerable<FullPath> paths);

        Task<object> ThumbsAsync(IEnumerable<FullPath> paths);

        Task<object> TreeAsync(FullPath path);

        Task<object> UploadAsync(FullPath path, IList<FileContent> files, bool? overwrite, IEnumerable<FullPath> uploadPaths, IEnumerable<string> renames, string suffix);
    }
}