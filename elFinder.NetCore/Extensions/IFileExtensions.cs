using System;
using System.Linq;
using elFinder.NetCore.Drivers;

namespace elFinder.NetCore.Extensions
{
    public static class IFileExtensions
    {
        public static byte GetLockedFlag(this IFile file, RootVolume volume)
        {
            if (volume.IsLocked)
            {
                return 1;
            }

            return GetFlag(file, volume, x => x.Locked);
        }

        public static byte GetReadFlag(this IFile file, RootVolume volume)
        {
            return GetFlag(file, volume, x => x.Read);
        }

        public static byte GetWriteFlag(this IFile file, RootVolume volume)
        {
            if (volume.IsReadOnly)
            {
                return 0;
            }

            return GetFlag(file, volume, x => x.Write);
        }

        private static byte GetFlag(this IFile file, RootVolume volume, Func<AccessControlAttributeSet, bool> fieldSelector)
        {
            if (volume.AccessControlAttributes != null)
            {
                var attributeSet = volume.AccessControlAttributes.FirstOrDefault(x => x.FullName == file.FullName);
                if (attributeSet != null)
                {
                    return fieldSelector(attributeSet) ? (byte)1 : (byte)0;
                }

                var parentDirectory = file.Directory;
                while (parentDirectory != null && parentDirectory.FullName != volume.RootDirectory)
                {
                    attributeSet = volume.AccessControlAttributes.FirstOrDefault(x => x.FullName == parentDirectory.FullName);
                    if (attributeSet != null)
                    {
                        return fieldSelector(attributeSet) ? (byte)1 : (byte)0;
                    }

                    parentDirectory = parentDirectory.Parent;
                }
            }

            return fieldSelector(volume.DefaultAccessControlAttributes) ? (byte)1 : (byte)0;
        }
    }
}