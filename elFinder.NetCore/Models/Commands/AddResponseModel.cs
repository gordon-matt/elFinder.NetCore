namespace elFinder.NetCore.Models.Commands;

public class AddResponseModel
{
    [JsonPropertyName("added")]
    public List<object> Added { get; protected set; } = [];

    [JsonPropertyName("hashes")]
    public Dictionary<string, string> Hashes { get; protected set; } = [];

    [JsonPropertyName("warning")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string> Warning { get; set; }
}