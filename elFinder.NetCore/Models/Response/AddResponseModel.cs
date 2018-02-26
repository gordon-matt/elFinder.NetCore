using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace elFinder.NetCore.Models.Response
{
    [DataContract]
    internal class AddResponseModel
    {
        private List<BaseModel> added;

        public AddResponseModel(FileInfo newFile, Root root)
        {
            added = new List<BaseModel>() { BaseModel.Create(newFile, root) };
        }

        public AddResponseModel(DirectoryInfo newDir, Root root)
        {
            added = new List<BaseModel>() { BaseModel.Create(newDir, root) };
        }

        public AddResponseModel()
        {
            added = new List<BaseModel>();
        }

        [DataMember(Name = "added")]
        public List<BaseModel> Added => added;
    }
}