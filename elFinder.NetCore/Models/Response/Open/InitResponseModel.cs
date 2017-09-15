using System.Collections.Generic;
using System.Runtime.Serialization;

namespace elFinder.NetCore.Models.Response
{
    [DataContract]
    internal class InitResponseModel : BaseOpenResponseModel
    {
        private static string[] empty = new string[0];

        public InitResponseModel(BaseModel currentWorkingDirectory, Options options)
            : base(currentWorkingDirectory)
        {
            Options = options;
        }

        [DataMember(Name = "api")]
        public string Api
        {
            get { return "2.0"; }
        }

        [DataMember(Name = "netDrivers")]
        public IEnumerable<string> NetDrivers
        {
            get { return empty; }
        }

        [DataMember(Name = "uplMaxSize")]
        public string UploadMaxSize { get; set; }
    }
}