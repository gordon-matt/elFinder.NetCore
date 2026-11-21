using System.Drawing;
using elFinder.NetCore.Drawing;
using ImageMagick;
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
    public void ImageSize_UsesMagickWhenHeaderReadThrows()
    {
        using var input = CreateImage(90, 40);
        using var stream = new ThrowOnceStream(input);
        Assert.Equal(new Size(90, 40), _editor.ImageSize(stream));
    }

    [Theory]
    [InlineData(MagickFormat.Jpeg, 30, 18)]
    [InlineData(MagickFormat.Png, 7, 13)]
    [InlineData(MagickFormat.Gif, 64, 40)]
    [InlineData(MagickFormat.Bmp, 21, 9)]
    [InlineData(MagickFormat.WebP, 48, 32)]
    [InlineData(MagickFormat.Tiff, 15, 11)]
    [InlineData(MagickFormat.Avif, 19, 17)]
    public void ImageSize_File_ReadsHeaderOrFallsBack(MagickFormat format, int width, int height)
    {
        string path = Path.Combine(Path.GetTempPath(), "elfinder-dim-" + Guid.NewGuid().ToString("N") + "." + format.ToString().ToLowerInvariant());
        try
        {
            WriteImage(path, (uint)width, (uint)height, format);
            Assert.Equal(new Size(width, height), _editor.ImageSize(path));
            Assert.Equal(new Size(width, height), _editor.ImageSize(path));

            WriteImage(path, (uint)(width + 4), (uint)(height + 2), format);
            Assert.Equal(new Size(width + 4, height + 2), _editor.ImageSize(path));
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
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
    [InlineData(MagickFormat.WebP, "image/webp")]
    [InlineData(MagickFormat.Bmp, "image/bmp")]
    [InlineData(MagickFormat.Avif, "image/avif")]
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
    [InlineData(".jpe")]
    [InlineData(".jfif")]
    [InlineData(".gif")]
    [InlineData(".tif")]
    [InlineData(".tiff")]
    [InlineData(".webp")]
    [InlineData(".bmp")]
    [InlineData(".avif")]
    public void CanProcessFile_AcceptsSupportedFormats(string extension) => Assert.True(_editor.CanProcessFile(extension));

    [Theory]
    [InlineData(".svg")]
    [InlineData(".heic")]
    [InlineData(".ico")]
    [InlineData(".jp2")]
    [InlineData(".jxl")]
    public void CanProcessFile_RejectsFormatsThatCannotRoundTrip(string extension) => Assert.False(_editor.CanProcessFile(extension));

    [Theory]
    [InlineData(".webp", ".webp")]
    [InlineData(".bmp", ".bmp")]
    [InlineData(".avif", ".avif")]
    [InlineData(".jpe", ".jpe")]
    [InlineData(".jfif", ".jpg")]
    [InlineData(".tif", ".png")]
    [InlineData(".tiff", ".png")]
    public void ConvertThumbnailExtension_MatchesBytesWrittenForFormat(string extension, string expected) =>
        Assert.Equal(expected, _editor.ConvertThumbnailExtension(extension));

    private sealed class ThrowOnceStream : Stream
    {
        private readonly Stream inner;
        private int reads;

        public ThrowOnceStream(Stream inner)
        {
            this.inner = inner;
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => inner.Length;

        public override long Position
        {
            get => inner.Position;
            set => inner.Position = value;
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

        public override int Read(Span<byte> buffer)
        {
            reads++;
            if (reads == 2)
            {
                throw new IOException("header read failed");
            }

            return inner.Read(buffer);
        }

        public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    private static void WriteImage(string path, uint width, uint height, MagickFormat format)
    {
        using var image = new MagickImage(MagickColors.CornflowerBlue, width, height);
        image.Write(path, format);
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