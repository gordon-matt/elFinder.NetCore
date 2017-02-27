using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using elFinder.NetCore.Drawing;
using elFinder.NetCore.Helpers;

namespace elFinder.NetCore
{
    /// <summary>
    /// Represents a root of file system
    /// </summary>
    public class Root
    {
        private DirectoryInfo directoryInfo;
        private int? maxUploadSize;
        private IPicturesEditor picturesEditor;
        private DirectoryInfo startPath;
        private DirectoryInfo thumbnailsDirectory;
        private int thumbnailSize;
        private DirectoryInfo thumbnailsStorage;
        private string thumbnailsUrl;
        private bool uploadOverwrite;
        private string url;
        private string volumeId;

        public Root(DirectoryInfo directoryInfo, string url)
        {
            if (directoryInfo == null)
            {
                throw new ArgumentNullException("directory", "Root directory cannot be null");
            }

            if (!directoryInfo.Exists)
            {
                throw new ArgumentException("Root directory must exist", "directory");
            }

            Alias = directoryInfo.Name;
            this.directoryInfo = directoryInfo;
            this.url = url;
            uploadOverwrite = true;
            thumbnailSize = 48;
        }

        public Root(DirectoryInfo directory)
            : this(directory, null)
        {
        }

        /// <summary>
        /// Get or sets alias for root. If not set will use directory name of path
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Get or sets a directory which is root
        /// </summary>
        public DirectoryInfo Directory
        {
            get { return directoryInfo; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Root directory can not be null", "value");
                }
                if (!value.Exists)
                {
                    throw new ArgumentException("Directory must exist", "directory");
                }
                directoryInfo = value;
            }
        }

        /// <summary>
        /// Get or sets if root is locked (user can't remove, rename or delete files or subdirectories)
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Get or sets if root for read only (users can't change file)
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Get or sets if user can only show files (and cannot download).
        /// Note: if you set url property, than users can access to directory by the provided url
        /// </summary>
        public bool IsShowOnly { get; set; }

        /// <summary>
        /// Gets or sets a list of root subfolders that should be locked (user can't remove, rename)
        /// </summary>
        public List<string> LockedFolders { get; set; }

        /// <summary>
        /// Get or sets maximum upload file size. This size is per files in bytes.
        /// Note: you still to configure maxupload limits in web.config for whole application
        /// </summary>
        public int? MaxUploadSize
        {
            get { return maxUploadSize; }
            set
            {
                if (value.HasValue && value.Value < 0)
                {
                    throw new ArgumentException("Max upload size can not be less than zero", "value");
                }
                maxUploadSize = value;
            }
        }

        /// <summary>
        /// Get or sets maximum upload file size. This size is per files in kb.
        /// Note: you still to configure maxupload limits in web.config for whole application
        /// </summary>
        public double? MaxUploadSizeInKb
        {
            get
            {
                return maxUploadSize.HasValue ? (double?)(maxUploadSize.Value / 1024.0) : null;
            }
            set
            {
                MaxUploadSize = value.HasValue ? (int?)(value * 1024) : null;
            }
        }

        /// <summary>
        /// Get or sets maximum upload file size. This size is per files in Mb.
        /// Note: you still to configure maxupload limits in web.config for whole application
        /// </summary>
        public double? MaxUploadSizeInMb
        {
            get { return MaxUploadSizeInKb.HasValue ? (double?)(MaxUploadSizeInKb.Value / 1024.0) : null; }
            set
            {
                MaxUploadSizeInKb = value.HasValue ? (int?)(value * 1024) : null;
            }
        }

        /// <summary>
        /// Get or sets pictures editor. The object responsible for generating thumnails and .
        /// </summary>
        public IPicturesEditor PicturesEditor
        {
            get
            {
                if (picturesEditor == null)
                {
                    picturesEditor = new DefaultPicturesEditor();
                }
                return picturesEditor;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                picturesEditor = value;
            }
        }

        /// <summary>
        /// Get or sets a subfolder of root diretory, which will be start
        /// </summary>
        public DirectoryInfo StartPath
        {
            get { return startPath; }
            set
            {
                if (value != null && !value.Exists)
                {
                    startPath = null;//throw new ArgumentException("Start directory must exist or can be null", "value");
                }
                else
                {
                    startPath = value;
                }
            }
        }

        /// <summary>
        /// Get or sets thumbnails size
        /// </summary>
        public int ThumbnailsSize
        {
            get { return thumbnailSize; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("Size can not be less or equals zero");
                }
                thumbnailSize = value;
            }
        }

        /// <summary>
        /// Get or sets directory for store all thumbnails.
        /// </summary>
        public DirectoryInfo ThumbnailsStorage
        {
            get { return thumbnailsStorage; }
            set
            {
                if (value != null)
                {
                    if (!value.Exists)
                    {
                        throw new ArgumentException("Thumbnails storage directory must exist");
                    }

                    thumbnailsDirectory = new DirectoryInfo(Path.Combine(value.FullName, ".tmb_" + directoryInfo.Name));
                    if (!thumbnailsDirectory.Exists)
                    {
                        thumbnailsDirectory = System.IO.Directory.CreateDirectory(thumbnailsDirectory.FullName);
                        thumbnailsDirectory.Attributes |= FileAttributes.Hidden;
                    }
                }
                else
                {
                    thumbnailsDirectory = value;
                }
                thumbnailsStorage = value;
            }
        }

