using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MiniWeb.Core
{
	public interface IMiniWebContentStorage
	{
		//Set explicitly to avoid circular dependency injection
		IMiniWebSite MiniWebSite { get; set; }
		Task<bool> Authenticate(string username, string password);
		Task<ISitePage> GetSitePageByUrl(string url);
		Task StoreSitePage(ISitePage sitePage, HttpRequest currentRequest);
		Task DeleteSitePage(ISitePage sitePage);
		Task<IEnumerable<ISitePage>> AllPages();

		Task<ISitePage> Deserialize(string filecontent);

		Task<List<IPageSection>> GetDefaultSectionContent(DefaultContent defaultContent);
		Task<IPageSection> GetPageSection(SitePageSectionPostModel section);


		Task<ISitePage> MiniWebLoginPage();
		Task<ISitePage> MiniWeb404Page();
		Task<ISitePage> NewPage();
	}
}