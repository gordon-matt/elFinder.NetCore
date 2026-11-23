using System.Drawing;
using System.Globalization;
using elFinder.NetCore.Ffmpeg;
using Xabe.FFmpeg;

namespace elFinder.NetCore.Drawing;

/// <summary>
/// Grabs one frame with ffmpeg and scales it through <see cref="IPictureEditor"/>.
/// </summary>
public class DefaultVideoEditor : IVideoEditor
{
    private const int MaxConcurrentCaptures = 2;
    private static readonly TimeSpan CaptureTimeout = TimeSpan.FromSeconds(15);
    private static readonly SemaphoreSlim Gate = new(MaxConcurrentCaptures, MaxConcurrentCaptures);

    private readonly IPictureEditor pictureEditor;

    public DefaultVideoEditor(IPictureEditor editor)
    {
        ArgumentNullException.ThrowIfNull(editor);
        pictureEditor = editor;
    }

    public bool CanProcessFile(string fileExtension)
    {
        string ext = fileExtension.ToLowerInvariant();
        return ext is ".mp4" or ".mpeg" or ".mpg" or ".mkv" or ".m4v" or ".avi" or ".mov" or ".webm" or ".wmv";
    }

    public async Task<ImageWithMimeType> GenerateThumbnailAsync(
        string videoPath,
        int size,
        bool keepAspectRatio,
        CancellationToken cancellationToken = default)
    {
        if (!FfmpegBootstrap.IsAvailable || string.IsNullOrEmpty(videoPath) || !File.Exists(videoPath))
        {
            return null;
        }

        string tempFile = Path.Combine(Path.GetTempPath(), $"elfinder-{Guid.NewGuid():N}.jpg");
        await Gate.WaitAsync(cancellationToken);
        try
        {
            var at = await GetMidpointAsync(videoPath, cancellationToken);
            bool captured = await TryCaptureAsync(videoPath, tempFile, at, size, keepAspectRatio, cancellationToken)
                || (at > TimeSpan.Zero && await TryCaptureAsync(videoPath, tempFile, TimeSpan.Zero, size, keepAspectRatio, cancellationToken));
            if (!captured)
            {
                return null;
            }

            using var frame = File.OpenRead(tempFile);
            return pictureEditor.GenerateThumbnail(frame, size, keepAspectRatio);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return null;
        }
        finally
        {
            Gate.Release();
            TryDelete(tempFile);
        }
    }

    private static async Task<TimeSpan> GetMidpointAsync(string videoPath, CancellationToken cancellationToken)
    {
        var media = await FFmpeg.GetMediaInfo(videoPath, cancellationToken);
        var duration = media.Duration;
        if (duration <= TimeSpan.Zero)
        {
            duration = media.VideoStreams.FirstOrDefault()?.Duration ?? TimeSpan.Zero;
        }

        return duration > TimeSpan.Zero
            ? TimeSpan.FromTicks(duration.Ticks / 2)
            : TimeSpan.Zero;
    }

    private static async Task<bool> TryCaptureAsync(
        string videoPath,
        string outputPath,
        TimeSpan at,
        int size,
        bool keepAspectRatio,
        CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(CaptureTimeout);
        try
        {
            // -ss before -i stops at the nearest keyframe, so ffmpeg does not decode up to the exact time.
            // The scale filter writes a thumbnail-sized frame. Snapshot() would probe the file again and
            // leave a full-size image for Magick to shrink.
            string seek = at.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture);
            string scale = keepAspectRatio
                ? $"scale={size}:{size}:force_original_aspect_ratio=decrease"
                : $"scale={size}:{size}";
            string arguments = string.Join(' ',
                "-nostdin",
                "-protocol_whitelist", "file",
                "-ss", seek,
                "-i", Quote(videoPath),
                "-an",
                "-frames:v", "1",
                "-vf", scale,
                "-q:v", "5",
                "-y",
                Quote(outputPath));

            await FFmpeg.Conversions.New().Start(arguments, timeout.Token);
            return File.Exists(outputPath) && new FileInfo(outputPath).Length > 0;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return false;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return false;
        }
    }

    private static string Quote(string path) => "\"" + path.Replace("\"", "\\\"") + "\"";

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (IOException)
        {
        }
    }

    public async Task<Size?> GetVideoSize(string videoPath)
    {
        try
        {
            using var stream = File.OpenRead(videoPath);
            if (MetadataReader.TryReadSize(stream, out Size headerSize))
            {
                return headerSize;
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return null;
        }

        try
        {
            var mediaInfo = await FFmpeg.GetMediaInfo(videoPath);
            var videoStream = mediaInfo.VideoStreams.FirstOrDefault();

            return videoStream is not null
                ? videoStream.Width <= 0 || videoStream.Height <= 0
                    ? null
                    : new()
                    {
                        Width = videoStream.Width,
                        Height = videoStream.Height
                    }
                : null;
        }
        catch
        {
            return null;
        }
    }
}