using System.Drawing;
using Newtonsoft.Json;

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

        [JsonProperty("dim")]
        public string Dimension { get; set; }
    }
}