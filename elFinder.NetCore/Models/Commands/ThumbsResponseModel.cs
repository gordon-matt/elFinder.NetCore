using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace elFinder.NetCore.Models.Commands
{
    public class ThumbsResponseModel
    {
        public ThumbsResponseModel()
        {
            Images = new Dictionary<string, string>();
        }

        [JsonPropertyName("images")]
        public Dictionary<string, string> Images { get; }
    }
}