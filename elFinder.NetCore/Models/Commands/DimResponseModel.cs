using System.Drawing;

namespace elFinder.NetCore.Models.Commands;

public class DimResponseModel
{
    public DimResponseModel(string dimension)
    {
        Dimension = dimension;
    }

    public DimResponseModel(Size size)
    {
        Dimension = $"{size.Width}x{size.Height}";
    }

    [JsonPropertyName("dim")]
    public string Dimension { get; set; }
}
