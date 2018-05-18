using System;
using System.IO;
using System.Threading.Tasks;
using elFinder.NetCore.Drivers.FileSystem;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace elFinder.NetCore.Web.Controllers
{
    [Route("el-finder/file-system")]
    public class FileSystemController : Controller
    {
        [Route("connector")]
        public virtual async Task<IActionResult> Index()
        {
            var connector = GetConnector();
            return await connector.Process(HttpContext.Request);
        }

        [Route("thumb/{hash}")]
        public IActionResult Thumbs(string hash)
        {
            var connector = GetConnector();
            return connector.GetThumbnail(HttpContext.Request, HttpContext.Response, hash);
        }

        private Connector GetConnector()
        {
            var driver = new FileSystemDriver();

            string absoluteUrl = UriHelper.BuildAbsolute(Request.Scheme, Request.Host);
            var uri = new Uri(absoluteUrl);

            var root = new RootVolume(
                new DirectoryInfo(Startup.MapPath("~/Files")),
                $"http://{uri.Authority}/Files/",
                $"http://{uri.Authority}/el-finder/file-system/thumb/")
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