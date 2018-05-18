using System;
using System.Threading.Tasks;
using elFinder.NetCore.Drawing;
using elFinder.NetCore.Drivers;
using elFinder.NetCore.Helpers;

namespace elFinder.NetCore
{
    public class FullPath
    {
        public string HashedTarget { get; set; }

        public IFile File { get; protected set; }

        public IDirectory Directory { get; protected set; }

        public bool IsDirectory { get; protected set; }

        public bool IsRoot { get; protected set; }

        public string RelativePath { get; protected set; }

        public RootVolume RootVolume { get; protected set; }

        public FullPath(RootVolume root, IFile file, string hashedTarget) : this(root, file.FullName, hashedTarget, false)
        {
            File = file ?? throw new ArgumentNullException("file", "IFile cannot be null");
        }

        public FullPath(RootVolume root, IDirectory dir, string hashedTarget) : this(root, dir.FullName, hashedTarget, true)
        {
            Directory = dir ?? throw new ArgumentNullException("dir", "IDirectory cannot be null");
        }

        public FullPath(RootVolume root, string path, string hashedTarget, bool isDirectory)
        {
            RootVolume = root ?? throw new ArgumentNullException("root", "RootVolume cannot be null");
            HashedTarget = hashedTarget;
            IsDirectory = isDirectory;

            if (path.StartsWith(root.RootDirectory))
            {
                if (path.Length == root.RootDirectory.Length)
                    RelativePath = string.Empty;
                else
                    RelativePath = path.Substring(root.RootDirectory.Length + 1);
            }
            else
            {
                throw new InvalidOperationException("path must be in the root directory or a subdirectory thereof");
            }
        }

        public async Task<ImageWithMimeType> GenerateThumbnail()
        {
            string name = File.FullName;
            for (int i = name.Length - 1; i >= 0; i--)
            {
                if (name[i] == '_')
                {
                    name = name.Substring(0, i);
                    break;
                }
            }

            string fullPath = $"{name}{File.Extension}";

            if (RootVolume.ThumbnailDirectory != null)
            {
                IFile thumbFile;
                if (File.FullName.StartsWith(RootVolume.ThumbnailDirectory))
                {
                    thumbFile = File;
                }
                else
                {
                    thumbFile = File.Clone(string.Concat(RootVolume.ThumbnailDirectory, "/", RelativePath));
                }

                if (!await thumbFile.ExistsAsync)
                {
                    if (!await thumbFile.Directory.ExistsAsync)
                    {
                        await thumbFile.Directory.CreateAsync();
                    }

                    using (var original = await File.Clone(fullPath).OpenReadAsync())
                    {
                        var thumb = RootVolume.PictureEditor.GenerateThumbnail(original, RootVolume.ThumbnailSize, true);
                        await thumbFile.PutAsync(thumb.ImageStream);
                        thumb.ImageStream.Position = 0;
                        return thumb;
                    }
                }
                else
                {
                    string mimeType = Utils.GetMimeType(RootVolume.PictureEditor.ConvertThumbnailExtension(thumbFile.Extension));
                    return new ImageWithMimeType(mimeType, await thumbFile.OpenReadAsync());
                }
            }
            else
            {
                using (var original = await File.Clone(fullPath).OpenReadAsync())
                {
                    return RootVolume.PictureEditor.GenerateThumbnail(original, RootVolume.ThumbnailSize, true);
                }
            }
        }
    }
}