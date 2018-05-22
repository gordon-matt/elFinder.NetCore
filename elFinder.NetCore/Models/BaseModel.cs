using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using elFinder.NetCore.Drivers;
using elFinder.NetCore.Http;
using Newtonsoft.Json;

namespace elFinder.NetCore.Models
{
    public abstract class BaseModel
    {
        protected static readonly DateTime _unixOrigin = new DateTime(1970, 1, 1, 0, 0, 0);

        /// <summary>
        ///  Name of file/dir. Required
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; protected set; }

        /// <summary>
        ///  Hash of current file/dir path, first symbol must be letter, symbols before _underline_ - volume id, Required.
        /// </summary>
        [JsonProperty("hash")]
        public string Hash { get; protected set; }

        /// <summary>
        ///  mime type. Required.
        /// </summary>
        [JsonProperty("mime")]
        public string Mime { get; protected set; }

        /// <summary>
        /// file modification time in unix timestamp. Required.
        /// </summary>
        [JsonProperty("ts")]
        public long UnixTimeStamp { get; protected set; }

        /// <summary>
        ///  file size in bytes
        /// </summary>
        [JsonProperty("size")]
        public long Size { get; protected set; }

        /// <summary>
        ///  is readable
        /// </summary>
        [JsonProperty("read")]
        public byte Read { get; protected set; }

        /// <summary>
        /// is writable
        /// </summary>
        [JsonProperty("write")]
        public byte Write { get; protected set; }

        /// <summary>
        ///  is file locked. If locked that object cannot be deleted and renamed
        /// </summary>
        [JsonProperty("locked")]
        public byte Locked { get; protected set; }

        public static async Task<BaseModel> CreateAsync(IDriver driver, IFile file, RootVolume volume)
        {
            if (file == null) throw new ArgumentNullException("file");
            if (volume == null) throw new ArgumentNullException("volume");

            string parentPath = file.DirectoryName.Substring(volume.RootDirectory.Length);
            string relativePath = file.FullName.Substring(volume.RootDirectory.Length);

            FileModel response;
            if (volume.CanCreateThumbnail(file))
            {
                using (var stream = await file.OpenReadAsync())
                {
                    var dim = volume.PictureEditor.ImageSize(stream);
                    response = new ImageModel
                    {
                        Thumbnail = await volume.GenerateThumbHashAsync(file),
                        Dimension = $"{dim.Width}x{dim.Height}"
                    };
                }
            }
            else
            {
                response = new FileModel();
            }

            response.Read = 1;
            response.Write = volume.IsReadOnly ? (byte)0 : (byte)1;
            response.Locked = ((volume.LockedFolders != null && volume.LockedFolders.Any(f => f == file.Directory.Name)) || volume.IsLocked) ? (byte)1 : (byte)0;
            response.Name = file.Name;
            response.Size = await file.LengthAsync;
            response.UnixTimeStamp = (long)(await file.LastWriteTimeUtcAsync - _unixOrigin).TotalSeconds;
            response.Mime = Http.Mime.GetMimeType(file);
            response.Hash = volume.VolumeId + HttpEncoder.EncodePath(relativePath);
            response.ParentHash = volume.VolumeId + HttpEncoder.EncodePath(parentPath.Length > 0 ? parentPath : file.Directory.Name);
            return response;
        }

        public static async Task<BaseModel> CreateAsync(IDriver driver, IDirectory directory, RootVolume volume)
        {
            if (directory == null)
            {
                throw new ArgumentNullException("directory");
            }

            if (volume == null)
            {
                throw new ArgumentNullException("volume");
            }

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
                    UnixTimeStamp = (long)(DateTime.UtcNow - _unixOrigin).TotalSeconds,
                    VolumeId = volume.VolumeId
                };
                return response;
            }
            else
            {
                string parentPath = directory.Parent.FullName.Substring(volume.RootDirectory.Length);
                var response = new DirectoryModel
                {
                    Mime = "directory",
                    ContainsChildDirs = (await directory.GetDirectoriesAsync()).Count() > 0 ? (byte)1 : (byte)0,
                    Hash = volume.VolumeId + HttpEncoder.EncodePath(directory.FullName.Substring(volume.RootDirectory.Length)),
                    Read = 1,
                    Write = volume.IsReadOnly ? (byte)0 : (byte)1,
                    Locked = ((volume.LockedFolders != null && volume.LockedFolders.Any(f => f == directory.Name)) || volume.IsLocked) ? (byte)1 : (byte)0,
                    Size = 0,
                    Name = directory.Name,
                    UnixTimeStamp = (long)(await directory.LastWriteTimeUtcAsync - _unixOrigin).TotalSeconds,
                    ParentHash = volume.VolumeId + HttpEncoder.EncodePath(parentPath.Length > 0 ? parentPath : directory.Parent.Name)
                };
                return response;
            }
        }
    }
}