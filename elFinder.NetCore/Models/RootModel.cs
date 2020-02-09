using System.Text.Json.Serialization;

namespace elFinder.NetCore.Models
{
    public class RootModel : DirectoryModel
    {
        [JsonPropertyName("isroot")]
        public byte IsRoot => 1;
    }
}