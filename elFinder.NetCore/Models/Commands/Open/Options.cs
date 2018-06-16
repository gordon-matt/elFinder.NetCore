using System.Collections.Generic;
using Newtonsoft.Json;

namespace elFinder.NetCore.Models.Commands
{
    public class Archive
    {
        private static string[] empty = new string[0];

        [JsonProperty("create")]
        public IEnumerable<string> Create => empty;

        [JsonProperty("extract")]
        public IEnumerable<string> Extract => empty;
    }

    public class Options
    {
        private static string[] disabled = new string[] { "archive", "callback", "chmod", "editor", "extract", "netmount", "ping", "search", "zipdl" };
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
        }

        [JsonProperty("archivers")]
        public Archive Archivers => emptyArchives;

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

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}