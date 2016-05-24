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

		IMiniWebContentStorage Storage { get; }
		MiniWebConfiguration Configuration { get; }

		IEnumerable<SitePage> Pages { get; set; }
		IEnumerable<SitePage> PageHierarchy { get; set; }
		IEnumerable<string> PageTemplates { get; }
		IEnumerable<string> ItemTemplates { get; }
		SitePage PageLogin { get; }
		SitePage Page404 { get; }

		void DeleteSitePage(SitePage page);
		SitePage GetPageByUrl(string url, bool editing = false);
		string GetPageUrl(SitePage page);
		bool Authenticate(string user, string password);
		bool IsAuthenticated(ClaimsPrincipal user);
		void ReloadPages();
		void SaveSitePage(SitePage page, bool storeImages = false);
		List<PageSection> GetDefaultContentForTemplate(string template);

		bool ShowSubMenuForPage(SitePage page);
	}
}