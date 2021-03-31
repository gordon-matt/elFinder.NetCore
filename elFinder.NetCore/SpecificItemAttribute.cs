using System;
using System.IO;

namespace elFinder.NetCore
{
    public class SpecificItemAttribute : ItemAttribute
    {
        public SpecificItemAttribute(string fullName) : base()
        {
            if (fullName == null) throw new ArgumentNullException(nameof(fullName));
            FullName = fullName;
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
