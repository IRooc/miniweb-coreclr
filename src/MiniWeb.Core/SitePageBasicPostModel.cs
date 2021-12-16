using System;
using System.Collections.Generic;

namespace MiniWeb.Core
{
	public class SitePageBasicPostModel
	{
		public string Layout { get; set; }
		public string MetaDescription { get; set; }
		public string MetaTitle { get; set; }
		public DateTime Date { get; set; }
		public bool ShowInMenu { get; set; }
		public int SortOrder { get; set; }
		public string Template { get; set; }
		public string Title { get; set; }
		public string Url { get; set; }
		public string RedirectUrl { get; set; }
		public bool Visible { get; set; }
		public bool? NewPage { get; set; }
	}


	public class SitePageSectionPostModel
	{
		public List<SitePageContentPostModel> Items { get;set; }
		public string Key { get; set; }
	}
	public class SitePageContentPostModel 
	{
		public string Template { get; set; }
		public Dictionary<string, string> Values { get; set; }
	}
}