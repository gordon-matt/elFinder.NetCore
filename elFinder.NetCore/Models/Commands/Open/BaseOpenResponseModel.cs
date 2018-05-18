using System.Collections.Generic;
using Newtonsoft.Json;

namespace elFinder.NetCore.Models.Commands
{
    internal class BaseOpenResponseModel
    {
        protected List<BaseModel> files;
        private static DebugResponseModel debug = new DebugResponseModel();

        public BaseOpenResponseModel(BaseModel currentWorkingDirectory)
        {
            files = new List<BaseModel>();
            this.CurrentWorkingDirectory = currentWorkingDirectory;
        }

        [JsonProperty("cwd")]
        public BaseModel CurrentWorkingDirectory { get; }

        [JsonProperty("debug")]
        public DebugResponseModel Debug => debug;

        [JsonProperty("files")]
        public List<BaseModel> Files => files;

        [JsonProperty("options")]
        public Options Options { get; protected set; }
    }
}