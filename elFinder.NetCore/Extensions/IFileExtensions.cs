using elFinder.NetCore.Drivers;

namespace elFinder.NetCore.Extensions;

public static class IFileExtensions
{
    extension(IFile file)
    {
        public byte GetLockedFlag(RootVolume volume) =>
            volume.IsLocked ? (byte)1 : GetFlag(file, volume, x => x.Locked);

        public byte GetReadFlag(RootVolume volume) =>
            GetFlag(file, volume, x => x.Read);

        public byte GetWriteFlag(RootVolume volume) =>
            volume.IsReadOnly ? (byte)0 : GetFlag(file, volume, x => x.Write);
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