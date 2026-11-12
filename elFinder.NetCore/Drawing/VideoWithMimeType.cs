using System.Drawing;

namespace elFinder.NetCore.Drawing;

public class VideoWithMimeType(string mimeType, Stream stream, Size size) : ImageWithMimeType(mimeType, stream)
{
    public Size Size { get; set; } = size;

}