using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace elFinder.NetCore
{
    public interface IDriver
    {
        Task<JsonResult> Open(string target, bool tree);

        Task<JsonResult> Init(string target);

        Task<JsonResult> Parents(string target);

        Task<JsonResult> Tree(string target);

        Task<JsonResult> List(string target);

        Task<JsonResult> MakeDir(string target, string name);

        Task<JsonResult> MakeFile(string target, string name);

        Task<JsonResult> Rename(string target, string name);

        Task<JsonResult> Remove(IEnumerable<string> targets);

        Task<JsonResult> Duplicate(IEnumerable<string> targets);

        Task<JsonResult> Get(string target);

        Task<JsonResult> Put(string target, string content);

        Task<JsonResult> Paste(string source, string dest, IEnumerable<string> targets, bool isCut);

        Task<JsonResult> Upload(string target, IEnumerable<IFormFile> targets);

        Task<JsonResult> Thumbs(IEnumerable<string> targets);

        Task<JsonResult> Dim(string target);

        Task<JsonResult> Resize(string target, int width, int height);

        Task<JsonResult> Crop(string target, int x, int y, int width, int height);

        Task<JsonResult> Rotate(string target, int degree);

        Task<IActionResult> File(string target, bool download);

        FullPath ParsePath(string target);
    }
}