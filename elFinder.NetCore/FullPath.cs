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
            await EnsureHiddenThumbnailDirectoryAsync(thumbFile.Directory);
            if (!await thumbFile.ExistsAsync)
            {
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

    private async Task EnsureHiddenThumbnailDirectoryAsync(IDirectory thumbDirectory)
    {
        // Directory.Create creates missing parents, and only the leaf was marked hidden.
        var missing = new List<IDirectory>();
        var current = thumbDirectory;
        while (current != null && !await current.ExistsAsync)
        {
            missing.Add(current);
            current = current.Parent;
        }

        if (missing.Count > 0)
        {
            await thumbDirectory.CreateAsync();
        }

        foreach (var directory in missing)
        {
            await MarkHiddenAsync(directory);
        }

        string thumbnailRoot = NormalizePath(RootVolume.ThumbnailDirectory);
        current = thumbDirectory;
        while (current != null && IsInsideDirectory(NormalizePath(current.FullName), thumbnailRoot))
        {
            var parent = current.Parent;
            await MarkHiddenAsync(current);
            current = parent;
        }

        // A thumbnail root such as ".tmb/Files" leaves ".tmb" outside ThumbnailDirectory.
        if (current != null
            && current.Name.Equals(".tmb", StringComparison.OrdinalIgnoreCase)
            && !NormalizePath(current.FullName).Equals(NormalizePath(RootVolume.RootDirectory), StringComparison.OrdinalIgnoreCase))
        {
            await MarkHiddenAsync(current);
        }
    }

    private static async Task MarkHiddenAsync(IDirectory directory)
    {
        await directory.RefreshAsync();
        if (!await directory.ExistsAsync)
        {
            return;
        }

        var attributes = directory.Attributes & ~FileAttributes.Directory;
        if ((attributes & FileAttributes.Hidden) != 0)
        {
            return;
        }

        directory.Attributes = attributes | FileAttributes.Hidden;
    }

    private static string NormalizePath(string path) =>
        Path.TrimEndingDirectorySeparator(path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));

    private static bool IsInsideDirectory(string directory, string root) =>
        directory.Equals(root, StringComparison.OrdinalIgnoreCase)
        || directory.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);

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