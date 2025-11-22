namespace elFinder.NetCore.Models.Commands;

public class ThumbsResponseModel
{
    [JsonPropertyName("images")]
    public Dictionary<string, string> Images { get; } = [];
}