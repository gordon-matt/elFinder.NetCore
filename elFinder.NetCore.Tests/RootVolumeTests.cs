using elFinder.NetCore.Drawing;
using elFinder.NetCore.Drivers.FileSystem;
using ImageMagick;
using Xunit;

namespace elFinder.NetCore.Tests;

public class RootVolumeTests
{
    [Fact]
    public void Constructor_ThumbnailDirectoryOmitted_UsesTmbUnderRoot()
    {
        using var root = new TempRoot();

        var volume = new RootVolume(root.Path, "https://example.test/files/", "https://example.test/thumb/");

        Assert.Equal($"{root.Path}{Path.DirectorySeparatorChar}.tmb", volume.ThumbnailDirectory);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_MissingThumbnailDirectory_UsesTmbUnderRoot(string? thumbnailDirectory)
    {
        using var root = new TempRoot();

        var volume = new RootVolume(
            root.Path,
            "https://example.test/files/",
            "https://example.test/thumb/",
            thumbnailDirectory);

        Assert.Equal($"{root.Path}{Path.DirectorySeparatorChar}.tmb", volume.ThumbnailDirectory);
    }

    [Fact]
    public void Constructor_DirectorySeparatorChar_AppliesToDefaultThumbnailDirectory()
    {
        using var root = new TempRoot();

        var volume = new RootVolume(root.Path, "https://example.test/files/", "https://example.test/thumb/", '/');

        Assert.Equal('/', volume.DirectorySeparatorChar);
        Assert.Equal($"{root.Path}/.tmb", volume.ThumbnailDirectory);
    }

    [Fact]
    public void Constructor_CustomThumbnailDirectory_UsesThatDirectory()
    {
        using var root = new TempRoot();
        using var thumbs = new TempRoot();

        var volume = new RootVolume(
            root.Path,
            "https://example.test/files/",
            "https://example.test/thumb/",
            thumbs.Path);

        Assert.Equal(thumbs.Path, volume.ThumbnailDirectory);
    }

    [Fact]
    public void Constructor_CustomThumbnailDirectoryWithTrailingSeparator_RemovesTrailingSeparator()
    {
        using var root = new TempRoot();
        using var thumbs = new TempRoot();

        var volume = new RootVolume(
            root.Path,
            "https://example.test/files/",
            "https://example.test/thumb/",
            thumbs.Path + Path.DirectorySeparatorChar);

        Assert.Equal(thumbs.Path, volume.ThumbnailDirectory);
    }

    [Fact]
    public void Constructor_DriveRootThumbnailDirectory_KeepsTrailingSeparator()
    {
        using var root = new TempRoot();

        var volume = new RootVolume(root.Path, "https://example.test/files/", "https://example.test/thumb/", @"C:\");

        Assert.Equal(@"C:\", volume.ThumbnailDirectory);
    }

    [Fact]
    public async Task GenerateThumbPathAsync_CustomThumbnailDirectory_PlacesThumbOutsideFilesRoot()
    {
        using var root = new TempRoot();
        using var thumbs = new TempRoot();
        string subDirectory = Path.Combine(root.Path, "album");
        Directory.CreateDirectory(subDirectory);
        string filePath = Path.Combine(subDirectory, "photo.png");
        await File.WriteAllBytesAsync(filePath, [1, 2, 3, 4], TestContext.Current.CancellationToken);

        var volume = new RootVolume(
            root.Path,
            "https://example.test/files/",
            "https://example.test/thumb/",
            thumbs.Path);
        string thumbPath = await volume.GenerateThumbPathAsync(new FileSystemFile(filePath));

        Assert.StartsWith(thumbs.Path + Path.DirectorySeparatorChar, thumbPath);
        Assert.Contains($"{Path.DirectorySeparatorChar}album{Path.DirectorySeparatorChar}photo_", thumbPath);
        Assert.EndsWith(".png", thumbPath);
        Assert.DoesNotContain($"{Path.DirectorySeparatorChar}.tmb{Path.DirectorySeparatorChar}", thumbPath);
    }

    [Fact]
    public async Task GenerateThumbnailAsync_NestedFile_HidesTmbDirectoryAndSubdirectory()
    {
        using var root = new TempRoot();
        string album = Path.Combine(root.Path, "album");
        Directory.CreateDirectory(album);
        WritePng(Path.Combine(album, "photo.png"));

        var volume = new RootVolume(root.Path, "https://example.test/files/", "https://example.test/thumb/");
        using var thumb = await GenerateThumbnailAsync(volume, root.Path, Path.Combine("album", "photo_abc.png"));

        Assert.True(IsHidden(Path.Combine(root.Path, ".tmb")));
        Assert.True(IsHidden(Path.Combine(root.Path, ".tmb", "album")));
        Assert.False(IsHidden(root.Path));
    }

    [Fact]
    public async Task GenerateThumbnailAsync_ExistingTmbParent_HidesTmbDirectory()
    {
        using var root = new TempRoot();
        using var parent = new TempRoot();
        string thumbnailDirectory = Path.Combine(parent.Path, ".tmb", "Files");
        Directory.CreateDirectory(thumbnailDirectory);
        WritePng(Path.Combine(root.Path, "photo.png"));

        var volume = new RootVolume(
            root.Path,
            "https://example.test/files/",
            "https://example.test/thumb/",
            thumbnailDirectory);
        using var thumb = await GenerateThumbnailAsync(volume, root.Path, "photo_abc.png");

        Assert.True(IsHidden(Path.Combine(parent.Path, ".tmb")));
        Assert.True(IsHidden(thumbnailDirectory));
        Assert.False(IsHidden(parent.Path));
    }

    private static async Task<ImageWithMimeType> GenerateThumbnailAsync(RootVolume volume, string root, string relativeThumbPath)
    {
        string thumbTarget = Path.Combine(root, relativeThumbPath);
        var path = new FullPath(volume, new FileSystemFile(thumbTarget), "hash");
        return await path.GenerateThumbnailAsync(TestContext.Current.CancellationToken);
    }

    private static bool IsHidden(string path)
    {
        var attributes = new DirectoryInfo(path).Attributes;
        return (attributes & FileAttributes.Hidden) != 0;
    }

    private static void WritePng(string filePath)
    {
        using var image = new MagickImage(MagickColors.CornflowerBlue, 8, 8);
        image.Write(filePath, MagickFormat.Png);
    }

    private sealed class TempRoot : IDisposable
    {
        public TempRoot()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "elfinder-volume-" + Guid.NewGuid().ToString("N"));
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