using System.Collections.Generic;
using Newtonsoft.Json;

namespace elFinder.NetCore.Models.Commands
{
    public class RemoveResponseModel
    {
        public RemoveResponseModel()
        {
            Removed = new List<string>();
        }

        [JsonProperty("removed")]
        public List<string> Removed { get; }
    }
}