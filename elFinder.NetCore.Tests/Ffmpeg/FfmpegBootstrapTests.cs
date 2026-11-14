using elFinder.NetCore.Ffmpeg;
using Xunit;

namespace elFinder.NetCore.Tests.Ffmpeg;

public class FfmpegBootstrapTests
{
    [Fact]
    public void Resolve_FindsConfiguredDirectoryWhenBothBinariesExist()
    {
        using var dir = new TempDirectory();
        WriteTools(dir.Path, includeProbe: true);

        FfmpegLocation found = FfmpegBootstrap.Resolve(Options(dir.Path));

        Assert.NotNull(found);
        Assert.Equal(Path.GetFullPath(dir.Path), found.Directory);
        Assert.True(File.Exists(found.FfmpegPath));
        Assert.True(File.Exists(found.FfprobePath));
    }

    [Fact]
    public void Resolve_RejectsDirectoryThatHasNoFfprobe()
    {
        using var dir = new TempDirectory();
        WriteTools(dir.Path, includeProbe: false);

        Assert.Null(FfmpegBootstrap.Resolve(Options(dir.Path)));
    }

    [Fact]
    public async Task EnsureAsync_ReturnsNullWhenFfmpegIsMissing()
    {
        FfmpegLocation found = await FfmpegBootstrap.EnsureAsync(new FfmpegBootstrapOptions
        {
            ConfiguredPath = Path.Combine(Path.GetTempPath(), "elfinder-missing-" + Guid.NewGuid().ToString("N")),
            SearchWellKnownLocations = false,
            SearchApplicationDirectories = false,
            SearchPath = false,
            PreferLinuxSystemDirectory = false,
            DownloadIfMissing = false
        }, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Null(found);
    }

    [Fact]
    public async Task EnsureAsync_ReturnsNullWhenTheDownloadDirectoryCannotBeCreated()
    {
        string file = Path.Combine(Path.GetTempPath(), "elfinder-not-a-dir-" + Guid.NewGuid().ToString("N"));
        await File.WriteAllTextAsync(file, "x", TestContext.Current.CancellationToken);
        try
        {
            FfmpegLocation found = await FfmpegBootstrap.EnsureAsync(new FfmpegBootstrapOptions
            {
                SearchWellKnownLocations = false,
                SearchApplicationDirectories = false,
                SearchPath = false,
                PreferLinuxSystemDirectory = false,
                DownloadIfMissing = true,
                DownloadDirectory = file
            }, cancellationToken: TestContext.Current.CancellationToken);

            Assert.Null(found);
        }
        finally
        {
            File.Delete(file);
        }
    }

    [Fact]
    public void Probe_AcceptsAnExecutablePath()
    {
        using var dir = new TempDirectory();
        WriteTools(dir.Path, includeProbe: true);
        string ffmpeg = Path.Combine(dir.Path, OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg");

        FfmpegLocation found = FfmpegBootstrap.Probe(ffmpeg, requireFfprobe: true);

        Assert.NotNull(found);
        Assert.Equal(Path.GetFullPath(dir.Path), found.Directory);
    }

    private static FfmpegBootstrapOptions Options(string path) => new()
    {
        ConfiguredPath = path,
        SearchWellKnownLocations = false,
        SearchApplicationDirectories = false,
        SearchPath = false,
        PreferLinuxSystemDirectory = false,
        RequireFfprobe = true
    };

    private static void WriteTools(string directory, bool includeProbe)
    {
        string ffmpegName = OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg";
        File.WriteAllText(Path.Combine(directory, ffmpegName), string.Empty);
        if (includeProbe)
        {
            string probeName = OperatingSystem.IsWindows() ? "ffprobe.exe" : "ffprobe";
            File.WriteAllText(Path.Combine(directory, probeName), string.Empty);
        }
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "elfinder-ffmpeg-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, true);
            }
        }
    }
}
