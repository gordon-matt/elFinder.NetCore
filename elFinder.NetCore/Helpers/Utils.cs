using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace elFinder.NetCore.Helpers
{
    public static class Utils
    {
        private static Encoder utf8Encoder = Encoding.UTF8.GetEncoder();
        private static MD5 md5 = MD5.Create();

        public static string DecodePath(string path)
        {
            return Encoding.UTF8.GetString(HttpEncoder.UrlTokenDecode(path));
        }

        public static string EncodePath(string path)
        {
            return HttpEncoder.UrlTokenEncode(Encoding.UTF8.GetBytes(path));
        }

        public static string GetDuplicatedName(FileInfo file)
        {
            string parentPath = file.DirectoryName;
            string name = Path.GetFileNameWithoutExtension(file.Name);
            string extension = file.Extension;

            string newName = $"{parentPath}/{name} copy{extension}";
            if (!File.Exists(newName))
            {
                return newName;
            }
            else
            {
                bool found = false;
                for (int i = 1; i < 10 && !found; i++)
                {
                    newName = $"{parentPath}/{name} copy {i}{extension}";
                    if (!File.Exists(newName))
                    {
                        found = true;
                    }
                }
                if (!found)
                {
                    newName = $"{parentPath}/{name} copy {Guid.NewGuid()}{extension}";
                }
            }

            return newName;
        }

        public static string GetFileMd5(FileInfo info)
        {
            return GetFileMd5(info.Name, info.LastWriteTimeUtc);
        }

        public static string GetFileMd5(string fileName, DateTime modified)
        {
            fileName += modified.ToFileTimeUtc();
            char[] fileNameChars = fileName.ToCharArray();
            byte[] buffer = new byte[utf8Encoder.GetByteCount(fileNameChars, 0, fileName.Length, true)];
            utf8Encoder.GetBytes(fileNameChars, 0, fileName.Length, buffer, 0, true);
            return BitConverter.ToString(md5.ComputeHash(buffer)).Replace("-", string.Empty);
        }

        public static string GetMimeType(FileInfo file)
        {
            if (file.Extension.Length > 1)
            {
                return Mime.GetMimeType(file.Extension.ToLower().Substring(1));
            }
            else
            {
                return "unknown";
            }
        }

        public static string GetMimeType(string ext)
        {
            if (ext.StartsWith("."))
            {
                return Mime.GetMimeType(ext.Substring(1));
            }

            return Mime.GetMimeType(ext);
        }
    }
}