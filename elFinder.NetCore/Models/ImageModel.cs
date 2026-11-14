namespace elFinder.NetCore.Models;

internal class ImageModel : FileModel
{
    [JsonPropertyName("tmb")]
    public object Thumbnail { get; set; }

    [JsonPropertyName("dim")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Dimension { get; set; }
}