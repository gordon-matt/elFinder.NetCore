using elFinder.NetCore.Drivers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace elFinder.NetCore.Extensions
{
    public static class IFileExtensions
    {
        public static byte GetLockedFlag(this IFile file, RootVolume volume)
        {
            if (volume.IsLocked) return 1;

            if (volume.ItemAttributes != null)
            {
                var specificAttr = volume.ItemAttributes.FirstOrDefault(attr => attr.Name == file.Name);

                if (specificAttr != null) return specificAttr.Locked ? (byte)1 : (byte)0;

                var currentParent = file.Directory;
                var rootPath = Path.GetFullPath(volume.RootDirectory);

                while (currentParent != null && currentParent.FullName != rootPath)
                {
                    specificAttr = volume.ItemAttributes.FirstOrDefault(attr => attr.Name == currentParent.Name);

                    if (specificAttr != null) return specificAttr.Locked ? (byte)1 : (byte)0;

                    currentParent = currentParent.Parent;
                }
            }

            return volume.DefaultAttribute.Locked ? (byte)1 : (byte)0;
        }

        public static byte GetReadFlag(this IFile file, RootVolume volume)
        {
            if (volume.ItemAttributes != null)
            {
                var specificAttr = volume.ItemAttributes.FirstOrDefault(attr => attr.Name == file.Name);

                if (specificAttr != null) return specificAttr.Read ? (byte)1 : (byte)0;

                var currentParent = file.Directory;
                var rootPath = Path.GetFullPath(volume.RootDirectory);

                while (currentParent != null && currentParent.FullName != rootPath)
                {
                    specificAttr = volume.ItemAttributes.FirstOrDefault(attr => attr.Name == currentParent.Name);

                    if (specificAttr != null) return specificAttr.Read ? (byte)1 : (byte)0;

                    currentParent = currentParent.Parent;
                }
            }

            return volume.DefaultAttribute.Read ? (byte)1 : (byte)0;
        }

        public static byte GetWriteFlag(this IFile file, RootVolume volume)
        {
            if (volume.IsReadOnly) return 0;

            if (volume.ItemAttributes != null)
            {
                var specificAttr = volume.ItemAttributes.FirstOrDefault(attr => attr.Name == file.Name);

                if (specificAttr != null) return specificAttr.Write ? (byte)1 : (byte)0;

                var currentParent = file.Directory;
                var rootPath = Path.GetFullPath(volume.RootDirectory);

                while (currentParent != null && currentParent.FullName != rootPath)
                {
                    specificAttr = volume.ItemAttributes.FirstOrDefault(attr => attr.Name == currentParent.Name);

                    if (specificAttr != null) return specificAttr.Write ? (byte)1 : (byte)0;

                    currentParent = currentParent.Parent;
                }
            }

            return volume.DefaultAttribute.Write ? (byte)1 : (byte)0;
        }
    }
}
