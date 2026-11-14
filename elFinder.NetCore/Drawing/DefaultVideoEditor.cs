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
            bool captured = await TryCaptureAsync(videoPath, tempFile, at, cancellationToken)
                || (at > TimeSpan.Zero && await TryCaptureAsync(videoPath, tempFile, TimeSpan.Zero, cancellationToken));
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
        CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(CaptureTimeout);
        try
        {
            var conversion = await FFmpeg.Conversions.FromSnippet.Snapshot(videoPath, outputPath, at);
            conversion.SetOverwriteOutput(true);
            // A crafted file can otherwise make ffmpeg open http, concat, or subfile targets.
            conversion.AddParameter("-nostdin -protocol_whitelist file", ParameterPosition.PreInput);
            await conversion.Start(timeout.Token);
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
}