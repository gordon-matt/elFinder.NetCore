using Newtonsoft.Json;

namespace elFinder.NetCore.Models.Commands
{
    public class DebugResponseModel
    {
        [JsonProperty("connector")]
        public string Connector => ".net";
    }
}