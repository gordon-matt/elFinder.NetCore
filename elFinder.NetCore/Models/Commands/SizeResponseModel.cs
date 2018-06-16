using Newtonsoft.Json;

namespace elFinder.NetCore.Models.Commands
{
    public class SizeResponseModel
    {
        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("fileCnt")]
        public int FileCount { get; set; }

        [JsonProperty("dirCnt")]
        public int DirectoryCount { get; set; }
    }
}