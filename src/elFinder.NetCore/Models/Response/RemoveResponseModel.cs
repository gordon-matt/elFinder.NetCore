using System.Collections.Generic;
using System.Runtime.Serialization;

namespace elFinder.NetCore.Models.Response
{
    [DataContract]
    internal class RemoveResponseModel
    {
        private List<string> removed;

        public RemoveResponseModel()
        {
            removed = new List<string>();
        }

        [DataMember(Name = "removed")]
        public List<string> Removed
        {
            get { return removed; }
        }
    }
}