using Newtonsoft.Json;

namespace elFinder.NetCore.Models
{
    public class FileModel : BaseModel
    {
        /// <summary>
        ///  Hash of parent directory. Required except roots dirs.
        /// </summary>
        [JsonProperty("phash")]
        public string ParentHash { get; set; }
    }
}