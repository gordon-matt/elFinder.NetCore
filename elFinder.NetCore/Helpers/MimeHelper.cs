namespace elFinder.NetCore.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    public static class MimeHelper
    {
        private static readonly Dictionary<string, string> mimeTypes;

        static MimeHelper()
        {
            mimeTypes = new Dictionary<string, string>();
            var assembly = typeof(MimeHelper).GetTypeInfo().Assembly;

            using var stream = assembly.GetManifestResourceStream("elFinder.NetCore.MimeTypes.txt");
            using var reader = new StreamReader(stream);
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
                    string mime = parts[0];

                    for (int i = 1; i < parts.Length; i++)
                    {
                        string ext = parts[i].ToLower();
                        if (!mimeTypes.ContainsKey(ext))
                        {
                            mimeTypes.Add(ext, mime);
                        }
                    }
                }
            }
        }

        public static string GetMimeType(string extension)
        {
            string ext = extension.ToLower();
            if (ext.StartsWith("."))
            {
                ext = ext.Substring(1);
            }

            if (mimeTypes.ContainsKey(ext))
            {
                return mimeTypes[ext];
            }

            return "unknown";
        }
    }
}