        /// <summary>
        /// Get ot sets thumbnals url
        /// </summary>
        public string ThumbnailsUrl
        {
            get { return thumbnailsUrl; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("Url can not be null or empty");
                }
                thumbnailsUrl = value;
            }
        }

        /// <summary>
        /// Get or sets if files on upload will replace or give them new names. true - replace old files, false give new names like original_name-number.ext
        /// </summary>
        public bool UploadOverwrite
        {
            get { return uploadOverwrite; }
            set { uploadOverwrite = value; }
        }

        /// <summary>
        /// Get or sets url that points to path directory (also called 'root URL').
        /// </summary>
        public string Url
        {
            get
            {
                return url;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Url can not be null", "value");
                }
                url = value;
            }
        }

        /// <summary>
        /// Gets a autogenerated prefix of root
        /// </summary>
        public string VolumeId
        {
            get { return volumeId; }
            internal set { volumeId = value; }
        }

        internal bool CanCreateThumbnail(FileInfo input)
        {
            return ThumbnailsUrl != null && PicturesEditor.CanProcessFile(input.Extension);
        }

        internal string GenerateThumbHash(FileInfo originalImage)
        {
            if (thumbnailsDirectory == null)
            {
                string thumbName = Path.GetFileNameWithoutExtension(originalImage.Name) + "_" + Utils.GetFileMd5(originalImage) + originalImage.Extension;
                string relativePath = originalImage.DirectoryName.Substring(directoryInfo.FullName.Length);
                return VolumeId + Utils.EncodePath(relativePath + "\\" + thumbName);
            }
            else
            {
                string thumbPath = GenerateThumbPath(originalImage);
                string relativePath = thumbPath.Substring(thumbnailsDirectory.FullName.Length);
                return VolumeId + Utils.EncodePath(relativePath);
            }
        }

        internal ImageWithMimeType GenerateThumbnail(FullPath originalImage)
        {
            string name = originalImage.File.Name;
            for (int i = name.Length - 1; i >= 0; i--)
            {
                if (name[i] == '_')
                {
                    name = name.Substring(0, i);
                    break;
                }
            }
            string fullPath = originalImage.File.DirectoryName + "\\" + name + originalImage.File.Extension;

            if (thumbnailsDirectory != null)
            {
                FileInfo thumbPath;
                if (originalImage.File.FullName.StartsWith(thumbnailsDirectory.FullName))
                {
                    thumbPath = originalImage.File;
                }
                else
                {
                    thumbPath = new FileInfo(Path.Combine(thumbnailsDirectory.FullName, originalImage.RelativePath));
                }

                if (!thumbPath.Exists)
                {
                    if (!thumbPath.Directory.Exists)
                    {
                        System.IO.Directory.CreateDirectory(thumbPath.Directory.FullName);
                    }
                    using (var thumbFile = thumbPath.Create())
                    using (var original = File.OpenRead(fullPath))
                    {
                        var thumb = PicturesEditor.GenerateThumbnail(original, thumbnailSize, true);
                        thumb.ImageStream.CopyTo(thumbFile);
                        thumb.ImageStream.Position = 0;
                        return thumb;
                    }
                }
                else
                {
                    return new ImageWithMimeType(PicturesEditor.ConvertThumbnailExtension(thumbPath.Extension), thumbPath.OpenRead());
                }
            }
            else
            {
                using (var original = File.OpenRead(fullPath))
                {
                    return PicturesEditor.GenerateThumbnail(original, thumbnailSize, true);
                }
            }
        }

        internal string GetExistingThumbHash(FileInfo originalImage)
        {
            string thumbPath = GetExistingThumbPath(originalImage);
            if (thumbPath == null)
            {
                return null;
            }
            string relativePath = thumbPath.Substring(thumbnailsDirectory.FullName.Length);
            return VolumeId + Utils.EncodePath(relativePath);
        }

        internal string GetExistingThumbPath(FileInfo originalImage)
        {
            string thumbPath = GenerateThumbPath(originalImage);
            return thumbPath != null && File.Exists(thumbPath) ? thumbPath : null;
        }

        internal string GetExistingThumbPath(DirectoryInfo originalDirectory)
        {
            if (thumbnailsDirectory == null)
            {
                return null;
            }
            string relativePath = originalDirectory.FullName.Substring(directoryInfo.FullName.Length);
            string thumbDir = thumbnailsDirectory.FullName + relativePath;
            return System.IO.Directory.Exists(thumbDir) ? thumbDir : null;
        }

        internal Size GetImageDimension(FileInfo input)
        {
            if (!input.Exists)
            {
                throw new ArgumentException("File not exist");
            }

            using (var image = Image.FromFile(input.FullName))
            {
                return new Size(image.Width, image.Height);
            }
        }

        private string GenerateThumbPath(FileInfo originalImage)
        {
            if (thumbnailsDirectory == null || !CanCreateThumbnail(originalImage))
            {
                return null;
            }
            string relativePath = originalImage.FullName.Substring(directoryInfo.FullName.Length);
            string thumbDir = Path.GetDirectoryName(thumbnailsDirectory.FullName + relativePath);
            string thumbName = Path.GetFileNameWithoutExtension(originalImage.Name) + "_" + Utils.GetFileMd5(originalImage) + originalImage.Extension;
            return Path.Combine(thumbDir, thumbName);
        }
    }
}