namespace elFinder.NetCore.Http
{
	using elFinder.NetCore.Drivers;
	using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    public static class Mime
    {
        private static Dictionary<string, string> mimeTypes;

        static Mime()
        {
            mimeTypes = new Dictionary<string, string>();
            var assembly = typeof(Mime).GetTypeInfo().Assembly;

            using (var stream = assembly.GetManifestResourceStream("elFinder.NetCore.MimeTypes.txt"))
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

		public static string GetMimeType(IFile file)
		{
			if (file.Extension.Length > 1)
			{
				return GetMimeType(file.Extension.ToLower());
			}
			else
			{
				return "unknown";
			}
		}

		public static string GetMimeType(string extension)
        {
			if (extension.StartsWith(".")) extension = extension.Substring(1);

            if (mimeTypes.ContainsKey(extension))
            {
                return mimeTypes[extension];
            }

            return "unknown";
        }
    }
}