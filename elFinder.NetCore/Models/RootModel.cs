using System.Runtime.Serialization;

namespace elFinder.NetCore.Models
{
    [DataContract]
    internal class RootModel : BaseModel
    {
        [DataMember(Name = "volumeId")]
        public string VolumeId { get; set; }

        [DataMember(Name = "dirs")]
        public byte Dirs { get; set; }
    }
}