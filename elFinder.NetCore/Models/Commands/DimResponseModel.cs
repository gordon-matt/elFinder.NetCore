using System.Drawing;
using System.Text.Json.Serialization;

namespace elFinder.NetCore.Models.Commands
{
    public class DimResponseModel
    {
        public DimResponseModel(string dimension)
        {
            Dimension = dimension;
        }

        public DimResponseModel(Size size)
        {
            Dimension = string.Format("{0}x{1}", size.Width, size.Height);
        }

        [JsonPropertyName("dim")]
        public string Dimension { get; set; }
    }
}