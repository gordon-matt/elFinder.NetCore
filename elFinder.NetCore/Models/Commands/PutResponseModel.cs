using System.Collections.Generic;
using Newtonsoft.Json;

namespace elFinder.NetCore.Models.Commands
{
    internal class PutResponseModel
    {
        public PutResponseModel()
        {
            Changed = new List<FileModel>();
        }

        [JsonProperty("changed")]
        public List<FileModel> Changed { get; private set; }
    }
}