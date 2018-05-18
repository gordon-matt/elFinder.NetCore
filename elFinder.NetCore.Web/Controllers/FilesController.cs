using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace elFinder.NetCore.Web.Controllers
{
    [Route("file")]
    public partial class FilesController : Controller
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

            var root = new Root(
                new DirectoryInfo(Startup.MapPath("~/Files")),
                string.Format("http://{0}/Files/", uri.Authority),
                string.Format("http://{0}/file/thumb/", uri.Authority))
            {
                // Sample using ASP.NET built in Membership functionality...
                // Only the super user can READ (download files) & WRITE (create folders/files/upload files).
                // Other users can only READ (download files)
                // IsReadOnly = !User.IsInRole(AccountController.SuperUser)

                IsReadOnly = false, // Can be readonly according to user's membership permission
                Alias = "Files", // Beautiful name given to the root/home folder
                MaxUploadSizeInKb = 500, // Limit imposed to user uploaded file <= 500 KB
                //LockedFolders = new List<string>(new string[] { "Folder1" })
            };

            //// Was a subfolder selected in Home Index page?
            //if (!string.IsNullOrEmpty(subFolder))
            //{
            //    root.StartPath = new DirectoryInfo(Startup.MapPath("~/Files/" + folder + "/" + subFolder));
            //}

            driver.AddRoot(root);

            return new Connector(driver);
        }

        //[Route("select-file")]
        //public virtual IActionResult SelectFile(string target)
        //{
        //    var driver = new FileSystemDriver();

        //    string absoluteUrl = UriHelper.BuildAbsolute(Request.Scheme, Request.Host);
        //    var uri = new Uri(absoluteUrl);

        //    driver.AddRoot(
        //        new Root(
        //            new DirectoryInfo(Startup.MapPath("~/Files")),
        //            "http://" + uri.Authority + "/Files")
        //        { IsReadOnly = false });

        //    var connector = new Connector(driver);

        //    return Json(connector.GetFileByHash(target).FullName);
        //}
    }
}