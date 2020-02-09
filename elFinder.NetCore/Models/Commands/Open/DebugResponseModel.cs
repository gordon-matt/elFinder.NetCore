using System.Text.Json.Serialization;

namespace elFinder.NetCore.Models.Commands
{
    public class DebugResponseModel
    {
        [JsonPropertyName("connector")]
        public string Connector => ".net";
    }
}