using System.Collections.Generic;
using Newtonsoft.Json;

namespace elFinder.NetCore.Models.Commands
{
    public class Archive
    {
        [JsonProperty("create")]
        public IEnumerable<string> Create { get; set; }

        [JsonProperty("extract")]
        public IEnumerable<string> Extract { get; set; }

        [JsonProperty("createext")]
        public IDictionary<string, string> CreateExt { get; set; }
    }

    public class Options
    {
        private static string[] disabled = new string[] { "callback", "chmod", "editor", "netmount", "ping", "search", "zipdl" };
        private static string[] empty = new string[0];
        private static Archive emptyArchives = new Archive();

        public Options(FullPath fullPath)
        {
            Path = fullPath.RootVolume.Alias;
            if (fullPath.RelativePath != string.Empty)
            {
                Path += Separator + fullPath.RelativePath.Replace('\\', Separator);
            }
            Url = fullPath.RootVolume.Url ?? string.Empty;
            ThumbnailsUrl = fullPath.RootVolume.ThumbnailUrl ?? string.Empty;
            //ThumbnailsUrl = fullPath.Root.ThumbnailUrl ?? fullPath.Root.Url + "/.tmb/";
            Archivers = new Archive
            {
                Create = new[] { "application/zip" },
                Extract = new[] { "application/zip" },
                CreateExt = new Dictionary<string, string>
                {
                    {"application/zip" ,"zip"}
                }
            };
        }

        [JsonProperty("archivers")]
        public Archive Archivers { get; set; }

        [JsonProperty("disabled")]
        public IEnumerable<string> Disabled => disabled;

        [JsonProperty("copyOverwrite")]
        public byte IsCopyOverwrite => 1;

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("separator")]
        public char Separator => '/';

        [JsonProperty("tmbUrl")]
        public string ThumbnailsUrl { get; set; }

        [JsonProperty("trashHash")]
        public string TrashHash => string.Empty;

        [JsonProperty("uploadMaxConn")]
        public int UploadMaxConnections => -1;

        [JsonProperty("uploadMaxSize")]
        public string UploadMaxSize { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}