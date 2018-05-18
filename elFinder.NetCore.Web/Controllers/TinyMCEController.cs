using Microsoft.AspNetCore.Mvc;

namespace elFinder.NetCore.Web.Controllers
{
    [Route("tiny-mce")]
    public class TinyMCEController : Controller
    {
        public virtual IActionResult Index()
        {
            return View();
        }

        [Route("browse")]
        public virtual IActionResult Browse()
        {
            return View();
        }
    }
}