using System.Collections.Generic;
using System.Runtime.Serialization;
using elFinder.NetCore.Models;

namespace elFinder.NetCore.Models.Response
{
    [DataContract]
    internal class PutResponseModel
    {
        public PutResponseModel()
        {
            Changed = new List<FileModel>();
        }

        [DataMember(Name = "changed")]
        public List<FileModel> Changed { get; private set; }
    }
}