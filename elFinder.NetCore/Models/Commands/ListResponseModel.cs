namespace elFinder.NetCore.Models.Commands;

public class ListResponseModel
{
    [JsonPropertyName("list")]
    public List<string> List { get; private set; } = [];
}