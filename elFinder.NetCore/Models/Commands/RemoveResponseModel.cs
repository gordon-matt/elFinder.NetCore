namespace elFinder.NetCore.Models.Commands;

public class RemoveResponseModel
{
    [JsonPropertyName("removed")]
    public List<string> Removed { get; } = [];
}