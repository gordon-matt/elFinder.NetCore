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
            return await connector.Process(Request);
        }

        [Route("thumb/{id}")]
        public async Task<IActionResult> Thumbs(string id)
        {
            var connector = GetConnector();
            return await connector.GetThumbnail(HttpContext.Request, HttpContext.Response, id);
        }

        [Route("files/{*path}")]
        public async Task<IActionResult> Files(string path)
        {
            var driver = new AzureStorageDriver();
            var file = await AzureStorageAPI.FileStream(path);
            return new FileStreamResult(file, Mime.GetMimeType(Path.GetExtension(path).Substring(1)));
        }

        private Connector GetConnector()
        {
            var driver = new AzureStorageDriver();

            string absoluteUrl = UriHelper.BuildAbsolute(Request.Scheme, Request.Host);
            var uri = new Uri(absoluteUrl);

            var root = new RootVolume(
                "test",
                $"http://{uri.Authority}/el-finder/azure-storage/files/test/",
                $"http://{uri.Authority}/el-finder/azure-storage/thumb/")
            {
                //IsReadOnly = !User.IsInRole("Administrators")
                IsReadOnly = false, // Can be readonly according to user's membership permission
                Alias = "Files", // Beautiful name given to the root/home folder
                MaxUploadSizeInKb = 500, // Limit imposed to user uploaded file <= 500 KB
                //LockedFolders = new List<string>(new string[] { "Folder1" })
            };

            driver.AddRoot(root);

            return new Connector(driver);
        }
    }
}