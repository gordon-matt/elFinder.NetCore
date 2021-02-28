using System;
using System.IO;

namespace elFinder.NetCore.Models
{
    public class FileContent : IDisposable
    {
        public string FileName { get; set; }
        public long Length { get; set; }
        public Stream ContentStream { get; set; }
        public string ContentType { get; set; }

        #region IDisposable Support

        public void Dispose()
        {
            ContentStream.Dispose();
        }

        #endregion IDisposable Support
    }
}
