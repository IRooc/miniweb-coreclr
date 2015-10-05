using System.Collections.Generic;
using MiniWeb.Core;

namespace MiniWeb.Storage.XmlStorage
{
	public class MiniWebXmlStorageConfig : IMiniWebStorageConfiguration
	{
		public string SitePageFolder { get; set; } = "/App_Data/SitePages/Xml/";
		public string SitePageVersionFolder { get; set; } = "/App_Data/SitePages/Xml/versions/";
		public Dictionary<string, string> Users { get; set; }
	}
}
