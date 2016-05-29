using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;

namespace MiniWeb.Core
{
	public interface IMiniWebSite
	{
		IHostingEnvironment HostingEnvironment { get; }
		ILogger Logger { get; }
		MiniWebConfiguration Configuration { get; }

		IMiniWebContentStorage ContentStorage { get; }

		IEnumerable<SitePage> Pages { get; set; }
		IEnumerable<SitePage> PageHierarchy { get; set; }
		IEnumerable<string> PageTemplates { get; }
		IEnumerable<string> ItemTemplates { get; }
		SitePage PageLogin { get; }
		SitePage Page404 { get; }

		void DeleteSitePage(SitePage page);
		SitePage GetPageByUrl(string url, bool editing = false);
		string GetPageUrl(SitePage page);
		void SaveSitePage(SitePage page, bool storeImages = false);
		void ReloadPages();
		List<PageSection> GetDefaultContentForTemplate(string template);

		IEnumerable<Asset> Assets { get; set; }
		void DeleteAsset(Asset asset);
		void SaveAsset(Asset asset);
		void ReloadAssets();

		bool Authenticate(string user, string password);
		bool IsAuthenticated(ClaimsPrincipal user);

	}
}