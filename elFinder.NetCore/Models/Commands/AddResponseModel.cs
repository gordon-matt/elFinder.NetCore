using System.Collections.Generic;
using elFinder.NetCore.Serialization;
using Newtonsoft.Json;

namespace elFinder.NetCore.Models.Commands
{
	public class AddResponseModel
	{
		[JsonProperty("added")]
		public List<BaseModel> Added { get; protected set; }

		[JsonProperty("hashes")]
		[JsonConverter(typeof(KeyValuePairConverter))]
		public List<KeyValuePair<string, string>> Hashes { get; protected set; }

		public AddResponseModel()
		{
			Added = new List<BaseModel>();
			Hashes = new List<KeyValuePair<string, string>>();
		}
	}
}
