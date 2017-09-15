using System.DrawingCore;
using System.Runtime.Serialization;

namespace elFinder.NetCore.Models.Response
{
    [DataContract]
    internal class DimResponseModel
    {
        public DimResponseModel(string dimension)
        {
            Dimension = dimension;
        }

        public DimResponseModel(Size size)
        {
            Dimension = string.Format("{0}x{1}", size.Width, size.Height);
        }

        [DataMember(Name = "dim")]
        public string Dimension { get; set; }
    }
}