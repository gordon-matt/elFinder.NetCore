namespace elFinder.NetCore.Web;

public static class PathHelper
{
    public static string WebRootPath { get; set; }

    public static string GetFullPathNormalized(string path) => Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));

    public static string MapPath(string path, string basePath = null)
    {
        basePath = string.IsNullOrEmpty(basePath) ? WebRootPath : basePath;

        if (string.IsNullOrEmpty(basePath))
        {
            throw new ArgumentException("PathHelper does not have WebRootPath or basePath configured.");
        }

        path = path.Replace("~/", "").TrimStart('/').Replace('/', '\\');
        return GetFullPathNormalized(Path.Combine(basePath, path));
    }
}