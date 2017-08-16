namespace MiniWeb.Core
{
	public class SitePageBasicPostModel
	{
		public string Layout { get; set; }
		public string MetaDescription { get; set; }
		public string MetaTitle { get; set; }
		public bool ShowInMenu { get; set; }
		public int SortOrder { get; set; }
		public string Template { get; set; }
		public string Title { get; set; }
		public string Url { get; set; }
		public bool Visible { get; set; }
	}
}