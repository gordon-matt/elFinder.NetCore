using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using elFinder.NetCore.Helpers;

namespace elFinder.NetCore.Models
{
    [DataContract]
    internal abstract class BaseModel
    {
        protected static readonly DateTime _unixOrigin = new DateTime(1970, 1, 1, 0, 0, 0);

        /// <summary>
        ///  Name of file/dir. Required
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; protected set; }

        /// <summary>
        ///  Hash of current file/dir path, first symbol must be letter, symbols before _underline_ - volume id, Required.
        /// </summary>
        [DataMember(Name = "hash")]
        public string Hash { get; protected set; }

        /// <summary>
        ///  mime type. Required.
        /// </summary>
        [DataMember(Name = "mime")]
        public string Mime { get; protected set; }

        /// <summary>
        /// file modification time in unix timestamp. Required.
        /// </summary>
        [DataMember(Name = "ts")]
        public long UnixTimeStamp { get; protected set; }

        /// <summary>
        ///  file size in bytes
        /// </summary>
        [DataMember(Name = "size")]
        public long Size { get; protected set; }

        /// <summary>
        ///  is readable
        /// </summary>
        [DataMember(Name = "read")]
        public byte Read { get; protected set; }

        /// <summary>
        /// is writable
        /// </summary>
        [DataMember(Name = "write")]
        public byte Write { get; protected set; }

        /// <summary>
        ///  is file locked. If locked that object cannot be deleted and renamed
        /// </summary>
        [DataMember(Name = "locked")]
        public byte Locked { get; protected set; }

        public static BaseModel Create(FileInfo info, Root root)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            if (root == null)
            {
                throw new ArgumentNullException("root");
            }

            string parentPath = info.Directory.FullName.Substring(root.Directory.FullName.Length);
            string relativePath = info.FullName.Substring(root.Directory.FullName.Length);

            FileModel response;
            if (root.CanCreateThumbnail(info))
            {
                var dim = root.GetImageDimension(info);
                response = new ImageModel
                {
                    Thumbnail = root.GetExistingThumbHash(info) ?? (object)1,
                    Dimension = string.Format("{0}x{1}", dim.Width, dim.Height)
                };
            }
            else
            {
                response = new FileModel();
            }

            response.Read = 1;
            response.Write = root.IsReadOnly ? (byte)0 : (byte)1;
            response.Locked = ((root.LockedFolders != null && root.LockedFolders.Any(f => f == info.Directory.Name)) || root.IsLocked) ? (byte)1 : (byte)0;
            response.Name = info.Name;
            response.Size = info.Length;
            response.UnixTimeStamp = (long)(info.LastWriteTimeUtc - _unixOrigin).TotalSeconds;
            response.Mime = Utils.GetMimeType(info);
            response.Hash = root.VolumeId + Utils.EncodePath(relativePath);
            response.ParentHash = root.VolumeId + Utils.EncodePath(parentPath.Length > 0 ? parentPath : info.Directory.Name);
            return response;
        }

        public static BaseModel Create(DirectoryInfo directory, Root root)
        {
            if (directory == null)
            {
                throw new ArgumentNullException("directory");
            }

            if (root == null)
            {
                throw new ArgumentNullException("root");
            }

            if (root.Directory.FullName == directory.FullName)
            {
                bool hasSubdirs = false;
                DirectoryInfo[] subdirs = directory.GetDirectories();
                foreach (var item in subdirs)
                {
                    if ((item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                    {
                        hasSubdirs = true;
                        break;
                    }
                }
                var response = new RootModel
                {
                    Mime = "directory",
                    Dirs = hasSubdirs ? (byte)1 : (byte)0,
                    Hash = root.VolumeId + Utils.EncodePath(directory.Name),
                    Read = 1,
                    Write = root.IsReadOnly ? (byte)0 : (byte)1,
                    Locked = root.IsLocked ? (byte)1 : (byte)0,
                    Name = root.Alias,
                    Size = 0,
                    UnixTimeStamp = (long)(directory.LastWriteTimeUtc - _unixOrigin).TotalSeconds,
                    VolumeId = root.VolumeId
                };
                return response;
            }
            else
            {
                string parentPath = directory.Parent.FullName.Substring(root.Directory.FullName.Length);
                var response = new DirectoryModel
                {
                    Mime = "directory",
                    ContainsChildDirs = directory.GetDirectories().Length > 0 ? (byte)1 : (byte)0,
                    Hash = root.VolumeId + Utils.EncodePath(directory.FullName.Substring(root.Directory.FullName.Length)),
                    Read = 1,
                    Write = root.IsReadOnly ? (byte)0 : (byte)1,
                    Locked = ((root.LockedFolders != null && root.LockedFolders.Any(f => f == directory.Name)) || root.IsLocked) ? (byte)1 : (byte)0,
                    Size = 0,
                    Name = directory.Name,
                    UnixTimeStamp = (long)(directory.LastWriteTimeUtc - _unixOrigin).TotalSeconds,
                    ParentHash = root.VolumeId + Utils.EncodePath(parentPath.Length > 0 ? parentPath : directory.Parent.Name)
                };
                return response;
            }
        }
    }
}