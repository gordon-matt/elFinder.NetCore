using Newtonsoft.Json;

namespace elFinder.NetCore.Models
{
    internal class RootModel : BaseModel
    {
        //[JsonProperty("volumeId")]
        [JsonProperty("volumeid")] // https://github.com/Studio-42/elFinder/issues/2403
        public string VolumeId { get; set; }

        [JsonProperty("dirs")]
        public byte Dirs { get; set; }

		[JsonProperty("isroot")]
		public byte IsRoot => 1;
	}
}