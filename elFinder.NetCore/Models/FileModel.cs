using System.Runtime.Serialization;

namespace elFinder.NetCore.Models
{
    [DataContract]
    internal class FileModel : BaseModel
    {
        /// <summary>
        ///  Hash of parent directory. Required except roots dirs.
        /// </summary>
        [DataMember(Name = "phash")]
        public string ParentHash { get; set; }
    }
}