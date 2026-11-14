using System.Globalization;
using elFinder.NetCore.Drivers;
using elFinder.NetCore.Exceptions;
using elFinder.NetCore.Helpers;
using Microsoft.Extensions.Primitives;

namespace elFinder.NetCore;

/// <summary>
/// Represents a connector which processes elFinder requests
/// </summary>
public class Connector
{
    private static readonly HttpClient httpClient = new() { Timeout = TimeSpan.FromSeconds(30) };
    private readonly IDriver driver;

    public Connector(IDriver driver)
    {
        this.driver = driver;
    }

    /// <summary>
    /// Hosts from which the connector is allowed to fetch remote content for the "put" command when
    /// encoding is "scheme" (e.g. saving an image from the online editors). Empty by default, which
    /// disables remote fetching entirely to prevent SSRF. Host names are matched case-insensitively.
    /// </summary>
    public ICollection<string> AllowedPutSchemeHosts { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Maximum number of bytes that will be downloaded for a "put" command with encoding "scheme". Defaults to 10 MB.
    /// </summary>
    public long MaxPutSchemeBytes { get; set; } = 10 * 1024 * 1024;

    public MimeDetectOption MimeDetect { get; set; } = MimeDetectOption.Auto;

    /// <summary>
    /// Get actual filesystem path by hash
    /// </summary>
    /// <param name="hash">Hash of file or directory</param>
    public async Task<IFile> GetFileByHashAsync(string hash)
    {
        var path = await driver.ParsePathAsync(hash);
        return !path.IsDirectory ? path.File : null;
    }

    public async Task<IActionResult> GetThumbnailAsync(HttpRequest request, HttpResponse response, string hash)
    {
        if (hash != null)
        {
            var path = await driver.ParsePathAsync(hash);
            if (!path.IsDirectory && path.RootVolume.CanCreateThumbnail(path.File))
            {
                //if (!await HttpCacheHelper.IsFileFromCache(path.File, request, response))
                //{
                var thumb = await path.GenerateThumbnailAsync(request.HttpContext.RequestAborted);
                return thumb == null ? new EmptyResult() : new FileStreamResult(thumb.ImageStream, thumb.MimeType);
                //}
                //else
                //{
                //	response.ContentType = Utils.GetMimeType(path.RootVolume.PictureEditor.ConvertThumbnailExtension(path.File.Extension));
                //response.End();
                //}
            }
        }
        return new EmptyResult();
    }

    public async Task<IActionResult> ProcessAsync(HttpRequest request)
    {
        try
        {
            return await ProcessCoreAsync(request);
        }
        catch (FileNotFoundException)
        {
            return Error.FileNotFound();
        }
        catch (DirectoryNotFoundException)
        {
            return Error.FolderNotFound();
        }
        catch (FileTypeNotAllowedException)
        {
            return Error.FileTypeNotAllowed();
        }
    }

    protected async Task<IActionResult> ProcessCoreAsync(HttpRequest request)
    {
        var parameters = request.Query.Any()
            ? request.Query.ToDictionary(k => k.Key, v => v.Value)
            : request.Form.ToDictionary(k => k.Key, v => v.Value);

        string cmd = parameters.GetValueOrDefault("cmd");
        if (string.IsNullOrEmpty(cmd))
        {
            return Error.UnknownCommand();
        }

        switch (cmd)
        {
            case "archive":
                {
                    var parentPath = await driver.ParsePathAsync(parameters.GetValueOrDefault("target"));
                    var paths = await GetFullPathArrayAsync(parameters.GetValueOrDefault("targets[]"));
                    var name = parameters.GetValueOrDefault("name");
                    var type = parameters.GetValueOrDefault("type");
                    return await driver.ArchiveAsync(parentPath, paths, name, type);
                }
            case "dim":
                {
                    var path = await driver.ParsePathAsync(parameters.GetValueOrDefault("target"));
                    return await driver.DimAsync(path);
                }
            case "duplicate":
                {
                    var targets = await GetFullPathArrayAsync(parameters.GetValueOrDefault("targets[]"));
                    return await driver.DuplicateAsync(targets);
                }
            case "extract":
                {
                    var fullPath = await driver.ParsePathAsync(parameters.GetValueOrDefault("target"));
                    var makedir = parameters.GetValueOrDefault("makedir");
                    return await driver.ExtractAsync(fullPath, makedir == "1");
                }
            case "file":
                {
                    var cPath = parameters.GetValueOrDefault("cpath");
                    var reqId = parameters.GetValueOrDefault("reqid");
                    var path = await driver.ParsePathAsync(parameters.GetValueOrDefault("target"));

                    var result = await driver.FileAsync(path, parameters.GetValueOrDefault("download") == "1");

                    request.HttpContext.Response.Headers.XContentTypeOptions = "nosniff";

                    if (!string.IsNullOrEmpty(cPath))
                    {
                        // API >= 2.1.39
                        request.HttpContext.Response.Cookies.Append($"elfdl{reqId}", "1");
                    }

                    return result;
                }
            case "get":
                {
                    var path = await driver.ParsePathAsync(parameters.GetValueOrDefault("target"));
                    return await driver.GetAsync(path);
                }
            case "ls":
                {
                    var path = await driver.ParsePathAsync(parameters.GetValueOrDefault("target"));
                    var intersects = parameters.GetValueOrDefault("intersect[]");

                    var mimeTypes = MimeDetect == MimeDetectOption.Internal
                        ? parameters.GetValueOrDefault("mimes[]")
                        : default;

                    return await driver.ListAsync(path, intersects, mimeTypes);
                }
            case "mkdir":
                {
                    var path = await driver.ParsePathAsync(parameters.GetValueOrDefault("target"));
                    var name = parameters.GetValueOrDefault("name");
                    var dirs = parameters.GetValueOrDefault("dirs[]");
                    return await driver.MakeDirAsync(path, name, dirs);
                }
            case "mkfile":
                {
                    var path = await driver.ParsePathAsync(parameters.GetValueOrDefault("target"));
                    var name = parameters.GetValueOrDefault("name");
                    return await driver.MakeFileAsync(path, name);
                }
            case "open":
                {
                    var path = await driver.ParsePathAsync(parameters.GetValueOrDefault("target"));

                    var mimeTypes = MimeDetect == MimeDetectOption.Internal
                        ? parameters.GetValueOrDefault("mimes[]")
                        : default;

                    return parameters.GetValueOrDefault("init") == "1"
                        ? await driver.InitAsync(path, mimeTypes)
                        : await driver.OpenAsync(path, parameters.GetValueOrDefault("tree") == "1", mimeTypes);
                }
            case "parents":
                {
                    var path = await driver.ParsePathAsync(parameters.GetValueOrDefault("target"));
                    return await driver.ParentsAsync(path);
                }
            case "paste":
                {
                    var paths = await GetFullPathArrayAsync(parameters.GetValueOrDefault("targets[]"));
                    var dst = parameters.GetValueOrDefault("dst");
                    bool cut = parameters.GetValueOrDefault("cut") == "1";
                    var renames = parameters.GetValueOrDefault("ranames[]");
                    var suffix = parameters.GetValueOrDefault("suffix");
                    return await driver.PasteAsync(await driver.ParsePathAsync(dst), paths, cut, renames, suffix);
                }
            case "put":
                {
                    var path = await driver.ParsePathAsync(parameters.GetValueOrDefault("target"));
                    var content = parameters.GetValueOrDefault("content");
                    var encoding = parameters.GetValueOrDefault("encoding");

                    if (encoding == "scheme")
                    {
                        if (!Uri.TryCreate(content, UriKind.Absolute, out var uri) ||
                            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
                            !AllowedPutSchemeHosts.Contains(uri.Host))
                        {
                            return Error.AccessDenied();
                        }

                        byte[] data = await DownloadWithLimitAsync(uri, MaxPutSchemeBytes);
                        return data == null ? Error.UploadFileTooLarge() : await driver.PutAsync(path, data);
                    }
                    else
                    {
                        return await driver.PutAsync(path, content);
                    }
                }
            case "rename":
                {
                    var path = await driver.ParsePathAsync(parameters.GetValueOrDefault("target"));
                    var name = parameters.GetValueOrDefault("name");
                    return await driver.RenameAsync(path, name);
                }
            case "resize":
                {
                    var path = await driver.ParsePathAsync(parameters.GetValueOrDefault("target"));
                    switch ((string)parameters.GetValueOrDefault("mode"))
                    {
                        case "resize":
                            {
                                return
                                    !TryGetInt(parameters, "width", out int width) ||
                                    !TryGetInt(parameters, "height", out int height) ||
                                    width <= 0 || height <= 0
                                        ? Error.InvalidCommandParams(cmd)
                                        : await driver.ResizeAsync(path, width, height);
                            }
                        case "crop":
                            {
                                return
                                    !TryGetInt(parameters, "x", out int x) ||
                                    !TryGetInt(parameters, "y", out int y) ||
                                    !TryGetInt(parameters, "width", out int width) ||
                                    !TryGetInt(parameters, "height", out int height) ||
                                    x < 0 || y < 0 || width <= 0 || height <= 0
                                        ? Error.InvalidCommandParams(cmd)
                                        : await driver.CropAsync(path, x, y, width, height);
                            }
                        case "rotate":
                            {
                                return !TryGetInt(parameters, "degree", out int degree)
                                    ? Error.InvalidCommandParams(cmd)
                                    : await driver.RotateAsync(path, degree);
                            }
                        default:
                            return Error.UnknownCommand();
                    }
                }
            case "rm":
                {
                    var paths = await GetFullPathArrayAsync(parameters.GetValueOrDefault("targets[]"));
                    return await driver.RemoveAsync(paths);
                }
            case "search":
                {
                    var path = await driver.ParsePathAsync(parameters.GetValueOrDefault("target"));
                    var query = parameters.GetValueOrDefault("q");

                    var mimeTypes = MimeDetect == MimeDetectOption.Internal
                        ? parameters.GetValueOrDefault("mimes[]")
                        : default;

                    return await driver.SearchAsync(path, query, mimeTypes);
                }
            case "size":
                {
                    var paths = new StringValues(parameters.Where(p => p.Key.StartsWith("target")).Select(p => (string)p.Value).ToArray());
                    return await driver.SizeAsync(await GetFullPathArrayAsync(paths));
                }
            case "tmb":
                {
                    var targets = await GetFullPathArrayAsync(parameters.GetValueOrDefault("targets[]"));
                    return await driver.ThumbsAsync(targets);
                }
            case "tree":
                {
                    var path = await driver.ParsePathAsync(parameters.GetValueOrDefault("target"));
                    return await driver.TreeAsync(path);
                }

            case "upload":
                {
                    var path = await driver.ParsePathAsync(parameters.GetValueOrDefault("target"));
                    var uploadPath = await GetFullPathArrayAsync(parameters.GetValueOrDefault("upload_path[]"));
                    bool overwrite = parameters.GetValueOrDefault("overwrite") != "0";
                    var renames = parameters.GetValueOrDefault("renames[]");
                    var suffix = parameters.GetValueOrDefault("suffix");
                    return await driver.UploadAsync(path, request.Form.Files, overwrite, uploadPath, renames, suffix);
                }
            case "zipdl":
                {
                    var targets = parameters.GetValueOrDefault("targets[]");
                    var download = parameters.GetValueOrDefault("download");

                    if (download != "1")
                    {
                        var targetPaths = await GetFullPathArrayAsync(targets);
                        return await driver.ZipDownloadAsync(targetPaths);
                    }

                    var cwdPath = await driver.ParsePathAsync(targets[0]);
                    string archiveFileKey = targets[1];
                    string downloadFileName = targets[2];
                    string mimeType = targets[3];

                    return await driver.ZipDownloadAsync(cwdPath, archiveFileKey, downloadFileName, mimeType);
                }
            default: return Error.UnknownCommand();
        }
    }

    private static async Task<byte[]> DownloadWithLimitAsync(Uri uri, long maxBytes)
    {
        using var response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        if (response.Content.Headers.ContentLength > maxBytes)
        {
            return null;
        }

        using var source = await response.Content.ReadAsStreamAsync();
        using var destination = new MemoryStream();
        byte[] buffer = new byte[81920];
        int read;
        while ((read = await source.ReadAsync(buffer)) > 0)
        {
            if (destination.Length + read > maxBytes)
            {
                return null;
            }
            destination.Write(buffer, 0, read);
        }

        return destination.ToArray();
    }

    private static bool TryGetInt(Dictionary<string, StringValues> parameters, string key, out int value) =>
        int.TryParse(parameters.GetValueOrDefault(key).ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);

    private async Task<IEnumerable<FullPath>> GetFullPathArrayAsync(StringValues targets)
    {
        var tasks = targets.Select(driver.ParsePathAsync);
        return await Task.WhenAll(tasks);
    }
}