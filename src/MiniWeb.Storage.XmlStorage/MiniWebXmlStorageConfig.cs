using System.Collections.Generic;

namespace MiniWeb.Storage.XmlStorage
{
    public class MiniWebXmlStorageConfig 
	{
		public string SitePageFolder { get; set; } = "/App_Data/SitePages/Xml/";
		public string SitePageVersionFolder { get; set; } = "/App_Data/SitePages/Xml/versions/";
		public Dictionary<string, string> Users { get; set; }
				
		public string Layout { get; set; } = "~/Views/_layout.cshtml";
		public string LoginView { get; set; } = "~/Views/login.cshtml";

		public string PageTemplatePath { get; set; } = "/Views/Pages";
		public string ItemTemplatePath { get; set; } = "/Views/Items";
	}
}
