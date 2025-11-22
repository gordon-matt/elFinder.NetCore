namespace elFinder.NetCore.Models.Commands;

public class TreeResponseModel
{
    [JsonPropertyName("tree")]
    public List<object> Tree { get; private set; } = [];
}