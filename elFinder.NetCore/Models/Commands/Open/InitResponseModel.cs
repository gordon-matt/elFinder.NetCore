namespace elFinder.NetCore.Models.Commands;

public class InitResponseModel : BaseOpenResponseModel
{
    private static readonly string[] empty = [];

    public InitResponseModel(DirectoryModel currentWorkingDirectory, Options options)
        : base(currentWorkingDirectory)
    {
        Options = options;
    }

    [JsonPropertyName("api")]
    public string Api => "2.1049";

    [JsonPropertyName("netDrivers")]
    public IEnumerable<string> NetDrivers => empty;
}