using System.Collections.Generic;
using Newtonsoft.Json;

namespace elFinder.NetCore.Models.Commands
{
    public class ListResponseModel
    {
        public ListResponseModel()
        {
            List = new List<string>();
        }

        [JsonProperty("list")]
        public List<string> List { get; private set; }
    }
}