using Newtonsoft.Json;

namespace elFinder.NetCore.Models
{
    internal class DirectoryModel : BaseModel
    {
        /// <summary>
        ///  Hash of parent directory. Required except roots dirs.
        /// </summary>
        [JsonProperty("phash")]
        public string ParentHash { get; set; }

        /// <summary>
        /// Is directory contains subfolders
        /// </summary>
        [JsonProperty("dirs")]
        public byte ContainsChildDirs { get; set; }
    }
}