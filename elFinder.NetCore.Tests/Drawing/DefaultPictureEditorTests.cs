using elFinder.NetCore.Drawing;
using ImageMagick;
using System.Drawing;
using Xunit;

namespace elFinder.NetCore.Tests.Drawing;

public class DefaultPictureEditorTests
{
    private readonly DefaultPictureEditor _editor = new();

    [Fact]
    public void ImageSize_ReturnsSourceDimensions()
    {
        using var input = CreateImage(120, 80);
        Assert.Equal(new Size(120, 80), _editor.ImageSize(input));
    }

    [Fact]
    public void GenerateThumbnail_PreservesAspectRatio()
    {
        using var input = CreateImage(120, 80);
        using var result = _editor.GenerateThumbnail(input, 60, true);
        Assert.Equal("image/png", result.MimeType);
        Assert.Equal(new Size(60, 40), ReadSize(result.ImageStream));
    }

    [Fact]
    public void GenerateThumbnail_CanCreateSquareImage()
    {
        using var input = CreateImage(120, 80);
        using var result = _editor.GenerateThumbnail(input, 60, false);
        Assert.Equal(new Size(60, 60), ReadSize(result.ImageStream));
    }

    [Fact]
    public void Resize_UsesRequestedDimensions()
    {
        using var input = CreateImage(120, 80);
        using var result = _editor.Resize(input, 32, 24);
        Assert.Equal(new Size(32, 24), ReadSize(result.ImageStream));
    }

    [Fact]
    public void Crop_UsesRequestedRegionDimensions()
    {
        using var input = CreateImage(120, 80);
        using var result = _editor.Crop(input, 10, 10, 30, 20);
        Assert.Equal(new Size(30, 20), ReadSize(result.ImageStream));
    }

    [Fact]
    public void Rotate_ExpandsCanvasForRightAngle()
    {
        using var input = CreateImage(120, 80);
        using var result = _editor.Rotate(input, 90);
        Assert.Equal(new Size(80, 120), ReadSize(result.ImageStream));
    }

    [Theory]
    [InlineData(MagickFormat.Jpeg, "image/jpeg")]
    [InlineData(MagickFormat.Gif, "image/gif")]
    [InlineData(MagickFormat.Tiff, "image/png")]
    public void Resize_UsesBrowserCompatibleOutputFormat(MagickFormat inputFormat, string expectedMimeType)
    {
        using var input = CreateImage(120, 80, inputFormat);
        using var result = _editor.Resize(input, 30, 20);

        Assert.Equal(expectedMimeType, result.MimeType);
        Assert.Equal(new Size(30, 20), ReadSize(result.ImageStream));
    }

    [Theory]
    [InlineData(".png")]
    [InlineData(".jpg")]
    [InlineData(".jpeg")]
    [InlineData(".gif")]
    [InlineData(".tiff")]
    public void CanProcessFile_AcceptsSupportedFormats(string extension)
    {
        Assert.True(_editor.CanProcessFile(extension));
    }

    private static MemoryStream CreateImage(uint width, uint height, MagickFormat format = MagickFormat.Png)
    {
        using var image = new MagickImage(MagickColors.CornflowerBlue, width, height);
        var stream = new MemoryStream();
        image.Write(stream, format);
        stream.Position = 0;
        return stream;
    }

    private static Size ReadSize(Stream stream)
    {
        stream.Position = 0;
        using var image = new MagickImage(stream);
        return new Size((int)image.Width, (int)image.Height);
    }
}
