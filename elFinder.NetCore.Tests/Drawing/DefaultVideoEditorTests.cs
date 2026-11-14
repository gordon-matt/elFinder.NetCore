using elFinder.NetCore;
using elFinder.NetCore.Drawing;
using elFinder.NetCore.Drivers.FileSystem;
using Xunit;

namespace elFinder.NetCore.Tests.Drawing;

public class DefaultVideoEditorTests
{
    private readonly DefaultVideoEditor editor = new(new DefaultPictureEditor());

    [Theory]
    [InlineData(".mp4", true)]
    [InlineData(".MKV", true)]
    [InlineData(".webm", true)]
    [InlineData(".wmv", true)]
    [InlineData(".png", false)]
    [InlineData(".txt", false)]
    public void CanProcessFile_MatchesVideoExtensions(string extension, bool expected) =>
        Assert.Equal(expected, editor.CanProcessFile(extension));

    [Fact]
    public void CanCreateThumbnail_ImageDoesNotNeedFfmpeg()
    {
        using var root = new TempRoot();
        var volume = new RootVolume(root.Path, "https://example.test/files/", "https://example.test/thumb/");
        var file = new FileSystemFile(Path.Combine(root.Path, "photo.png"));

        Assert.True(volume.CanCreateThumbnail(file));
    }

    [Fact]
    public void CanCreateThumbnail_VideoStaysOffUntilFfmpegIsReady()
    {
        using var root = new TempRoot();
        var volume = new RootVolume(root.Path, "https://example.test/files/", "https://example.test/thumb/");
        var file = new FileSystemFile(Path.Combine(root.Path, "clip.mp4"));

        Assert.False(volume.CanCreateThumbnail(file));
    }

    [Fact]
    public void CanCreateThumbnail_RequiresThumbnailUrl()
    {
        using var root = new TempRoot();
        var volume = new RootVolume(root.Path, "https://example.test/files/");
        var file = new FileSystemFile(Path.Combine(root.Path, "photo.png"));

        Assert.False(volume.CanCreateThumbnail(file));
    }

    private sealed class TempRoot : IDisposable
    {
        public TempRoot()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "elfinder-root-" + Guid.NewGuid().ToString("N"));
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
