using System.Runtime.InteropServices;
using elFinder.NetCore.Drivers.FileSystem;
using Microsoft.AspNetCore.Http.Extensions;
using ThumbGen;

namespace elFinder.NetCore.Web.Controllers;

[Route("el-finder/file-system")]
public class FileSystemController : Controller
{
    private readonly IWebHostEnvironment _env;
    public FileSystemController(IWebHostEnvironment env)
    {
        // Setup for video thumbnails here
        _env = env;
        // We need the FFmpeg libs for ThumbGen https://github.com/thesn10/ThumbGen
        // Here we set the path to the libraries.
        bool isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        bool isOsx = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        string ffmpegLibs = env.IsDevelopment()
            ? Path.GetFullPath(".\\bin\\Debug\\net10.0\\runtimes\\win-x64\\native\\")
            : Path.GetFullPath(".\\runtimes\\win-x64\\native\\");
        if (isLinux) { ffmpegLibs = "/usr/lib/x86_64-linux-gnu/"; }
        if (isOsx) { ffmpegLibs = "/usr/local/lib/"; } // /opt/homebrew/lib/
        try
        {
            FFmpegBindings.Initialize(ffmpegLibs);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    [Route("connector")]
    public async Task<IActionResult> Connector()
    {
        try
        {
            var connector = GetConnector();
            return await connector.ProcessAsync(Request);
        }
        catch (Exception ex)
        {
            // TODO: would be good to Sanitize Exception Message at least in production, can leak server file paths
            return Json(new { error = "Unable to process your request: " + ex.Message });
        }
    }

    [Route("thumb/{hash}")]
    public async Task<IActionResult> Thumbs(string hash)
    {
        try
        {
            var connector = GetConnector();
            return await connector.GetThumbnailAsync(HttpContext.Request, HttpContext.Response, hash);
        }
        catch (Exception ex)
        {
            // TODO: would be good to Sanitize Exception Message at least in production, can leak server file paths
            return Json(new { error = "Unable to process your request: " + ex.Message });
        }
    }

    private Connector GetConnector()
    {
        var driver = new FileSystemDriver();

        string absoluteUrl = UriHelper.BuildAbsolute(Request.Scheme, Request.Host);
        var uri = new Uri(absoluteUrl);

        var root = new RootVolume(
            PathHelper.MapPath("~/Files"),
            $"{uri.Scheme}://{uri.Authority}/Files/",
            $"{uri.Scheme}://{uri.Authority}/el-finder/file-system/thumb/")
        {
            //IsReadOnly = !User.IsInRole("Administrators")
            IsReadOnly = false, // Can be readonly according to user's membership permission
            IsLocked = false, // If locked, files and directories cannot be deleted, renamed or moved
            Alias = "Files", // Beautiful name given to the root/home folder
            MaxUploadSizeInMb = 2048, // Limit imposed to user uploaded file <= 2048 KB
            AccessControlAttributes = new HashSet<NamedAccessControlAttributeSet>()
            {
                new(PathHelper.MapPath("~/Files/readonly.txt"))
                {
                    Write = false,
                    Locked = true
                },
                new(PathHelper.MapPath("~/Files/Prohibited"))
                {
                    Read = false,
                    Write = false,
                    Locked = true
                },
                new(PathHelper.MapPath("~/Files/Parent/Children"))
                {
                    Read = true,
                    Write = false,
                    Locked = true
                }
            },
            // Upload file type constraints
            UploadAllow = ["image", "text", "video", "audio", "application/x-extension-fcstd"],
            //UploadDeny = ["text/csv"],
            //UploadOrder = ["allow", "deny"]
        };

        driver.AddRoot(root);

        var connector = new Connector(driver)
        {
            // This allows support for the "onlyMimes" option on the client.
            MimeDetect = MimeDetectOption.Internal
        };

        // Remote fetching for the "put" command (encoding=scheme, used by some online editors) is disabled by default
        // to prevent SSRF. Opt in by adding the trusted hosts, e.g.:
        //connector.AllowedPutSchemeHosts.Add("cdn.example.com");
        //connector.MaxPutSchemeBytes = 5 * 1024 * 1024;

        return connector;
    }
}