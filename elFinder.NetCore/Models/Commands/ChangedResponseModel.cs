using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace elFinder.NetCore.Models.Commands
{
    public class ChangedResponseModel
    {
        public ChangedResponseModel()
        {
            Changed = new List<FileModel>();
        }

        [JsonPropertyName("changed")]
        public List<FileModel> Changed { get; private set; }
    }
}