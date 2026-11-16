using elFinder.NetCore.Drivers.FileSystem;
using elFinder.NetCore.Ffmpeg;
using elFinder.NetCore.Helpers;
using Microsoft.AspNetCore.Http.Extensions;

namespace elFinder.NetCore.Web.Controllers;

[Route("el-finder/file-system")]
public class FileSystemController : Controller
{
    private readonly IWebHostEnvironment env;
    private readonly IConfiguration config;
    private readonly ILogger<FileSystemController> logger;

    public FileSystemController(IWebHostEnvironment env, IConfiguration config, ILogger<FileSystemController> logger)
    {
        this.env = env;
        this.config = config;
        this.logger = logger;

        // Extensions listed under elFinder:MimeTypeMap override the built-in map and can add new ones.
        MimeHelper.Configure(config);

        // Video thumbnails need ffmpeg and ffprobe. An official build is downloaded into ./ffmpeg
        // only when neither is already installed. That folder is gitignored.
        try
        {
            FfmpegBootstrap.EnsureAsync(
                new FfmpegBootstrapOptions
                {
                    PreferLinuxSystemDirectory = true,
                    SearchWellKnownLocations = true,
                    SearchApplicationDirectories = true,
                    DownloadDirectory = Path.Combine(env.ContentRootPath, "ffmpeg"),
                    DownloadIfMissing = true,
                    RequireFfprobe = true
                },
                logger).Wait();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "FFmpeg setup failed. Video thumbnails are disabled.");
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
            //MaxUploadSizeInKb = 2048, // Limit imposed to user uploaded file <= 2048 KB
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
            //UploadAllow = new[] { "image", "text" },
            //UploadDeny = new[] { "text/csv" },
            //UploadOrder = new[] { "allow", "deny" }
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