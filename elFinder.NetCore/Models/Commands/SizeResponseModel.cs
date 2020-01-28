using System.Text.Json.Serialization;

namespace elFinder.NetCore.Models.Commands
{
    public class SizeResponseModel
    {
        [JsonPropertyName("size")]
        public long Size { get; set; }

        [JsonPropertyName("fileCnt")]
        public int FileCount { get; set; }

        [JsonPropertyName("dirCnt")]
        public int DirectoryCount { get; set; }
    }
}