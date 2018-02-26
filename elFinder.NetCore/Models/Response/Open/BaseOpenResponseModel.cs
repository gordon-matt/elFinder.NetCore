using System.Collections.Generic;
using System.Runtime.Serialization;

namespace elFinder.NetCore.Models.Response
{
    [DataContract]
    internal class BaseOpenResponseModel
    {
        protected List<BaseModel> files;
        private static DebugResponseModel debug = new DebugResponseModel();
        private BaseModel currentWorkingDirectory;

        public BaseOpenResponseModel(BaseModel currentWorkingDirectory)
        {
            files = new List<BaseModel>();
            this.currentWorkingDirectory = currentWorkingDirectory;
        }

        [DataMember(Name = "cwd")]
        public BaseModel CurrentWorkingDirectory
        {
            get { return currentWorkingDirectory; }
        }

        [DataMember(Name = "debug")]
        public DebugResponseModel Debug => debug;

        [DataMember(Name = "files")]
        public List<BaseModel> Files => files;

        [DataMember(Name = "options")]
        public Options Options { get; protected set; }
    }
}