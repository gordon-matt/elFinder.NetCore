using System;
using System.IO;

namespace elFinder.NetCore
{
    public class SpecificItemAttribute : ItemAttribute
    {
        public SpecificItemAttribute(string relativePath, string fromRootDirectoryPath) : base()
        {
            if (relativePath == null) throw new ArgumentNullException(nameof(relativePath));
            if (fromRootDirectoryPath == null) throw new ArgumentNullException(nameof(fromRootDirectoryPath));

            FullName = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(
                    Path.Combine(fromRootDirectoryPath, relativePath)));
        }

        public string FullName { get; }

        public override bool Equals(object obj)
        {
            return (obj as SpecificItemAttribute)?.FullName == FullName;
        }

        public override int GetHashCode()
        {
            return FullName.GetHashCode();
        }
    }
}
