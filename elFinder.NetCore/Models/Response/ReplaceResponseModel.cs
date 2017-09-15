using System.Collections.Generic;
using System.Runtime.Serialization;

namespace elFinder.NetCore.Models.Response
{
    [DataContract]
    internal class ReplaceResponseModel
    {
        public ReplaceResponseModel()
        {
            Added = new List<BaseModel>();
            Removed = new List<string>();
        }

        [DataMember(Name = "added")]
        public List<BaseModel> Added { get; private set; }

        [DataMember(Name = "removed")]
        public List<string> Removed { get; private set; }
    }
}