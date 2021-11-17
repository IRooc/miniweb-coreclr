using System.Collections.Generic;

namespace MiniWeb.Storage.JsonStorage
{
    public class MiniWebJsonStorageConfig
	{
		public string SitePageFolder { get; set; } = "/App_Data/SitePages/Json/";
		public string SitePageVersionFolder { get; set; } = "/App_Data/SitePages/Json/versions/";
				
		public string Layout { get; set; } = "~/Views/_layout.cshtml";
		public string LoginView { get; set; } = "~/Views/login.cshtml";

		public string PageTemplatePath { get; set; } = "/Views/Pages";
		public string ItemTemplatePath { get; set; } = "/Views/Items";

		public string Username {get;set;} 

		public string Password {get;set;}
	}
}
