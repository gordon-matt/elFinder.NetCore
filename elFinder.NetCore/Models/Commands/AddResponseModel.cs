using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace elFinder.NetCore.Models.Commands
{
    public class AddResponseModel
    {
        [JsonPropertyName("added")]
        public List<object> Added { get; protected set; }

        [JsonPropertyName("hashes")]
        public Dictionary<string, string> Hashes { get; protected set; }

        public AddResponseModel()
        {
            Added = new List<object>();
            Hashes = new Dictionary<string, string>();
        }
    }
}