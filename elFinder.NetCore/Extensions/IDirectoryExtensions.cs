using System;
using System.Linq;
using elFinder.NetCore.Drivers;

namespace elFinder.NetCore.Extensions
{
    public static class IDirectoryExtensions
    {
        public static byte GetLockedFlag(this IDirectory directory, RootVolume volume)
        {
            if (volume.IsLocked)
            {
                return 1;
            }

            return GetFlag(directory, volume, x => x.Locked);
        }

        public static byte GetReadFlag(this IDirectory directory, RootVolume volume)
        {
            return GetFlag(directory, volume, x => x.Read);
        }

        public static byte GetWriteFlag(this IDirectory directory, RootVolume volume)
        {
            if (volume.IsReadOnly)
            {
                return 0;
            }

            return GetFlag(directory, volume, x => x.Write);
        }

        private static byte GetFlag(this IDirectory directory, RootVolume volume, Func<AccessControlAttributeSet, bool> fieldSelector)
        {
            if (volume.AccessControlAttributes != null)
            {
                var attributeSet = volume.AccessControlAttributes.FirstOrDefault(x => x.FullName == directory.FullName);
                if (attributeSet != null)
                {
                    return fieldSelector(attributeSet) ? (byte)1 : (byte)0;
                }

                var parentDirectory = directory.Parent;
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