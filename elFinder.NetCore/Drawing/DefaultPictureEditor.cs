using ImageMagick;
using System.Drawing;

namespace elFinder.NetCore.Drawing;

/// <summary>
/// Represents the default picture editor.
/// </summary>
public class DefaultPictureEditor : IPictureEditor
{
    public DefaultPictureEditor(Color backgroundColor) => BackgroundColor = backgroundColor;

    public DefaultPictureEditor() : this(Color.Transparent)
    {
    }

    public Color BackgroundColor { get; set; }

    public bool CanProcessFile(string fileExtension)
    {
        string ext = fileExtension.ToLowerInvariant();
        return ext is ".png" or ".jpg" or ".jpeg" or ".gif" or ".tiff";
    }

    public string ConvertThumbnailExtension(string originalImageExtension)
    {
        string ext = originalImageExtension.ToLowerInvariant();
        return ext == ".tiff"
            ? ".png"
            : ext is ".png" or ".jpg" or ".jpeg" or ".gif"
                ? ext
                : throw new ArgumentException($"{typeof(DefaultPictureEditor).FullName} does not support thumbnails for '{originalImageExtension}' files.");
    }

    public ImageWithMimeType Crop(Stream input, int x, int y, int width, int height)
    {
        using var image = new MagickImage(input);
        image.Crop(new MagickGeometry(x, y, (uint)width, (uint)height));
        image.ResetPage();
        return SaveImage(image);
    }

    public ImageWithMimeType GenerateThumbnail(Stream input, int size, bool keepAspectRatio)
    {
        using var image = new MagickImage(input);
        image.Resize(new MagickGeometry((uint)size, (uint)size) { IgnoreAspectRatio = !keepAspectRatio });
        return SaveImage(image);
    }

    public Size ImageSize(Stream input)
    {
        using var image = new MagickImage(input);
        return new Size((int)image.Width, (int)image.Height);
    }

    public ImageWithMimeType Resize(Stream input, int width, int height)
    {
        using var image = new MagickImage(input);
        image.Resize(new MagickGeometry((uint)width, (uint)height) { IgnoreAspectRatio = true });
        return SaveImage(image);
    }

    public ImageWithMimeType Rotate(Stream input, int degrees)
    {
        using var image = new MagickImage(input)
        {
            BackgroundColor = new MagickColor(BackgroundColor.R, BackgroundColor.G, BackgroundColor.B, BackgroundColor.A)
        };
        image.Rotate(degrees);
        image.ResetPage();
        return SaveImage(image);
    }

    private static ImageWithMimeType SaveImage(MagickImage image)
    {
        MagickFormat outputFormat = image.Format switch
        {
            MagickFormat.Jpeg or MagickFormat.Jpg => MagickFormat.Jpeg,
            MagickFormat.Gif => MagickFormat.Gif,
            _ => MagickFormat.Png
        };

        var memoryStream = new MemoryStream();
        image.Write(memoryStream, outputFormat);
        memoryStream.Position = 0;

        string mimeType = outputFormat switch
        {
            MagickFormat.Jpeg => "image/jpeg",
            MagickFormat.Gif => "image/gif",
            _ => "image/png"
        };
        return new ImageWithMimeType(mimeType, memoryStream);
    }
}
