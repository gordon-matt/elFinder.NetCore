using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace elFinder.NetCore
{
    public interface IDriver
    {
        Task<JsonResult> OpenAsync(string target, bool tree);

        Task<JsonResult> InitAsync(string target);

        Task<JsonResult> ParentsAsync(string target);

        Task<JsonResult> TreeAsync(string target);

        Task<JsonResult> ListAsync(string target);

        Task<JsonResult> MakeDirAsync(string target, string name);

        Task<JsonResult> MakeFileAsync(string target, string name);

        Task<JsonResult> RenameAsync(string target, string name);

        Task<JsonResult> RemoveAsync(IEnumerable<string> targets);

        Task<JsonResult> DuplicateAsync(IEnumerable<string> targets);

        Task<JsonResult> GetAsync(string target);

        Task<JsonResult> PutAsync(string target, string content);

        Task<JsonResult> PasteAsync(string dest, IEnumerable<string> targets, bool isCut);

        Task<JsonResult> UploadAsync(string target, IEnumerable<IFormFile> targets);

        Task<JsonResult> ThumbsAsync(IEnumerable<string> targets);

        Task<JsonResult> DimAsync(string target);

        Task<JsonResult> ResizeAsync(string target, int width, int height);

        Task<JsonResult> CropAsync(string target, int x, int y, int width, int height);

        Task<JsonResult> RotateAsync(string target, int degree);

        Task<IActionResult> FileAsync(string target, bool download);

        FullPath ParsePath(string target);
    }
}