using Microsoft.AspNetCore.Hosting;
using System;
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
            basePath = string.IsNullOrEmpty(basePath) ? WebRootPath : basePath;

            if (string.IsNullOrEmpty(basePath))
            {
                throw new ArgumentException("elFinder PathHelper don't have WebRootPath or basePath configured.");
            }

            path = path.Replace("~/", "").TrimStart('/').Replace('/', '\\');
            return GetFullPathNormalized(Path.Combine(basePath, path));
        }
    }
}