using System;

namespace elFinder.NetCore
{
    public class AccessControlAttributeSet
    {
        public bool Read { get; set; } = true;

        public bool Write { get; set; } = true;

        public bool Locked { get; set; }
    }

    public class NamedAccessControlAttributeSet : AccessControlAttributeSet
    {
        public NamedAccessControlAttributeSet() : base()
        {
        }

        public NamedAccessControlAttributeSet(string fullName) : base()
        {
            FullName = fullName ?? throw new ArgumentNullException(nameof(fullName));
        }

        public string FullName { get; }

        public override bool Equals(object obj)
        {
            return (obj as NamedAccessControlAttributeSet)?.FullName == FullName;
        }

        public override int GetHashCode()
        {
            return FullName.GetHashCode();
        }
    }
}