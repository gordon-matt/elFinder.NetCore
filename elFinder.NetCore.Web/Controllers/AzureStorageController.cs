using System;
using System.Threading.Tasks;
using elFinder.NetCore.Drivers.AzureStorage;
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

        private Connector GetConnector()
        {
            var driver = new AzureStorageDriver();

            string absoluteUrl = UriHelper.BuildAbsolute(Request.Scheme, Request.Host);
            var uri = new Uri(absoluteUrl);

            var root = new RootVolume(
                "test",
                $"http://{uri.Authority}/Files/",
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