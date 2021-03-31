using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace elFinder.NetCore
{
    public enum MimeDetectOption : byte
    {
        Auto = 0,
        Internal = 1,

        // Not supported
        //FInfo = 2,
        //MimeContentType = 3
    }
}
