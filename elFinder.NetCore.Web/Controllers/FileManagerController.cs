using Microsoft.AspNetCore.Mvc;

namespace elFinder.NetCore.Web.Controllers
{
    [Route("file-manager")]
    public class FileManagerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}