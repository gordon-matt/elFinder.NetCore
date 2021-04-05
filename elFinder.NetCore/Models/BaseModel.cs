using System;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using elFinder.NetCore.Drivers;
using elFinder.NetCore.Extensions;
using elFinder.NetCore.Helpers;

namespace elFinder.NetCore.Models
{
    public abstract class BaseModel
    {
        protected static readonly DateTime unixOrigin = new DateTime(1970, 1, 1, 0, 0, 0);

        /// <summary>
        ///  Name of file/dir. Required
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; protected set; }

        /// <summary>
        ///  Hash of current file/dir path, first symbol must be letter, symbols before _underline_ - volume id, Required.
        /// </summary>
        [JsonPropertyName("hash")]
        public string Hash { get; protected set; }

        /// <summary>
        ///  mime type. Required.
        /// </summary>
        [JsonPropertyName("mime")]
        public string Mime { get; protected set; }

        /// <summary>
        /// file modification time in unix timestamp. Required.
        /// </summary>
        [JsonPropertyName("ts")]
        public long UnixTimeStamp { get; protected set; }

        /// <summary>
        ///  file size in bytes
        /// </summary>
        [JsonPropertyName("size")]
        public long Size { get; protected set; }

        /// <summary>
        ///  is readable
        /// </summary>
        [JsonPropertyName("read")]
        public byte Read { get; protected set; }

        /// <summary>
        /// is writable
        /// </summary>
        [JsonPropertyName("write")]
        public byte Write { get; protected set; }

        /// <summary>
        ///  is file locked. If locked that object cannot be deleted and renamed
        /// </summary>
        [JsonPropertyName("locked")]
        public byte Locked { get; protected set; }

        public static async Task<FileModel> CreateAsync(IFile file, RootVolume volume)
        {
            if (file == null) throw new ArgumentNullException("file");
            if (volume == null) throw new ArgumentNullException("volume");

            await file.RefreshAsync();

            string parentPath = file.DirectoryName.Substring(volume.RootDirectory.Length);
            string relativePath = file.FullName.Substring(volume.RootDirectory.Length);

            var fileLength = await file.LengthAsync;

            FileModel response;
            if (volume.CanCreateThumbnail(file) && fileLength > 0)
            {
                using (var stream = await file.OpenReadAsync())
                {
                    try
                    {
                        var dim = volume.PictureEditor.ImageSize(stream);
                        response = new ImageModel
                        {
                            Thumbnail = await volume.GenerateThumbHashAsync(file),
                            Dimension = $"{dim.Width}x{dim.Height}"
                        };
                    }
                    catch
                    {
                        // Fix for non-standard formats
                        // https://github.com/gordon-matt/elFinder.NetCore/issues/36
                        response = new FileModel();
                    }
                }
            }
            else
            {
                response = new FileModel();
            }

            response.Read = file.GetReadFlag(volume);
            response.Write = file.GetWriteFlag(volume);
            response.Locked = file.GetLockedFlag(volume);
            response.Name = file.Name;
            response.Size = fileLength;
            response.UnixTimeStamp = (long)(await file.LastWriteTimeUtcAsync - unixOrigin).TotalSeconds;
            response.Mime = MimeHelper.GetMimeType(file.Extension);
            response.Hash = volume.VolumeId + HttpEncoder.EncodePath(relativePath);
            response.ParentHash = volume.VolumeId + HttpEncoder.EncodePath(parentPath.Length > 0 ? parentPath : file.Directory.Name);
            return response;
        }

        public static async Task<DirectoryModel> CreateAsync(IDirectory directory, RootVolume volume)
        {
            if (directory == null)
            {
                throw new ArgumentNullException("directory");
            }

            if (volume == null)
            {
                throw new ArgumentNullException("volume");
            }

            await directory.RefreshAsync();

            if (volume.RootDirectory == directory.FullName)
            {
                bool hasSubdirs = false;
                var subdirs = await directory.GetDirectoriesAsync();
                foreach (var item in subdirs)
                {
                    if (!item.Attributes.HasFlag(FileAttributes.Hidden))
                    {
                        hasSubdirs = true;
                        break;
                    }
                }

                var response = new RootModel
                {
                    Mime = "directory",
                    Dirs = hasSubdirs ? (byte)1 : (byte)0,
                    Hash = volume.VolumeId + HttpEncoder.EncodePath(directory.Name),
                    Read = 1,
                    Write = volume.IsReadOnly ? (byte)0 : (byte)1,
                    Locked = volume.IsLocked ? (byte)1 : (byte)0,
                    Name = volume.Alias,
                    Size = 0,
                    UnixTimeStamp = (long)(DateTime.UtcNow - unixOrigin).TotalSeconds,
                    VolumeId = volume.VolumeId
                };
                return response;
            }
            else
            {
                string parentPath = directory.Parent.FullName.Substring(volume.RootDirectory.Length);
                string relativePath = directory.FullName.Substring(volume.RootDirectory.Length).TrimEnd(Path.DirectorySeparatorChar);
                var response = new DirectoryModel
                {
                    Mime = "directory",
                    Dirs = (await directory.GetDirectoriesAsync()).Count() > 0 ? (byte)1 : (byte)0,
                    Hash = volume.VolumeId + HttpEncoder.EncodePath(relativePath),
                    Read = directory.GetReadFlag(volume),
                    Write = directory.GetWriteFlag(volume),
                    Locked = directory.GetLockedFlag(volume),
                    Size = 0,
                    Name = directory.Name,
                    UnixTimeStamp = (long)(await directory.LastWriteTimeUtcAsync - unixOrigin).TotalSeconds,
                    ParentHash = volume.VolumeId + HttpEncoder.EncodePath(parentPath.Length > 0 ? parentPath : directory.Parent.Name),
                    VolumeId = volume.VolumeId
                };
                return response;
            }
        }
    }
}