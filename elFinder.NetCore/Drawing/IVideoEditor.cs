using System.Drawing;
using elFinder.NetCore.Drivers;

namespace elFinder.NetCore.Drawing
{
    public interface IVideoEditor
    {
        bool CanProcessFile(string fileExtension);

        Task<VideoWithMimeType> GenerateThumbnailAsync(RootVolume rootVolume, IFile file,
            bool keepAspectRatio, CancellationToken cancellationToken = default);

        Task<VideoWithMimeType> GenerateThumbnailInBackgroundAsync(RootVolume rootVolume, IFile file,
            bool keepAspectRatio, CancellationToken cancellationToken = default);
    }
}
