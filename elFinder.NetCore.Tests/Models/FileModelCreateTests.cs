using elFinder.NetCore.Drivers.FileSystem;
using elFinder.NetCore.Models;
using ImageMagick;
using Xunit;

namespace elFinder.NetCore.Tests.Models;

public class FileModelCreateTests
{
    [Fact]
    public async Task CreateAsync_ImageListing_IncludesHeaderDimensions()
    {
        using var root = new TempRoot();
        string filePath = Path.Combine(root.Path, "photo.png");
        WritePng(filePath, 40, 20);

        var volume = new RootVolume(root.Path, "https://example.test/files/", "https://example.test/thumb/");
        BaseModel model = await BaseModel.CreateAsync(new FileSystemFile(filePath), volume);

        ImageModel image = Assert.IsType<ImageModel>(model);
        Assert.Equal("40x20", image.Dimension);
        Assert.False(string.IsNullOrEmpty(image.Thumbnail as string));
        Assert.Equal("photo.png", image.Name);
    }

    [Fact]
    public async Task CreateAsync_UnreadableImage_IsListedWithoutFailing()
    {
        using var root = new TempRoot();
        string filePath = Path.Combine(root.Path, "photo.png");
        await File.WriteAllBytesAsync(filePath, [1, 2, 3, 4], TestContext.Current.CancellationToken);

        var volume = new RootVolume(root.Path, "https://example.test/files/", "https://example.test/thumb/");
        BaseModel model = await BaseModel.CreateAsync(new FileSystemFile(filePath), volume);

        Assert.IsNotType<ImageModel>(model);
        Assert.Equal("photo.png", model.Name);
    }

    [Fact]
    public async Task CreateAsync_PlainFile_HasNoThumbnail()
    {
        using var root = new TempRoot();
        string filePath = Path.Combine(root.Path, "notes.txt");
        await File.WriteAllTextAsync(filePath, "hello", TestContext.Current.CancellationToken);

        var volume = new RootVolume(root.Path, "https://example.test/files/", "https://example.test/thumb/");
        BaseModel model = await BaseModel.CreateAsync(new FileSystemFile(filePath), volume);

        Assert.IsNotType<ImageModel>(model);
        Assert.Equal("notes.txt", model.Name);
    }

    private static void WritePng(string filePath, uint width, uint height)
    {
        using var image = new MagickImage(MagickColors.CornflowerBlue, width, height);
        image.Write(filePath, MagickFormat.Png);
    }

    private sealed class TempRoot : IDisposable
    {
        public TempRoot()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "elfinder-list-" + Guid.NewGuid().ToString("N"));
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