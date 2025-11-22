namespace elFinder.NetCore.Models.Commands;

public class ChangedResponseModel
{
    [JsonPropertyName("changed")]
    public List<FileModel> Changed { get; private set; } = [];
}