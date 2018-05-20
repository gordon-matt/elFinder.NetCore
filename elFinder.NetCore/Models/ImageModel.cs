using Newtonsoft.Json;

namespace elFinder.NetCore.Models
{
    internal class ImageModel : FileModel
    {
        [JsonProperty("tmb")]
        public object Thumbnail { get; set; }

        [JsonProperty("dim")]
        public string Dimension { get; set; }
    }
}