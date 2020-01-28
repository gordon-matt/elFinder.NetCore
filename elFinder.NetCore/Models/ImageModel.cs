using System.Text.Json.Serialization;

namespace elFinder.NetCore.Models
{
    internal class ImageModel : FileModel
    {
        [JsonPropertyName("tmb")]
        public object Thumbnail { get; set; }

        [JsonPropertyName("dim")]
        public string Dimension { get; set; }
    }
}