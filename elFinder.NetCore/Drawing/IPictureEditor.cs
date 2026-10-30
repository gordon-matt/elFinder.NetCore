using System.Drawing;

namespace elFinder.NetCore.Drawing;

/// <summary>
/// Represents a picture editor.
/// </summary>
public interface IPictureEditor
{
    /// <summary>
    /// Gets or sets the background color used in rotation operations.
    /// </summary>
    Color BackgroundColor { get; set; }

    /// <summary>
    /// Determines whether this picture editor can process the given file extension.
    /// </summary>
    bool CanProcessFile(string fileExtension);

    /// <summary>
    /// Converts an image extension to a browser-compatible thumbnail extension.
    /// </summary>
    string ConvertThumbnailExtension(string originalImageExtension);

    /// <summary>
    /// Crops an image to the requested rectangle.
    /// </summary>
    ImageWithMimeType Crop(Stream input, int x, int y, int width, int height);

    /// <summary>
    /// Generates a thumbnail of the given image.
    /// </summary>
    ImageWithMimeType GenerateThumbnail(Stream input, int size, bool keepAspectRatio);

    /// <summary>
    /// Gets the width and height of an image.
    /// </summary>
    Size ImageSize(Stream input);

    /// <summary>
    /// Resizes an image to the requested dimensions.
    /// </summary>
    ImageWithMimeType Resize(Stream input, int width, int height);

    /// <summary>
    /// Rotates an image clockwise by the requested angle.
    /// </summary>
    ImageWithMimeType Rotate(Stream input, int degrees);
}
