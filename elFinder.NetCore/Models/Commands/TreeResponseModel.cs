using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace elFinder.NetCore.Models.Commands
{
    public class TreeResponseModel
    {
        public TreeResponseModel()
        {
            Tree = new List<object>();
        }

        [JsonPropertyName("tree")]
        public List<object> Tree { get; private set; }
    }
}