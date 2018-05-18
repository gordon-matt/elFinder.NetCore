using System.Collections.Generic;
using Newtonsoft.Json;

namespace elFinder.NetCore.Models.Commands
{
    internal class Archive
    {
        private static string[] empty = new string[0];

        [JsonProperty("create")]
        public IEnumerable<string> Create => empty;

        [JsonProperty("extract")]
        public IEnumerable<string> Extract => empty;
    }

    internal class Options
    {
        private static string[] disabled = new string[] { "extract", "create" };
        private static string[] empty = new string[0];
        private static Archive emptyArchives = new Archive();

        public Options(FullPath fullPath)
        {
            Path = fullPath.Root.Alias;
            if (fullPath.RelativePath != string.Empty)
            {
                Path += Separator + fullPath.RelativePath.Replace('\\', Separator);
            }
            Url = fullPath.Root.Url ?? string.Empty;
            ThumbnailsUrl = fullPath.Root.ThumbnailsUrl ?? string.Empty;
            //ThumbnailsUrl = fullPath.Root.ThumbnailsUrl ?? fullPath.Root.Url + "/.tmb/";
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