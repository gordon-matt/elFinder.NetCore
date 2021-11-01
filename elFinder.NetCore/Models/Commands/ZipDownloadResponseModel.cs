using System.Text.Json.Serialization;

namespace elFinder.NetCore.Models.Commands
{
    public class ZipDownloadResponseModel
    {
        [JsonPropertyName("zipdl")]
        public ZipDownloadData ZipDownload { get; set; }

        public class ZipDownloadData
        {
            [JsonPropertyName("file")]
            public string File { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("mime")]
            public string Mime { get; set; }
        }
    }
}