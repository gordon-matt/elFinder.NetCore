using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace elFinder.NetCore
{
    public class ItemAttribute
    {
        public ItemAttribute()
        {
            Read = true;
            Write = true;
            Locked = false;
        }

        /// <summary>
        /// Default: true
        /// </summary>
        public bool Read { get; set; }

        /// <summary>
        /// Default: true
        /// </summary>
        public bool Write { get; set; }

        /// <summary>
        /// Gets or sets a list of root subfolders that should be locked (user can't remove, rename). Default: false
        /// </summary>
        public bool Locked { get; set; }
    }
}
