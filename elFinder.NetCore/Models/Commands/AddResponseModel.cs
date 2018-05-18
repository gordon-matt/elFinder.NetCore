using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace elFinder.NetCore.Models.Commands
{
    public class AddResponseModel
    {
        public AddResponseModel(FileInfo newFile, RootVolume root)
        {
            Added = new List<BaseModel>() { BaseModel.Create(newFile, root) };
        }

        public AddResponseModel(DirectoryInfo newDir, RootVolume root)
        {
            Added = new List<BaseModel>() { BaseModel.Create(newDir, root) };
        }

        public AddResponseModel()
        {
            Added = new List<BaseModel>();
        }

        [JsonProperty("added")]
        public List<BaseModel> Added { get; }
    }
}