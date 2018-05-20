using System.Collections.Generic;
using Newtonsoft.Json;

namespace elFinder.NetCore.Models.Commands
{
    public class BaseOpenResponseModel
    {
        private static DebugResponseModel debug = new DebugResponseModel();

        public BaseOpenResponseModel(BaseModel currentWorkingDirectory)
        {
            Files = new List<BaseModel>();
            CurrentWorkingDirectory = currentWorkingDirectory;
        }

        [JsonProperty("cwd")]
        public BaseModel CurrentWorkingDirectory { get; protected set; }

        [JsonProperty("debug")]
        public DebugResponseModel Debug => debug;

        [JsonProperty("files")]
        public List<BaseModel> Files { get; protected set; }

        [JsonProperty("options")]
        public Options Options { get; protected set; }
    }
}