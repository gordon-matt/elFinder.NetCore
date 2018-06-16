using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace elFinder.NetCore.Serialization
{
    public class KeyValuePairConverter : JsonConverter
    {
        private readonly Type[] types;

        public KeyValuePairConverter()
        {
        }

        public KeyValuePairConverter(params Type[] types)
        {
            this.types = types;
        }

        public override bool CanRead => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(List<KeyValuePair<string, string>>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        public override void WriteJson(JsonWriter writer, object list, JsonSerializer serializer)
        {
            var obj = new JObject();
            foreach (var pair in list as List<KeyValuePair<string, string>>)
            {
                obj.Add(pair.Key, pair.Value);
            }
            obj.WriteTo(writer);
        }
    }
}