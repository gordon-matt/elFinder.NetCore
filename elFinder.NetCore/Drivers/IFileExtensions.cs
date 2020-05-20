using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace elFinder.NetCore.Drivers
{
    public static class IFileExtensions
    {
        //private static readonly Encoder utf8Encoder = Encoding.UTF8.GetEncoder();
        private static readonly MD5 md5 = MD5.Create();

        public static async Task<string> GetFileMd5Async(this IFile file)
        {
            string fileName = file.Name;
            DateTime modified = await file.LastWriteTimeUtcAsync;

            fileName += modified.ToFileTimeUtc();
            var bytes = Encoding.UTF8.GetBytes(fileName);
            return BitConverter.ToString(md5.ComputeHash(bytes)).Replace("-", string.Empty);

            // OLD
            //char[] fileNameChars = fileName.ToCharArray();
            //byte[] buffer = new byte[utf8Encoder.GetByteCount(fileNameChars, 0, fileName.Length, true)];
            //utf8Encoder.GetBytes(fileNameChars, 0, fileName.Length, buffer, 0, true);

            //return BitConverter.ToString(md5.ComputeHash(buffer)).Replace("-", string.Empty);
        }
    }
}