using elFinder.NetCore.Drawing;
using elFinder.NetCore.Drivers;
using elFinder.NetCore.Helpers;

namespace elFinder.NetCore;

public class FullPath
{
    #region Constructors

    public FullPath(RootVolume root, IFile file, string hashedTarget) : this(root, file.FullName, hashedTarget, false)
    {
        File = file ?? throw new ArgumentNullException(nameof(file), "IFile cannot be null");
    }

    public FullPath(RootVolume root, IDirectory dir, string hashedTarget) : this(root, dir.FullName, hashedTarget, true)
    {
        Directory = dir ?? throw new ArgumentNullException(nameof(dir), "IDirectory cannot be null");
    }

    public FullPath(RootVolume root, string path, string hashedTarget, bool isDirectory)
    {
        RootVolume = root ?? throw new ArgumentNullException(nameof(root), "RootVolume cannot be null");
        HashedTarget = hashedTarget;
        IsDirectory = isDirectory;

        RelativePath = path.StartsWith(root.RootDirectory)
            ? path.Length == root.RootDirectory.Length ? string.Empty : path[(root.RootDirectory.Length + 1)..]
            : throw new InvalidOperationException("path must be in the root directory or a subdirectory thereof");
    }

    #endregion Constructors

    public IDirectory Directory { get; protected set; }

    public IFile File { get; protected set; }

    public string HashedTarget { get; set; }

    public bool IsDirectory { get; protected set; }

    public bool IsRoot { get; protected set; }

    public string RelativePath { get; protected set; }

    public RootVolume RootVolume { get; protected set; }

    public Task<ImageWithMimeType> GenerateThumbnailAsync(CancellationToken cancellationToken = default)
    {
        string name = File.FullName;
        for (int i = name.Length - 1; i >= 0; i--)
        {
            if (name[i] == '_')
            {
                name = name[..i];
                break;
            }
        }

        // The thumb hash keeps the source extension, so stripping _{md5} recovers the original file.
        string fullPath = $"{name}{File.Extension}";
        return GenerateThumbnailCoreAsync(fullPath, cancellationToken);
    }

    private async Task<ImageWithMimeType> GenerateThumbnailCoreAsync(string fullPath, CancellationToken cancellationToken)
    {
        if (RootVolume.ThumbnailDirectory != null)
        {
            var thumbFile = File.FullName.StartsWith(RootVolume.ThumbnailDirectory)
                ? File
                : File.Open($"{RootVolume.ThumbnailDirectory}{RootVolume.DirectorySeparatorChar}{RelativePath}");
            if (!await thumbFile.ExistsAsync)
            {
                var thumbDir = thumbFile.Directory;
                if (!await thumbDir.ExistsAsync)
                {
                    await thumbDir.CreateAsync();
                    thumbDir.Attributes = FileAttributes.Hidden;
                }

                var thumb = await CreateThumbnailAsync(fullPath, cancellationToken);
                if (thumb == null)
                {
                    return null;
                }

                await thumbFile.PutAsync(thumb.ImageStream);
                thumb.ImageStream.Position = 0;
                return thumb;
            }

            // Video thumbs are JPEG bytes stored under the source extension so the hash still maps back.
            string mimeType = RootVolume.VideoEditor.CanProcessFile(thumbFile.Extension)
                ? "image/jpeg"
                : MimeHelper.GetMimeType(RootVolume.PictureEditor.ConvertThumbnailExtension(thumbFile.Extension));
            return new ImageWithMimeType(mimeType, await thumbFile.OpenReadAsync());
        }

        return await CreateThumbnailAsync(fullPath, cancellationToken);
    }

    private async Task<ImageWithMimeType> CreateThumbnailAsync(string fullPath, CancellationToken cancellationToken)
    {
        if (RootVolume.VideoEditor.CanProcessFile(Path.GetExtension(fullPath)))
        {
            return await RootVolume.VideoEditor.GenerateThumbnailAsync(fullPath, RootVolume.ThumbnailSize, true, cancellationToken);
        }

        using var original = await File.Open(fullPath).OpenReadAsync();
        return RootVolume.PictureEditor.GenerateThumbnail(original, RootVolume.ThumbnailSize, true);
    }
}