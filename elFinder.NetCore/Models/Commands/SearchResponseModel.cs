namespace elFinder.NetCore.Models.Commands;

public class SearchResponseModel
{
    [JsonPropertyName("files")]
    public List<object> Files { get; private set; } = [];
}