using System.Drawing;
using MetadataExtractor;
using MetadataExtractor.Formats.Bmp;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Gif;
using MetadataExtractor.Formats.Heif;
using MetadataExtractor.Formats.Ico;
using MetadataExtractor.Formats.Jpeg;
using MetadataExtractor.Formats.Netpbm;
using MetadataExtractor.Formats.Photoshop;
using MetadataExtractor.Formats.Png;
using MetadataExtractor.Formats.WebP;
using MetadataDirectory = MetadataExtractor.Directory;

namespace elFinder.NetCore.Drawing;

/// <summary>
/// Reads width and height with MetadataExtractor. ImageMagick is the fallback.
/// </summary>
internal static class ImageMetadataSize
{
    public static bool TryRead(Stream stream, out Size size)
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
        if (TryFrom<JpegDirectory>(directories, JpegDirectory.TagImageWidth, JpegDirectory.TagImageHeight, out size))
        {
            return true;
        }

        if (TryFrom<PngDirectory>(directories, PngDirectory.TagImageWidth, PngDirectory.TagImageHeight, out size))
        {
            return true;
        }

        if (TryFrom<GifHeaderDirectory>(directories, GifHeaderDirectory.TagImageWidth, GifHeaderDirectory.TagImageHeight, out size))
        {
            return true;
        }

        if (TryFrom<BmpHeaderDirectory>(directories, BmpHeaderDirectory.TagImageWidth, BmpHeaderDirectory.TagImageHeight, out size))
        {
            return true;
        }

        if (TryFrom<WebPDirectory>(directories, WebPDirectory.TagImageWidth, WebPDirectory.TagImageHeight, out size))
        {
            return true;
        }

        if (TryFrom<HeicImagePropertiesDirectory>(directories, HeicImagePropertiesDirectory.TagImageWidth, HeicImagePropertiesDirectory.TagImageHeight, out size))
        {
            return true;
        }

        if (TryFrom<IcoDirectory>(directories, IcoDirectory.TagImageWidth, IcoDirectory.TagImageHeight, out size))
        {
            return true;
        }

        if (TryFrom<PsdHeaderDirectory>(directories, PsdHeaderDirectory.TagImageWidth, PsdHeaderDirectory.TagImageHeight, out size))
        {
            return true;
        }

        if (TryFrom<NetpbmHeaderDirectory>(directories, NetpbmHeaderDirectory.TagWidth, NetpbmHeaderDirectory.TagHeight, out size))
        {
            return true;
        }

        // TIFF, and camera raw formats stored as TIFF, keep the pixel size in IFD0.
        if (TryFrom<ExifIfd0Directory>(directories, ExifDirectoryBase.TagImageWidth, ExifDirectoryBase.TagImageHeight, out size))
        {
            return true;
        }

        return TryFrom<ExifSubIfdDirectory>(directories, ExifDirectoryBase.TagExifImageWidth, ExifDirectoryBase.TagExifImageHeight, out size);
    }

    private static bool TryFrom<TDirectory>(IEnumerable<MetadataDirectory> directories, int widthTag, int heightTag, out Size size)
        where TDirectory : MetadataDirectory
    {
        foreach (var directory in directories.OfType<TDirectory>())
        {
            if (directory.TryGetInt32(widthTag, out int width)
                && directory.TryGetInt32(heightTag, out int height)
                && width > 0
                && height > 0)
            {
                size = new Size(width, height);
                return true;
            }
        }

        size = default;
        return false;
    }
}