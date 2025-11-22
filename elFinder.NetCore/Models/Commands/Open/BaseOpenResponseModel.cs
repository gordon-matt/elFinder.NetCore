namespace elFinder.NetCore.Models.Commands;

public class BaseOpenResponseModel(DirectoryModel currentWorkingDirectory)
{
    private static readonly DebugResponseModel debug = new();

    [JsonPropertyName("cwd")]
    public DirectoryModel CurrentWorkingDirectory { get; protected set; } = currentWorkingDirectory;

    [JsonPropertyName("debug")]
    public DebugResponseModel Debug => debug;

    [JsonPropertyName("files")]
    public List<object> Files { get; protected set; } = [];

    [JsonPropertyName("options")]
    public Options Options { get; protected set; }
}