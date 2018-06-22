using System;
using System.IO;
using System.Threading.Tasks;
using elFinder.NetCore.Drivers.AzureStorage;
using elFinder.NetCore.Helpers;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace elFinder.NetCore.Web.Controllers
{
    [Route("el-finder/azure-storage")]
    public class AzureStorageController : Controller
    {
        [Route("connector")]
        public async Task<IActionResult> Connector()
        {
            var connector = GetConnector();
            return await connector.ProcessAsync(Request);
        }

        [Route("thumb/{id}")]
        public async Task<IActionResult> Thumbs(string id)
        {
            var connector = GetConnector();
            return await connector.GetThumbnailAsync(HttpContext.Request, HttpContext.Response, id);
        }

        [Route("files/{*path}")]
        public async Task<IActionResult> Files(string path)
        {
            var file = await AzureStorageAPI.FileStreamAsync(path);
            return new FileStreamResult(file, MimeHelper.GetMimeType(Path.GetExtension(path).Substring(1)));
        }

        private Connector GetConnector()
        {
            var driver = new AzureStorageDriver();

            string absoluteUrl = UriHelper.BuildAbsolute(Request.Scheme, Request.Host);
            var uri = new Uri(absoluteUrl);

            string rootDirectory = "test"; // TODO: Change this to the name of your own Azure file share.

            var root = new RootVolume(
                rootDirectory,
                $"http://{uri.Authority}/el-finder/azure-storage/files/{rootDirectory}/",
                $"http://{uri.Authority}/el-finder/azure-storage/thumb/",
                '/')
            {
                //IsReadOnly = !User.IsInRole("Administrators")
                IsReadOnly = false, // Can be readonly according to user's membership permission
				IsLocked = false, // If locked, files and directories cannot be deleted, renamed or moved
				Alias = "Files", // Beautiful name given to the root/home folder
                MaxUploadSizeInKb = 2048, // Limit imposed to user uploaded file <= 2048 KB
                //LockedFolders = new List<string>(new string[] { "Folder1" })
            };

            driver.AddRoot(root);

            return new Connector(driver);
        }
    }
}