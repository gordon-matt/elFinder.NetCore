using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace elFinder.NetCore
{
    public interface IDriver
    {
        Task<FullPath> GetFullPathAsync(string target);

        Task<JsonResult> OpenAsync(FullPath path, bool tree);

        Task<JsonResult> InitAsync(FullPath path);

        Task<JsonResult> ParentsAsync(FullPath path);

        Task<JsonResult> TreeAsync(FullPath path);

        Task<JsonResult> ListAsync(FullPath path);

        Task<JsonResult> MakeDirAsync(FullPath path, string name);

        Task<JsonResult> MakeFileAsync(FullPath path, string name);

        Task<JsonResult> RenameAsync(FullPath path, string name);

        Task<JsonResult> RemoveAsync(IEnumerable<FullPath> paths);

        Task<JsonResult> DuplicateAsync(IEnumerable<FullPath> paths);

        Task<JsonResult> GetAsync(FullPath path);

        Task<JsonResult> PutAsync(FullPath path, string content);

        Task<JsonResult> PasteAsync(FullPath dest, IEnumerable<FullPath> paths, bool isCut);

        Task<JsonResult> UploadAsync(FullPath path, IEnumerable<IFormFile> files);

        Task<JsonResult> ThumbsAsync(IEnumerable<FullPath> paths);

        Task<JsonResult> DimAsync(FullPath path);

        Task<JsonResult> ResizeAsync(FullPath path, int width, int height);

        Task<JsonResult> CropAsync(FullPath path, int x, int y, int width, int height);

        Task<JsonResult> RotateAsync(FullPath path, int degree);

        Task<IActionResult> FileAsync(FullPath path, bool download);
    }
}