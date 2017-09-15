using System.Runtime.Serialization;

namespace elFinder.NetCore.Models.Response
{
    [DataContract]
    internal class DebugResponseModel
    {
        [DataMember(Name = "connector")]
        public string Connector
        {
            get { return ".net"; }
        }
    }
}