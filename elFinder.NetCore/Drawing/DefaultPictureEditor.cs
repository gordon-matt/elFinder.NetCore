using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;

namespace elFinder.NetCore.Drawing
{
    /// <summary>
    /// Represents default pictures editor
    /// </summary>
    public class DefaultPictureEditor : IPictureEditor
    {
        #region Constructors

        public DefaultPictureEditor(Color backgroundColor)
        {
            BackgroundColor = backgroundColor;
        }

        public DefaultPictureEditor()
            : this(Color.Transparent)
        {
        }

        #endregion Constructors

        #region IPictureEditor Members

        public Color BackgroundColor { get; set; }

        public bool CanProcessFile(string fileExtension)
        {
            string ext = fileExtension.ToLower();
            return ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".gif" || ext == ".tiff";
        }

        public string ConvertThumbnailExtension(string originalImageExtension)
        {
            string ext = originalImageExtension.ToLower();
            if (ext == ".tiff")
            {
                return ".png";
            }

            if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".gif")
            {
                return ext;
            }
            else
            {
                throw new ArgumentException(typeof(DefaultPictureEditor).FullName + " does not support thumbnails for '" + originalImageExtension + "' files.");
            }
        }

        public ImageWithMimeType Crop(Stream input, int x, int y, int width, int height)
        {
            using (var image = Image.Load(input))
            {
                return ScaleOrCrop(image, new Rectangle(x, y, width, height), new Rectangle(0, 0, width, height));
            }
        }

        public ImageWithMimeType GenerateThumbnail(Stream input, int size, bool keepAspectRatio)
        {
            using (var inputImage = Image.Load(input))
            {
                int targetWidth;
                int targetHeight;

                if (keepAspectRatio)
                {
                    double width = inputImage.Width;
                    double height = inputImage.Height;
                    double percentWidth = width != 0 ? size / width : 0;
                    double percentHeight = height != 0 ? size / height : 0;
                    double percent = percentHeight < percentWidth ? percentHeight : percentWidth;

                    targetWidth = (int)(width * percent);
                    targetHeight = (int)(height * percent);
                }
                else
                {
                    targetWidth = size;
                    targetHeight = size;
                }

                return ScaleOrCrop(
                    inputImage,
                    new Rectangle(0, 0, inputImage.Width, inputImage.Height),
                    new Rectangle(0, 0, targetWidth, targetHeight));
            }
        }

        public Size ImageSize(Stream input)
        {
            using (var image = Image.Load(input))
            {
                return new Size(image.Width, image.Height);
            }
        }

        public ImageWithMimeType Resize(Stream input, int width, int height)
        {
            using (var image = Image.Load(input))
            {
                return ScaleOrCrop(image, new Rectangle(0, 0, image.Width, image.Height), new Rectangle(0, 0, width, height));
            }
        }

        public ImageWithMimeType Rotate(Stream input, int angle)
        {
            using (var image = Image.Load(input))
            {
                return Rotate(image, angle);
            }
        }

        #endregion IPictureEditor Members

        /// <summary>
        /// Creates a new Image containing the same image, only rotated
        /// </summary>
        /// <param name="image">The <see cref="System.Drawing.Image"/> to rotate</param>
        /// <param name="degrees">The amount to rotate the image, clockwise, in degrees</param>
        /// <returns>A new <see cref="System.Drawing.Bitmap"/> that is just large enough to contain the rotated image without cutting any corners off.</returns>
        /// <remarks>Original code can be found at http://www.codeproject.com/Articles/58815/C-Image-PictureBox-Rotations </remarks>
        /// <exception cref="System.ArgumentNullException">Thrown if <see cref="image"/> is null.</exception>
        private ImageWithMimeType Rotate(SixLabors.ImageSharp.Image image, float degrees)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            // Rotate the image using ImageSharp's Rotate method
            image.Mutate(x => x.Rotate(degrees));

            // Convert to a memory stream and return
            var memoryStream = new MemoryStream();
            image.Save(memoryStream, image.Metadata.DecodedImageFormat);
            memoryStream.Position = 0;

            return new ImageWithMimeType("image/jpeg", memoryStream); // Adjust MIME type as needed
        }

        private ImageWithMimeType SaveImage(SixLabors.ImageSharp.Image image, IImageFormat imageFormat)
        {
            var memoryStream = new MemoryStream(); // Will be disposed when "ImageWithMimeType" is disposed

            string mimeType;
            if (imageFormat == SixLabors.ImageSharp.Formats.Jpeg.JpegFormat.Instance)
            {
                image.Save(memoryStream, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder());
                mimeType = "image/jpeg";
            }
            else if (imageFormat == SixLabors.ImageSharp.Formats.Gif.GifFormat.Instance)
            {
                image.Save(memoryStream, new SixLabors.ImageSharp.Formats.Gif.GifEncoder());
                mimeType = "image/gif";
            }
            else // Default to PNG
            {
                image.Save(memoryStream, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
                mimeType = "image/png";
            }
            memoryStream.Position = 0;
            return new ImageWithMimeType(mimeType, memoryStream);
        }

        private ImageWithMimeType ScaleOrCrop(Image image, Rectangle source, Rectangle destination)
        {
            using (var newImage = image.Clone(ctx => ctx.Crop(source).Resize(destination.Width, destination.Height)))
            {
                return SaveImage(newImage, image.Metadata.DecodedImageFormat);
            }
        }
    }
}