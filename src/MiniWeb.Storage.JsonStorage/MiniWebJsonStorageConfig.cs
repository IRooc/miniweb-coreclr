using System.Collections.Generic;
using MiniWeb.Core;

namespace MiniWeb.Storage.JsonStorage
{
	public class MiniWebJsonStorageConfig : IMiniWebStorageConfiguration
	{
		public string SitePageFolder { get; set; } = "/App_Data/SitePages/Json/";
		public string SitePageVersionFolder { get; set; } = "/App_Data/SitePages/Json/versions/";
		public Dictionary<string, string> Users { get; set; }
	}
}
