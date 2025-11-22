namespace elFinder.NetCore.Models.Commands;

public class PutResponseModel
{
    [JsonPropertyName("changed")]
    public List<FileModel> Changed { get; private set; } = [];
}