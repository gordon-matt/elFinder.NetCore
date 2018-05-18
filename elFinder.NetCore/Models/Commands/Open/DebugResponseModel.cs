using Newtonsoft.Json;

namespace elFinder.NetCore.Models.Commands
{
    internal class DebugResponseModel
    {
        [JsonProperty("connector")]
        public string Connector => ".net";
    }
}