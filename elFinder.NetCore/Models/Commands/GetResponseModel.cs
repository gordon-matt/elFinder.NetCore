using Newtonsoft.Json;

namespace elFinder.NetCore.Models.Commands
{
    internal class GetResponseModel
    {
        [JsonProperty("content")]
        public string Content { get; set; }
    }
}