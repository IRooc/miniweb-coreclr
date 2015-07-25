using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
