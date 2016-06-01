using System;
using System.Collections.Generic;

namespace MiniWeb.Core
{
	public interface ISitePage
	{
		string BaseUrl { get; }
		DateTime Created { get; set; }
		DateTime LastModified { get; set; }
		string Layout { get; set; }
		string MetaDescription { get; set; }
		string MetaTitle { get; set; }
		IEnumerable<ISitePage> Pages { get; set; }
		ISitePage Parent { get; set; }
		List<IPageSection> Sections { get; set; }
		bool ShowInMenu { get; set; }
		int SortOrder { get; set; }
		string Template { get; set; }
		string Title { get; set; }
		string Url { get; set; }
		bool Visible { get; set; }

		string GetBodyCss();
		bool VisibleInMenu();
	}
	
	public interface IPageSection
	{
		List<IContentItem> Items { get; set; }
		string Key { get; set; }
	}
	
	public interface IContentItem
	{
		ISitePage Page { get; set; }
		string Template { get; set; }
		Dictionary<string, string> Values { get; set; }

		T Get<T>(string value, T defaultvalue = default(T));
		string GetValue(string value, string defaultvalue = "");
	}
}