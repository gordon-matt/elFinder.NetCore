using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace elFinder.NetCore
{
    public interface IDriver
    {
        Task<JsonResult> ArchiveAsync(FullPath parentPath, IEnumerable<FullPath> paths, string filename, string mimeType);

        Task<JsonResult> CropAsync(FullPath path, int x, int y, int width, int height);

        Task<JsonResult> DimAsync(FullPath path);

        Task<JsonResult> DuplicateAsync(IEnumerable<FullPath> paths);

        Task<JsonResult> ExtractAsync(FullPath fullPath, bool newFolder);

        Task<IActionResult> FileAsync(FullPath path, bool download);

        Task<JsonResult> GetAsync(FullPath path);

        Task<JsonResult> InitAsync(FullPath path, IEnumerable<string> mimeTypes);

        Task<JsonResult> ListAsync(FullPath path, IEnumerable<string> intersect, IEnumerable<string> mimeTypes);

        Task<JsonResult> MakeDirAsync(FullPath path, string name, IEnumerable<string> dirs);

        Task<JsonResult> MakeFileAsync(FullPath path, string name);

        Task<JsonResult> OpenAsync(FullPath path, bool tree, IEnumerable<string> mimeTypes);

        Task<JsonResult> ParentsAsync(FullPath path);

        Task<FullPath> ParsePathAsync(string target);

        Task<JsonResult> PasteAsync(FullPath dest, IEnumerable<FullPath> paths, bool isCut, IEnumerable<string> renames, string suffix);

        Task<JsonResult> PutAsync(FullPath path, string content);

        Task<JsonResult> PutAsync(FullPath path, byte[] content);

        Task<JsonResult> RemoveAsync(IEnumerable<FullPath> paths);

        Task<JsonResult> RenameAsync(FullPath path, string name);

        Task<JsonResult> ResizeAsync(FullPath path, int width, int height);

        Task<JsonResult> RotateAsync(FullPath path, int degree);

        Task<JsonResult> SearchAsync(FullPath path, string query, IEnumerable<string> mimeTypes);

        Task<JsonResult> SizeAsync(IEnumerable<FullPath> paths);

        Task<JsonResult> ThumbsAsync(IEnumerable<FullPath> paths);

        Task<JsonResult> TreeAsync(FullPath path);

        Task<JsonResult> UploadAsync(FullPath path, IEnumerable<IFormFile> files, bool? overwrite, IEnumerable<FullPath> uploadPaths, IEnumerable<string> renames, string suffix);

        Task<JsonResult> ZipDownloadAsync(IEnumerable<FullPath> paths);

        Task<FileStreamResult> ZipDownloadAsync(FullPath cwdPath, string archivedFileKey, string downloadFileName, string mimeType);
    }
}