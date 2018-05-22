using elFinder.NetCore.Drivers;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace elFinder.NetCore.Http
{
	public static class Cryptography
    {
		private static MD5 md5 = MD5.Create();

		public static async Task<string> GetFileMd5Async(IFile info)
		{
			return GetFileMd5(info.Name, await info.LastWriteTimeUtcAsync);
		}

		public static string GetFileMd5(string fileName, DateTime modified)
		{
			fileName += modified.ToFileTimeUtc();
			var bytes = Encoding.UTF8.GetBytes(fileName);
			return BitConverter.ToString(md5.ComputeHash(bytes)).Replace("-", string.Empty);
		}
	}
}
