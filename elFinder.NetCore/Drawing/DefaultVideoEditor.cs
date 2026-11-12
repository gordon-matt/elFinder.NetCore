using System.Drawing;
using elFinder.NetCore.Drivers;
using elFinder.NetCore.Helpers;
using ImageMagick;
using ThumbGen;
using ThumbGen.Builder;
using ThumbGen.FrameCapture;

namespace elFinder.NetCore.Drawing
{
    public class DefaultVideoEditor(Color backgroundColor) : IVideoEditor
    {
        public DefaultVideoEditor() : this(Color.Transparent)
        {
        }
        private static Size VideoSize { get; set; } = default!;

        public Color BackgroundColor { get; set; } = backgroundColor;

        public bool CanProcessFile(string fileExtension)
        {
            string ext = fileExtension.ToLowerInvariant();
            return ext is ".mp4" or ".mpeg" or ".mpg" or ".mkv" or ".m4v" or ".avi" or ".mov" or ".webm" or ".wmv";
        }
        private static RootVolume _rootVolume { get; set; }

        public async Task<VideoWithMimeType> GenerateThumbnailAsync(RootVolume rootVolume, IFile file,
            bool keepAspectRatio, CancellationToken cancellationToken = default)
        {
            _rootVolume = rootVolume;
            // set the thumbnail size
            Size tsize = new(_rootVolume.ThumbnailSize, _rootVolume.ThumbnailSize);
            if (CanProcessFile(file.Extension))
            {
                var thumb = await CreateThumbnailUsingFFmpeg(file, tsize);
                return thumb;
            }
            return null;
        }

        public async Task<VideoWithMimeType> GenerateThumbnailInBackgroundAsync(RootVolume rootVolume, IFile file,
            bool keepAspectRatio, CancellationToken cancellationToken = default)
        {
            _rootVolume = rootVolume;
            Size tsize = new(_rootVolume.ThumbnailSize, _rootVolume.ThumbnailSize);
            var thumb = await CreateThumbnailUsingFFmpeg(file, tsize);

            return await Task.FromResult(thumb);
        }

        protected static async Task<VideoWithMimeType> CreateThumbnailUsingFFmpeg(IFile file, Size size)
        {
            string mimeType = MimeHelper.GetMimeType(file.Extension);
            try
            {
                string outFilePath = await _rootVolume.GenerateThumbPathAsync(file);
                string mediaFilePath = file.FullName;
                var captureTime = TimeSpan.FromSeconds(5);
                // create the thumbnail on disk if needed.
                if (!File.Exists(outFilePath))
                {
                    if (captureTime < TimeSpan.Zero)
                        throw new ArgumentException("The timestamp must not be negative.");

                    if (!File.Exists(mediaFilePath))
                        throw new FileNotFoundException("The input video does not exist.", mediaFilePath);

                    string directory = Path.GetDirectoryName(outFilePath)!;
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    string temporaryFile = Path.Combine(directory, $".thumbnail-{Guid.NewGuid():N}.jpg");
                    try
                    {
                        var thumbnailGenerator = ThumbnailGeneratorBuilder
                            .Create()
                            .WithFFmpegVideoCapture()
                            .UseMagickRenderer()
                            .ConfigureRendering(renderingOpts => renderingOpts
                                .WithFrameSize(size.Width, size.Height)
                                .WithTiling(options =>
                                {
                                    options.Columns = 1;
                                    options.Rows = 1;
                                })
                                .PreserveFrameAspect(false))
                            .Build(mediaFilePath);

                        var opts = new ThumbGenOptions()
                            .WithStartTime(captureTime)
                            .WithOutputFilename(Path.GetFullPath(temporaryFile));

                        await thumbnailGenerator.ExecuteAsync(opts);
                        File.Move(temporaryFile, outFilePath, overwrite: true);
                        VideoSize = new Size(thumbnailGenerator.VideoSize.Width, thumbnailGenerator.VideoSize.Height);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("ThumbGen {0}", ex.Message);
                        return null;
                    }
                    finally
                    {
                        File.Delete(temporaryFile);
                    }

                    MagickImage img = new(outFilePath);
                    var memoryStream = new MemoryStream();
                    img.Write(memoryStream, MagickFormat.Jpg);
                    memoryStream.Position = 0;
                    return new VideoWithMimeType(mimeType, memoryStream, VideoSize);
                }
                else
                {
                    VideoFrameCaptureManager frameCaptureManager = new(new VideoFrameExtractor(file.FullName));
                    VideoSize = new Size(frameCaptureManager.Width, frameCaptureManager.Height);
                    return new VideoWithMimeType(mimeType, new MemoryStream(), VideoSize);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
