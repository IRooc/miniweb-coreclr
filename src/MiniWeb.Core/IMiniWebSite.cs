using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace MiniWeb.Core
{
    public interface IMiniWebSite
	{
		IHostingEnvironment HostingEnvironment { get; }
		ILogger Logger { get; }
		MiniWebConfiguration Configuration { get; }

		IMiniWebContentStorage ContentStorage { get; }

		IEnumerable<ISitePage> Pages { get; set; }
		IEnumerable<ISitePage> PageHierarchy { get; set; }
		IEnumerable<string> PageTemplates { get; }
		IEnumerable<string> ItemTemplates { get; }

		void DeleteSitePage(ISitePage page);
		ISitePage GetPageByUrl(string url, bool editing = false);
		string GetPageUrl(ISitePage page);
		void SaveSitePage(ISitePage page, bool storeImages = false);
		void ReloadPages();

		IContentItem DummyContent(string template);

		List<IPageSection> GetDefaultContentForTemplate(string template);
		IEnumerable<IAsset> Assets { get; set; }
		void DeleteAsset(IAsset asset);
		void ReloadAssets();

		bool Authenticate(string user, string password);
		bool IsAuthenticated(ClaimsPrincipal user);

	}
}