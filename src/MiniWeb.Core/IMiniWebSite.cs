using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace MiniWeb.Core
{
    public interface IMiniWebSite
	{
		IHostingEnvironment HostingEnvironment { get; }
		ILogger Logger { get; }
		MiniWebConfiguration Configuration { get; }

		IMiniWebContentStorage ContentStorage { get; }
		IMiniWebAssetStorage AssetStorage { get; }

		IEnumerable<ISitePage> Pages { get; set; }
		IEnumerable<ISitePage> PageHierarchy { get; set; }
		IEnumerable<string> PageTemplates { get; }
		IEnumerable<string> ItemTemplates { get; }

		void DeleteSitePage(ISitePage page);
		FindResult GetPageByUrl(string url, bool editing = false);
		string GetPageUrl(ISitePage page);
		void SaveSitePage(ISitePage page, HttpRequest currentRequest, bool storeImages = false);
		void ReloadPages(bool forceReload = false);

		IContentItem DummyContent(string template);

		List<IPageSection> GetDefaultContentForTemplate(string template);
		IEnumerable<IAsset> Assets { get; set; }
		void DeleteAsset(IAsset asset);
		void ReloadAssets(bool forceReload = false);

		bool Authenticate(string username, string password);
		bool IsAuthenticated(ClaimsPrincipal user);
		ClaimsPrincipal GetClaimsPrincipal(string username);

	}
}