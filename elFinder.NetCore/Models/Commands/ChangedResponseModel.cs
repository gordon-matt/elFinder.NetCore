using System.Collections.Generic;
using Newtonsoft.Json;

namespace elFinder.NetCore.Models.Commands
{
    public class ChangedResponseModel
    {
        public ChangedResponseModel()
        {
            Changed = new List<FileModel>();
        }

        [JsonProperty("changed")]
        public List<FileModel> Changed { get; private set; }
    }
}