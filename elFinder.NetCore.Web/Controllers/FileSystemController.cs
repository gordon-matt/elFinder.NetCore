using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using elFinder.NetCore.Drawing;
using elFinder.NetCore.Drivers.FileSystem;
using elFinder.NetCore.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace elFinder.NetCore.Web.Controllers
{
    [Route("el-finder/file-system")]
    public class FileSystemController : Controller
    {
        [HttpGet("connector")]
        public async Task<IActionResult> Connector()
        {
            var connector = GetConnector();

            var parameters = Request.Query.ToDictionary(k => k.Key, v => v.Value);

            var result = (await connector.ProcessAsync(parameters)).Value;
            if (result is FileContent)
            {
                var file = result as FileContent;
                return File(file.ContentStream, file.ContentType);
            }
            else
            {
                return Json(result);
            }
        }
        
        [HttpPost("connector")] // put/upload are HTTP POST
        public async Task<IActionResult> ConnectorPost()
        {
            var connector = GetConnector();

            var parameters = Request.Form.ToDictionary(k => k.Key, v => v.Value);

            if (Request.Form.Files.Count > 0)
            {
                var files = new List<FileContent>();
                foreach (var file in Request.Form.Files)
                {
                    files.Add(new FileContent
                    {
                        Length = file.Length,
                        ContentStream = file.OpenReadStream(),
                        ContentType = file.ContentType,
                        FileName = file.FileName
                    });
                }
                return Json((await connector.ProcessAsync(parameters, files)).Value);
            }
            else
            {
                return Json((await connector.ProcessAsync(parameters)).Value);
            }
        }

        [HttpGet("thumb/{hash}")]
        public async Task<IActionResult> Thumbs(string hash)
        {
            var connector = GetConnector();

            var result = (await connector.GetThumbnailAsync(hash)).Value;
            if (result is ImageWithMimeType)
            {
                var file = result as ImageWithMimeType;
                return File(file.ImageStream, file.MimeType);
            }
            else
            {
                return Json(result);
            }
        }

        private Connector GetConnector()
        {
            var driver = new FileSystemDriver();

            string absoluteUrl = UriHelper.BuildAbsolute(Request.Scheme, Request.Host);
            var uri = new Uri(absoluteUrl);

            var root = new RootVolume(
                Startup.MapPath("~/Files"),
                $"{uri.Scheme}://{uri.Authority}/Files/",
                $"{uri.Scheme}://{uri.Authority}/el-finder/file-system/thumb/")
            {
                //IsReadOnly = !User.IsInRole("Administrators")
                IsReadOnly = false, // Can be readonly according to user's membership permission
                IsLocked = false, // If locked, files and directories cannot be deleted, renamed or moved
                Alias = "Files", // Beautiful name given to the root/home folder
                //MaxUploadSizeInKb = 2048, // Limit imposed to user uploaded file <= 2048 KB
                //LockedFolders = new List<string>(new string[] { "Folder1" })
            };

            driver.AddRoot(root);

            return new Connector(driver)
            {
                // This allows support for the "onlyMimes" option on the client.
                MimeDetect = MimeDetectOption.Internal
            };
        }
    }
}