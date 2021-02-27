using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace elFinder.NetCore.Models.Commands
{
    public class SearchResponseModel
    {
        public SearchResponseModel()
        {
            Files = new List<object>();
        }

        [JsonPropertyName("files")]
        public List<object> Files { get; private set; }
    }
}