using System.Runtime.Serialization;

namespace elFinder.NetCore.Models.Commands
{
    
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