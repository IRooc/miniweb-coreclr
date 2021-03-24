using System;
using MiniWeb.Core;
using Newtonsoft.Json;

namespace MiniWeb.Storage.JsonStorage
{
    public class JsonInterfaceConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return (objectType == typeof(IPageSection)) || (objectType == typeof(IContentItem)) || (objectType == typeof(ISitePage));
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (objectType == typeof(ISitePage))
				return serializer.Deserialize<JsonSitePage>(reader);
			if (objectType == typeof(IPageSection))
				return serializer.Deserialize<JsonPageSection>(reader);
			if (objectType == typeof(IContentItem))
				return serializer.Deserialize<JsonContentItem>(reader);
			return null;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, value);
		}
	}
}
