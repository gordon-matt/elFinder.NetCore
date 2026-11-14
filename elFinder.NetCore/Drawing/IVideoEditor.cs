namespace elFinder.NetCore.Drawing;

/// <summary>
/// Captures a thumbnail from a video file.
/// </summary>
public interface IVideoEditor
{
    /// <summary>
    /// Determines whether this editor can read the given file extension.
    /// </summary>
    bool CanProcessFile(string fileExtension);

    /// <summary>
    /// Captures one frame and scales it to <paramref name="size"/>. Returns null when ffmpeg
    /// cannot read the file. The caller caches the result beside image thumbnails.
    /// </summary>
    Task<ImageWithMimeType> GenerateThumbnailAsync(
        string videoPath,
        int size,
        bool keepAspectRatio,
        CancellationToken cancellationToken = default);
}
