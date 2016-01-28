using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MiniWeb.Core
{
	public class SitePage
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

		public List<PageSection> Sections { get; set; }

		[IgnoreDataMember]
		public IEnumerable<SitePage> Pages { get; set; }

		[IgnoreDataMember]
		public SitePage Parent { get; set; }

		[IgnoreDataMember]
		public string UrlSuffix { get; set; }

		[IgnoreDataMember]
		public string BaseUrl
		{
			get
			{
				return Url.Split('/')[0];
			}
		}

		public SitePage()
		{
			Pages = new List<SitePage>();
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

	public class PageSection
	{
		public string Key { get; set; }
		public List<ContentItem> Items { get; set; }
	}

	public class ContentItem
	{
		[IgnoreDataMember]
		public SitePage Page { get; set; }
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

		public static ContentItem DefaultItem(string template)
		{
			return new ContentItem
			{
				Template = template
			};
		}
	}
}