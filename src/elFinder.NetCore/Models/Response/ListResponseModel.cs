using System.Collections.Generic;
using System.Runtime.Serialization;

namespace elFinder.NetCore.Models.Response
{
    [DataContract]
    internal class ListResponseModel
    {
        public ListResponseModel()
        {
            List = new List<string>();
        }

        [DataMember(Name = "list")]
        public List<string> List { get; private set; }
    }
}