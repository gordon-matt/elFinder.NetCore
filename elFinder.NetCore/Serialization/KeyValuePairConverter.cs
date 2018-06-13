using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace elFinder.NetCore.Serialization
{
	public class KeyValuePairConverter : JsonConverter
	{
		private readonly Type[] _types;

		public KeyValuePairConverter() { }

		public KeyValuePairConverter(params Type[] types)
		{
			_types = types;
		}

		public override void WriteJson(JsonWriter writer, object list, JsonSerializer serializer)
		{
			var o = new JObject();
			foreach (var kvp in list as List<KeyValuePair<string, string>>)
			{
				o.Add(kvp.Key, kvp.Value);
			}
			o.WriteTo(writer);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
		}

		public override bool CanRead
		{
			get { return false; }
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(List<KeyValuePair<string, string>>);
		}
	}
}
