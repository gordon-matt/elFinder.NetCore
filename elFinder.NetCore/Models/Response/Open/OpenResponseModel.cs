using System.Runtime.Serialization;

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