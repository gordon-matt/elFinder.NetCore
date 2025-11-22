namespace elFinder.NetCore.Web.Controllers;

[Route("tiny-mce")]
public class TinyMCEController : Controller
{
    public IActionResult Index() => View();

    [Route("browse")]
    public IActionResult Browse() => View();
}