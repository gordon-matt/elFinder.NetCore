using System.IO;
using System.Linq;
using elFinder.NetCore.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace elFinder.NetCore.Web.Controllers
{
    public partial class HomeController : Controller
    {
        [Route("")]
        public virtual IActionResult Index()
        {
            var directoryInfo = new DirectoryInfo(Startup.MapPath("~/Files/MyFolder"));

            var folders = directoryInfo.GetDirectories()
                .ToList()
                .Select(d => d.Name);

            return View(folders);
        }

        [Route("file-manager/{subFolder?}")]
        public virtual IActionResult Files(string subFolder)
        {
            var model = new FileModel()
            {
                Folder = "MyFolder",
                SubFolder = subFolder
            };

            return View(model);
        }
    }
}