using Microsoft.AspNetCore.Mvc;

namespace elFinder.NetCore.Web.Controllers
{
    public partial class HomeController : Controller
    {
        [Route("")]
        public virtual IActionResult Index()
        {
            return View();
        }

        [Route("file-manager")]
        public virtual IActionResult Files()
        {
            return View();
        }
    }
}