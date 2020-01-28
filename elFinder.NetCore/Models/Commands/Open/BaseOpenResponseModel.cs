using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace elFinder.NetCore.Models.Commands
{
    public class BaseOpenResponseModel
    {
        private static DebugResponseModel debug = new DebugResponseModel();

        public BaseOpenResponseModel(DirectoryModel currentWorkingDirectory)
        {
            Files = new List<object>();
            CurrentWorkingDirectory = currentWorkingDirectory;
        }

        [JsonPropertyName("cwd")]
        public DirectoryModel CurrentWorkingDirectory { get; protected set; }

        [JsonPropertyName("debug")]
        public DebugResponseModel Debug => debug;

        [JsonPropertyName("files")]
        public List<object> Files { get; protected set; }

        [JsonPropertyName("options")]
        public Options Options { get; protected set; }
    }
}