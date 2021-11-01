using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using elFinder.NetCore.Drawing;
using elFinder.NetCore.Drivers;
using elFinder.NetCore.Helpers;

namespace elFinder.NetCore
{
    /// <summary>
    /// Represents a root of file system
    /// </summary>
    public class RootVolume
    {
        private AccessControlAttributeSet defaultAccessControlAttributes = new AccessControlAttributeSet();

        /// <summary>
        ///
        /// </summary>
        /// <param name="rootDirectory">The root directory (physical location)</param>
        /// <param name="url">URL of root directory</param>
        /// <param name="thumbnailsUrl">URL of where to find thumbnails</param>
        /// <param name="directorySeparatorChar">Character used to separate directory levels in a path string. Default is System.IO.Path.DirectorySeparatorChar</param>
        public RootVolume(
            string rootDirectory,
            string url,
            string thumbnailsUrl = null,
            char directorySeparatorChar = default)
        {
            if (rootDirectory == null)
            {
                throw new ArgumentNullException("rootDirectory", "Root directory cannot be null");
            }

            Alias = Path.GetFileNameWithoutExtension(rootDirectory);
            RootDirectory = rootDirectory;
            Url = url;
            UploadOverwrite = true;
            PictureEditor = new DefaultPictureEditor();

            // https://github.com/EvgenNoskov/Elfinder.NET/blob/fb19f17a3682ed81cadcfea978dcce575806eebd/docs/Documentation.md
            if (!string.IsNullOrEmpty(thumbnailsUrl))
            {
                ThumbnailUrl = thumbnailsUrl;
            }

            DirectorySeparatorChar = directorySeparatorChar == default(char) ? Path.DirectorySeparatorChar : directorySeparatorChar; // Can be changed for other providers
            ThumbnailDirectory = $"{rootDirectory}{DirectorySeparatorChar}.tmb";
            UploadOrder = new[] { "deny", "allow" };
        }

        /// <summary>
        /// Gets or sets the path separator char to use. Default is System.IO.Path.DirectorySeparatorChar.
        /// </summary>
        public char DirectorySeparatorChar { get; }

        /// <summary>
        /// Get or sets alias for root. If not set will use directory name of path
        /// </summary>
        public string Alias { get; set; }

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
        public bool IsShowOnly { get; }

        /// <summary>
        /// Set of named item attributes used for permissions, access control.
        /// </summary>
        public ICollection<NamedAccessControlAttributeSet> AccessControlAttributes { get; set; }

        /// <summary>
        /// Default attribute for files/directories if not any named item attribute detected.
        /// Note: This can not be null
        /// </summary>
        public AccessControlAttributeSet DefaultAccessControlAttributes
        {
            get => defaultAccessControlAttributes;
            set => defaultAccessControlAttributes = value ?? throw new ArgumentNullException(nameof(DefaultAccessControlAttributes));
        }

        /// <summary>
        /// Get or sets maximum upload file size. This size is per files in bytes.
        /// Note: you still to configure maxupload limits in web.config for whole application
        /// </summary>
        public int? MaxUploadSize { get; set; }

        /// <summary>
        /// Get or sets maximum upload file size. This size is per files in kb.
        /// Note: you still to configure maxupload limits in web.config for whole application
        /// </summary>
        public double? MaxUploadSizeInKb
        {
            get { return MaxUploadSize.HasValue ? (double?)(MaxUploadSize.Value / 1024.0) : null; }
            set { MaxUploadSize = value.HasValue ? (int?)(value * 1024) : null; }
        }

        /// <summary>
        /// Get or sets maximum upload file size. This size is per files in Mb.
        /// Note: you still to configure maxupload limits in web.config for whole application
        /// </summary>
        public double? MaxUploadSizeInMb
        {
            get { return MaxUploadSizeInKb.HasValue ? (double?)(MaxUploadSizeInKb.Value / 1024.0) : null; }
            set { MaxUploadSizeInKb = value.HasValue ? (int?)(value * 1024) : null; }
        }

        /// <summary>
        /// Gets the picture editor for this volume
        /// </summary>
        public IPictureEditor PictureEditor { get; }

        /// <summary>
        /// Get or sets a directory which is root
        /// </summary>
        public string RootDirectory { get; }

        /// <summary>
        /// Get or sets a subfolder of root diretory, which will be start
        /// </summary>
        public string StartDirectory { get; set; }

        /// <summary>
        /// Get ot sets thumbnals directory
        /// </summary>
        public string ThumbnailDirectory { get; private set; }

