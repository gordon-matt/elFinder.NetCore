using Microsoft.Extensions.Logging;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace elFinder.NetCore.Ffmpeg;

/// <summary>
/// Where ffmpeg and ffprobe were found. Xabe runs only when both binaries are in one directory.
/// </summary>
public sealed record FfmpegLocation(string Directory, string FfmpegPath, string FfprobePath);

/// <summary>
/// How to find or download ffmpeg. The host fills in the directory it wants to use.
/// </summary>
public sealed class FfmpegBootstrapOptions
{
    /// <summary>A file path or a directory. A file path uses its parent directory.</summary>
    public string ConfiguredPath { get; init; }

    /// <summary>Where an official build is stored when <see cref="DownloadIfMissing"/> is set.</summary>
    public string DownloadDirectory { get; init; }

    /// <summary>Look in Program Files, the user profile, and the usual Linux bin directories.</summary>
    public bool SearchWellKnownLocations { get; init; } = true;

    /// <summary>Also look next to the app and in the process working directory.</summary>
    public bool SearchApplicationDirectories { get; init; }

    /// <summary>Also look through the process PATH.</summary>
    public bool SearchPath { get; init; } = true;

    /// <summary>On Linux, check <c>/usr/bin</c> before any other location.</summary>
    public bool PreferLinuxSystemDirectory { get; init; }

    /// <summary>
    /// Download the official build when nothing local is complete. That build is GPL-licensed,
    /// so the host opts in.
    /// </summary>
    public bool DownloadIfMissing { get; init; }

    /// <summary>Frame capture needs ffprobe beside ffmpeg.</summary>
    public bool RequireFfprobe { get; init; } = true;
}

/// <summary>
/// Finds ffmpeg and ffprobe, downloading an official build only when the host asked for that.
/// Call once at startup, before the app accepts requests.
/// </summary>
public static class FfmpegBootstrap
{
    private static volatile bool isAvailable;

    /// <summary>True after <see cref="EnsureAsync"/> has pointed Xabe at a usable directory.</summary>
    public static bool IsAvailable => isAvailable;

    public static async Task<FfmpegLocation> EnsureAsync(
        FfmpegBootstrapOptions options,
        ILogger logger = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        FfmpegLocation resolved = Resolve(options);
        if (resolved is not null)
        {
            Use(resolved);
            logger?.LogInformation("FFmpeg tools directory: {Path}", resolved.Directory);
            return resolved;
        }

        if (!options.DownloadIfMissing || string.IsNullOrWhiteSpace(options.DownloadDirectory))
        {
            logger?.LogInformation("FFmpeg was not found. Video thumbnails are disabled.");
            return null;
        }

        string downloadDir = Path.GetFullPath(options.DownloadDirectory);
        Directory.CreateDirectory(downloadDir);
        resolved = Probe(downloadDir, options.RequireFfprobe);
        if (resolved is not null)
        {
            Use(resolved);
            logger?.LogInformation("FFmpeg tools directory: {Path}", resolved.Directory);
            return resolved;
        }

        try
        {
            logger?.LogInformation("Downloading FFmpeg (official build) to {Path}", downloadDir);
            await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, downloadDir, progress: null)
                .WaitAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger?.LogError(ex, "Failed to download FFmpeg into {Path}", downloadDir);
            return null;
        }

        resolved = Probe(downloadDir, options.RequireFfprobe);
        if (resolved is null)
        {
            logger?.LogError("FFmpeg download finished but the binaries were not found under {Path}", downloadDir);
            return null;
        }

        Use(resolved);
        logger?.LogInformation("FFmpeg tools directory: {Path}", resolved.Directory);
        return resolved;
    }

    /// <summary>Looks on disk only. Does not download and does not change Xabe's path.</summary>
    public static FfmpegLocation Resolve(FfmpegBootstrapOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        foreach (string candidate in Candidates(options))
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            FfmpegLocation found = Probe(candidate, options.RequireFfprobe);
            if (found is not null)
            {
                return found;
            }
        }

        return null;
    }

    public static void Use(FfmpegLocation location)
    {
        ArgumentNullException.ThrowIfNull(location);
        FFmpeg.SetExecutablesPath(location.Directory);
        isAvailable = true;
    }

    public static FfmpegLocation Probe(string path, bool requireFfprobe)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        string full = Path.GetFullPath(path.Trim());
        string directory = File.Exists(full)
            ? Path.GetDirectoryName(full) ?? full
            : full;

        if (!Directory.Exists(directory))
        {
            return null;
        }

        bool windows = OperatingSystem.IsWindows();
        string ffmpeg = Path.Combine(directory, windows ? "ffmpeg.exe" : "ffmpeg");
        string ffprobe = Path.Combine(directory, windows ? "ffprobe.exe" : "ffprobe");
        bool hasFfmpeg = File.Exists(ffmpeg);
        bool hasFfprobe = File.Exists(ffprobe);

        if (!hasFfmpeg || (requireFfprobe && !hasFfprobe))
        {
            return null;
        }

        return new FfmpegLocation(directory, ffmpeg, hasFfprobe ? ffprobe : ffmpeg);
    }

    private static IEnumerable<string> Candidates(FfmpegBootstrapOptions options)
    {
        if (options.PreferLinuxSystemDirectory && OperatingSystem.IsLinux())
        {
            yield return "/usr/bin";
        }

        if (!string.IsNullOrWhiteSpace(options.ConfiguredPath))
        {
            yield return options.ConfiguredPath;
        }

        if (options.SearchWellKnownLocations)
        {
            if (OperatingSystem.IsWindows())
            {
                yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "ffmpeg", "bin");
                yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ffmpeg");
            }

            yield return "/usr/bin";
            yield return "/usr/local/bin";
        }

        if (options.SearchApplicationDirectories)
        {
            yield return AppContext.BaseDirectory;
            yield return Directory.GetCurrentDirectory();
        }

        if (!options.SearchPath)
        {
            yield break;
        }

        string pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathEnv))
        {
            yield break;
        }

        foreach (string entry in pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            yield return entry.Trim();
        }
    }
}
