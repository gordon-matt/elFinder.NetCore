namespace elFinder.NetCore.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    internal static class Mime
    {
        private static Dictionary<string, string> mimeTypes;

        static Mime()
        {
            mimeTypes = new Dictionary<string, string>();
            var assembly = typeof(Mime).GetTypeInfo().Assembly;

            using (var stream = assembly.GetManifestResourceStream("elFinder.NetCore.mimeTypes.txt"))
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();

                    if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                    {
                        continue;
                    }

                    var parts = line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length > 1)
                    {
                        var mime = parts[0];

                        for (var i = 1; i < parts.Length; i++)
                        {
                            var ext = parts[i].ToLower();
                            if (!mimeTypes.ContainsKey(ext))
                            {
                                mimeTypes.Add(ext, mime);
                            }
                        }
                    }
                }
            }
        }

        public static string GetMimeType(string extension)
        {
            if (mimeTypes.ContainsKey(extension))
            {
                return mimeTypes[extension];
            }

            return "unknown";
        }
    }
}