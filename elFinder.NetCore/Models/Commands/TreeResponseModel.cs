using System.Collections.Generic;
using Newtonsoft.Json;

namespace elFinder.NetCore.Models.Commands
{
    internal class TreeResponseModel
    {
        public TreeResponseModel()
        {
            Tree = new List<BaseModel>();
        }

        [JsonProperty("tree")]
        public List<BaseModel> Tree { get; private set; }
    }
}