using System.Collections.Generic;

namespace MiniWeb.Core
{
	public interface IMiniWebStorage
	{
		//Set explicitly to avoid circular dependency injection
		IMiniWebSite MiniWebSite { get; set; }
		bool Authenticate(string username, string password);
		SitePage GetSitePageByUrl(string url);
		void StoreSitePage(SitePage sitePage);
		void DeleteSitePage(SitePage sitePage);
		IEnumerable<SitePage> AllPages();
	}
	public interface IMiniWebStorageConfiguration
	{

	}
}