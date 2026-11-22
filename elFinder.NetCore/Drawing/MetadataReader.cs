using System.Drawing;
using MetadataExtractor;
using MetadataExtractor.Formats.Avi;
using MetadataExtractor.Formats.Bmp;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Gif;
using MetadataExtractor.Formats.Heif;
using MetadataExtractor.Formats.Ico;
using MetadataExtractor.Formats.Jpeg;
using MetadataExtractor.Formats.Netpbm;
using MetadataExtractor.Formats.Photoshop;
using MetadataExtractor.Formats.Png;
using MetadataExtractor.Formats.QuickTime;
using MetadataExtractor.Formats.WebP;
using MetadataDirectory = MetadataExtractor.Directory;

namespace elFinder.NetCore.Drawing;

/// <summary>
/// Reads width and height with MetadataExtractor. Images fall back to ImageMagick, and videos to ffprobe.
/// </summary>
internal static class MetadataReader
{
    public static bool TryReadSize(Stream stream, out Size size)
    {
        size = default;
        if (stream == null || !stream.CanSeek)
        {
            return false;
        }

        long start = stream.Position;
        try
        {
            var directories = ImageMetadataReader.ReadMetadata(stream);
            if (TryGetSize(directories, out size))
            {
                return true;
            }
        }
        catch (Exception)
        {
            size = default;
        }

        try
        {
            stream.Position = start;
        }
        catch (Exception)
        {
            // The next reader rewinds a seekable stream. A failed rewind still has to report failure.
        }

        return false;
    }

    private static bool TryGetSize(IEnumerable<MetadataDirectory> directories, out Size size)
    {
        // Video containers can also carry a still image. The track size is the one the preview should use.
        if (TryGetSizeFrom<QuickTimeTrackHeaderDirectory>(directories, QuickTimeTrackHeaderDirectory.TagWidth, QuickTimeTrackHeaderDirectory.TagHeight, out size))
        {
            return true;
        }

        if (TryGetSizeFrom<AviDirectory>(directories, AviDirectory.TagWidth, AviDirectory.TagHeight, out size))
        {
            return true;
        }

        if (TryGetSizeFrom<JpegDirectory>(directories, JpegDirectory.TagImageWidth, JpegDirectory.TagImageHeight, out size))
        {
            return true;
        }

        if (TryGetSizeFrom<PngDirectory>(directories, PngDirectory.TagImageWidth, PngDirectory.TagImageHeight, out size))
        {
            return true;
        }

        if (TryGetSizeFrom<GifHeaderDirectory>(directories, GifHeaderDirectory.TagImageWidth, GifHeaderDirectory.TagImageHeight, out size))
        {
            return true;
        }

        if (TryGetSizeFrom<BmpHeaderDirectory>(directories, BmpHeaderDirectory.TagImageWidth, BmpHeaderDirectory.TagImageHeight, out size))
        {
            return true;
        }

        if (TryGetSizeFrom<WebPDirectory>(directories, WebPDirectory.TagImageWidth, WebPDirectory.TagImageHeight, out size))
        {
            return true;
        }

        if (TryGetSizeFrom<HeicImagePropertiesDirectory>(directories, HeicImagePropertiesDirectory.TagImageWidth, HeicImagePropertiesDirectory.TagImageHeight, out size))
        {
            return true;
        }

        if (TryGetSizeFrom<IcoDirectory>(directories, IcoDirectory.TagImageWidth, IcoDirectory.TagImageHeight, out size))
        {
            return true;
        }

        if (TryGetSizeFrom<PsdHeaderDirectory>(directories, PsdHeaderDirectory.TagImageWidth, PsdHeaderDirectory.TagImageHeight, out size))
        {
            return true;
        }

        if (TryGetSizeFrom<NetpbmHeaderDirectory>(directories, NetpbmHeaderDirectory.TagWidth, NetpbmHeaderDirectory.TagHeight, out size))
        {
            return true;
        }

        // TIFF, and camera raw formats stored as TIFF, keep the pixel size in IFD0.
        if (TryGetSizeFrom<ExifIfd0Directory>(directories, ExifDirectoryBase.TagImageWidth, ExifDirectoryBase.TagImageHeight, out size))
        {
            return true;
        }

        return TryGetSizeFrom<ExifSubIfdDirectory>(directories, ExifDirectoryBase.TagExifImageWidth, ExifDirectoryBase.TagExifImageHeight, out size);
    }

    private static bool TryGetSizeFrom(MetadataDirectory directory, int widthTag, int heightTag, out Size size)
    {
        if (directory.TryGetInt32(widthTag, out int width)
            && directory.TryGetInt32(heightTag, out int height)
            && width > 0
            && height > 0)
        {
            size = new Size(width, height);
            return true;
        }

        size = default;
        return false;
    }

    private static bool TryGetSizeFrom<TDirectory>(IEnumerable<MetadataDirectory> directories, int widthTag, int heightTag, out Size size)
        where TDirectory : MetadataDirectory
    {
        foreach (var directory in directories.OfType<TDirectory>())
        {
            if (TryGetSizeFrom(directory, widthTag, heightTag, out size))
            {
                return true;
            }
        }

        size = default;
        return false;
    }
}