using System.Runtime.Serialization;

namespace elFinder.NetCore.Models.Commands
{
    [DataContract]
    public class OpenResponse : BaseOpenResponseModel
    {
        public OpenResponse(DirectoryModel currentWorkingDirectory, FullPath fullPath) : base(currentWorkingDirectory)
        {
            Options = new Options(fullPath);
            Files.Add(currentWorkingDirectory);
        }
    }
}