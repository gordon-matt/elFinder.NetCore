using System;
using System.IO;

namespace elFinder.NetCore
{
    public class NamedItemAttribute : ItemAttribute
    {
        public NamedItemAttribute(string name) : base()
        {
            if (name == null) throw new ArgumentException(nameof(name));

            Name = name;
        }

        public string Name { get; }

        public string GetFullName(string fromRoot)
        {
            return Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(
                    Path.Combine(fromRoot, Name)));
        }

        public override bool Equals(object obj)
        {
            return (obj as NamedItemAttribute)?.Name == Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
