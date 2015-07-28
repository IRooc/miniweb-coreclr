namespace MiniWeb.Core
{
	public class MiniWebConfiguration
	{
		public string AuthenticationScheme = "MiniWebCms";
		public string MiniWebCmsRoleValue = "MiniWebCmsRole";

		public string Title { get; set; } = "MiniWeb by Rooc";
		public string DefaultPage { get; set; } = "/home";

		public string Layout { get; set; } = "~/Views/_layout.cshtml";
		public string LoginView { get; set; } = "~/Views/login.cshtml";

		public string PageTemplatePath { get; set; } = "/Views/Pages";
		public string ItemTemplatePath { get; set; } = "/Views/Items";
		public string ImageSavePath { get; set; } = "~/images/saved/";
		public string LoginPath { get; set; } = "/miniweb/login";
		public string LogoutPath { get; set; } = "/miniweb/logout";

		public bool RedirectToFirstSub { get; set; } = false;
		public bool StoreVersions { get; set; } = false;
	}
}