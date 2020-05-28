namespace elFinder.NetCore.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    public static class MimeHelper
    {
        private static readonly Dictionary<string, MimeType> mimeTypes;

        // https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types
        static MimeHelper()
        {
            mimeTypes = new Dictionary<string, MimeType>();
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
                    string[] mime = parts[0].Split('/');

                    for (int i = 1; i < parts.Length; i++)
                    {
                        string ext = parts[i].ToLower();
                        if (!mimeTypes.ContainsKey(ext))
                        {
                            mimeTypes.Add(ext, new MimeType { Type = mime[0], Subtype = mime[1] });
                        }
                    }
                }
            }
        }

        public static MimeType GetMimeType(string extension)
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

            // https://stackoverflow.com/questions/12539058/is-there-a-default-mime-type/12560996
            return new MimeType { Type = "application", Subtype = "octet-stream" };
        }
    }
}