using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace elFinder.NetCore.Helpers
{
    public static class PathHelper
    {
        public static string GetFullPathNormalized(string path)
        {
            return Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(path));
        }
    }
}
