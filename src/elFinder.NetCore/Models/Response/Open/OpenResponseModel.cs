using System.Runtime.Serialization;
using elFinder.NetCore.Models;

namespace elFinder.NetCore.Models.Response
{
    [DataContract]
    internal class OpenResponse : BaseOpenResponseModel
    {
        public OpenResponse(BaseModel currentWorkingDirectory, FullPath fullPath)
            : base(currentWorkingDirectory)
        {
            Options = new Options(fullPath);
            files.Add(currentWorkingDirectory);
        }
    }
}