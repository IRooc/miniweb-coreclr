using System.Collections.Generic;

namespace MiniWeb.Core
{
	public interface IMiniWebContentStorage
	{
		//Set explicitly to avoid circular dependency injection
		IMiniWebSite MiniWebSite { get; set; }
		bool Authenticate(string username, string password);
		ISitePage GetSitePageByUrl(string url);
		void StoreSitePage(ISitePage sitePage);
		void DeleteSitePage(ISitePage sitePage);
		IEnumerable<ISitePage> AllPages();
		
		ISitePage MiniWebLoginPage { get; }
		ISitePage MiniWeb404Page { get; }

		List<IPageSection> GetDefaultSectionContent(DefaultContent defaultContent);
	}
}