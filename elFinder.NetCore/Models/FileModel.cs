namespace elFinder.NetCore.Models;

public class FileModel : BaseModel
{
    /// <summary>
    ///  Hash of parent directory. Required except roots dirs.
    /// </summary>
    [JsonPropertyName("phash")]
    public string ParentHash { get; set; }
}