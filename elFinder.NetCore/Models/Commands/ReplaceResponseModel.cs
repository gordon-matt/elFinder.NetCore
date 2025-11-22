namespace elFinder.NetCore.Models.Commands;

public class ReplaceResponseModel
{
    [JsonPropertyName("added")]
    public List<object> Added { get; private set; } = [];

    [JsonPropertyName("removed")]
    public List<string> Removed { get; private set; } = [];
}