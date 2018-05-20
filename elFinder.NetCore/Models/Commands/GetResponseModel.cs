using Newtonsoft.Json;

namespace elFinder.NetCore.Models.Commands
{
    public class GetResponseModel
    {
        [JsonProperty("content")]
        public string Content { get; set; }
    }
}