        /// <summary>
        /// Thumbnails size in pixels. Thumbnails are square
        /// </summary>
        public int ThumbnailSize { get; set; } = 48;

        /// <summary>
        /// Get ot sets thumbnails url
        /// </summary>
        public string ThumbnailUrl { get; }

        /// <summary>
        /// Replace files with the same name on upload or give them new names. true - replace old files, false give new names like original_name-number.ext
        /// </summary>
        public bool UploadOverwrite { get; }

        /// <summary>
        /// URL that points to path directory (also called 'root URL'). If not set client will not see full path to files (replacement
        /// for old fileURL option), also all files downloads will be handled by connector.
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// Gets a autogenerated prefix of root
        /// </summary>
        public string VolumeId { get; set; }

        /// <summary>
        /// Mimetypes allowed to upload
        /// </summary>
        public IEnumerable<string> UploadAllow { get; set; }

        /// <summary>
        /// Mimetypes not allowed to upload. Same values accepted as in uploadAllow
        /// </summary>
        public IEnumerable<string> UploadDeny { get; set; }

        /// <summary>
        /// Order to proccess uploadAllow and uploadDeny options. Logic is the same as Apache web server options Allow, Deny, Order
        /// </summary>
        public IEnumerable<string> UploadOrder { get; set; }

        public bool CanCreateThumbnail(IFile input)
        {
            return ThumbnailUrl != null && PictureEditor.CanProcessFile(input.Extension);
        }

        public async Task<string> GenerateThumbHashAsync(IFile originalImage)
        {
            if (ThumbnailDirectory == null)
            {
                string md5 = await originalImage.GetFileMd5Async();
                string thumbName = $"{Path.GetFileNameWithoutExtension(originalImage.Name)}_{md5}{originalImage.Extension}";
                string relativePath = originalImage.DirectoryName.Substring(RootDirectory.Length);
                return VolumeId + HttpEncoder.EncodePath($"{relativePath}{DirectorySeparatorChar}{thumbName}");
            }
            else
            {
                string thumbPath = await GenerateThumbPathAsync(originalImage);
                string relativePath = thumbPath.Substring(ThumbnailDirectory.Length);
                return VolumeId + HttpEncoder.EncodePath(relativePath);
            }
        }

        public async Task<string> GenerateThumbPathAsync(IFile originalImage)
        {
            if (ThumbnailDirectory == null || !CanCreateThumbnail(originalImage))
            {
                return null;
            }
            string relativePath = originalImage.FullName.Substring(RootDirectory.Length);
            string thumbDir = GetDirectoryName($"{ThumbnailDirectory}{relativePath}");
            string md5 = await originalImage.GetFileMd5Async();
            string thumbName = $"{Path.GetFileNameWithoutExtension(originalImage.Name)}_{md5}{originalImage.Extension}";
            return $"{thumbDir}{DirectorySeparatorChar}{thumbName}";
        }

        public string GenerateThumbPath(IDirectory originalDirectory)
        {
            if (ThumbnailDirectory == null)
            {
                return null;
            }
            string relativePath = originalDirectory.FullName.Substring(RootDirectory.Length);
            return ThumbnailDirectory + relativePath;
        }

        private string GetDirectoryName(string file)
        {
            int length = file.Length;
            int startIndex = length;
            while (--startIndex >= 0)
            {
                char ch = file[startIndex];
                if (ch == DirectorySeparatorChar)
                {
                    return file.Substring(0, startIndex);
                }
            }
            return string.Empty;
        }

        internal async Task<string> GetExistingThumbHashAsync(IFile originalImage)
        {
            string thumbPath = await GetExistingThumbPathAsync(originalImage);
            if (thumbPath == null)
            {
                return null;
            }
            string relativePath = thumbPath.Substring(ThumbnailDirectory.Length);
            return VolumeId + HttpEncoder.EncodePath(relativePath);
        }

        internal async Task<string> GetExistingThumbPathAsync(IFile originalImage)
        {
            string thumbPath = await GenerateThumbPathAsync(originalImage);
            return thumbPath;
        }

        internal string GetExistingThumbPath(IDirectory originalDirectory)
        {
            if (ThumbnailDirectory == null)
            {
                return null;
            }

            string relativePath = originalDirectory.FullName.Substring(RootDirectory.Length);
            string thumbDir = ThumbnailDirectory + relativePath;
            return thumbDir;
        }
    }
}