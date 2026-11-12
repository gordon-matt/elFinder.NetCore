using elFinder.NetCore.Drawing;

namespace elFinder.NetCore.Models;

public class FileModel : BaseModel
{
    /// <summary>
    ///  Hash of parent directory. Required except roots dirs.
    /// </summary>
    [JsonPropertyName("phash")]
    public string ParentHash { get; set; }

    public static implicit operator FileModel(ImageWithMimeType v) => throw new NotImplementedException();
}