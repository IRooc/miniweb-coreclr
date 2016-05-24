using System.Collections.Generic;

namespace MiniWeb.Core
{
	public interface IMiniWebContentStorage
	{
		//Set explicitly to avoid circular dependency injection
		IMiniWebSite MiniWebSite { get; set; }
		bool Authenticate(string username, string password);
		SitePage GetSitePageByUrl(string url);
		void StoreSitePage(SitePage sitePage);
		void DeleteSitePage(SitePage sitePage);
		IEnumerable<SitePage> AllPages();
	}
}