using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace elFinder.NetCore.Web
{
    public static class PathHelper
    {
        public static string WebRootPath { get; set; }

        public static string GetFullPathNormalized(string path)
        {
            return Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
        }

        public static string MapPath(string path, string basePath = null)
        {
            if (string.IsNullOrEmpty(basePath))
            {
                //basePath = Startup.WebRootPath;
                basePath = WebRootPath;
            }

            path = path.Replace("~/", "").TrimStart('/').Replace('/', '\\');
            return GetFullPathNormalized(Path.Combine(basePath, path));
        }
    }
}