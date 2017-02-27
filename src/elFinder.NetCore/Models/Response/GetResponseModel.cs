using System.Runtime.Serialization;

namespace elFinder.NetCore.Models.Response
{
    [DataContract]
    internal class GetResponseModel
    {
        [DataMember(Name = "content")]
        public string Content { get; set; }
    }
}