using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MiniWeb.Core;
using Newtonsoft.Json;

namespace MiniWeb.Storage.JsonStorage
{
	public class JsonSitePage : ISitePage
	{
		public string Url { get; set; }
		public string Title { get; set; }
		public string Template { get; set; }
		public string Layout { get; set; }
		public string MetaTitle { get; set; }
		public string MetaDescription { get; set; }

		public bool Visible { get; set; }
		public bool ShowInMenu { get; set; }
		public int SortOrder { get; set; }
		public DateTime Created { get; set; }
		public DateTime LastModified { get; set; }

		public List<IPageSection> Sections { get; set; }

		[IgnoreDataMember]
		public IEnumerable<ISitePage> Pages { get; set; }

		[IgnoreDataMember]
		public ISitePage Parent { get; set; }

		[IgnoreDataMember]
		public string BaseUrl
		{
			get
			{
				return Url.Split('/')[0];
			}
		}

		public JsonSitePage()
		{
			Pages = new List<ISitePage>();
			LastModified = DateTime.MinValue;
		}

		public string GetBodyCss()
		{
			var bodyCss = Url.Replace("/", "-");

			return bodyCss;
		}

		public bool VisibleInMenu()
		{
			return Visible && ShowInMenu;
		}
	}
	public class InterfaceConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return (objectType == typeof(IPageSection)) || (objectType == typeof(IContentItem));
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (objectType == typeof(IPageSection))
				return serializer.Deserialize<PageSection>(reader);
			if (objectType == typeof(IContentItem))
				return serializer.Deserialize<ContentItem>(reader);
			return null;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, value);
		}
	}

	public class PageSection : IPageSection
	{
		public string Key { get; set; }
		public List<IContentItem> Items { get; set; }
	}

	public class ContentItem : IContentItem
	{
		[IgnoreDataMember]
		public ISitePage Page { get; set; }
		public string Template { get; set; }
		public Dictionary<string, string> Values { get; set; } = new Dictionary<string, string>();

		public string GetValue(string value, string defaultvalue = "")
		{
			if (Values.ContainsKey(value))
			{
				return Values[value];
			}
			return defaultvalue;
		}

		public T Get<T>(string value, T defaultvalue = default(T))
		{
			try
			{
				return (T)Convert.ChangeType(GetValue(value, Convert.ToString(defaultvalue)), typeof(T));
			}
			catch
			{
				return default(T);
			}
		}
		
	}
}