using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNet.Antiforgery;
using Microsoft.AspNet.Hosting;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.Logging;

namespace MiniWeb.Core
{
	public interface IMiniWebSite
	{
		IAntiforgery Antiforgery { get; }
		IApplicationEnvironment AppEnvironment { get; }
		IHostingEnvironment HostingEnvironment { get; }
		ILogger Logger { get; }

		IMiniWebStorage Storage { get; }
		MiniWebConfiguration Configuration { get; }

		IEnumerable<SitePage> Pages { get; set; }
		IEnumerable<SitePage> PageHierarchy { get; set; }
		IEnumerable<string> PageTemplates { get; }
		IEnumerable<string> ItemTemplates { get; }
		SitePage PageLogin { get; }
		SitePage Page404 { get; }

		void DeleteSitePage(SitePage page);
		SitePage GetPageByUrl(string url, bool editing = false);
		bool Authenticate(string user, string password);
		bool IsAuthenticated(ClaimsPrincipal user);
		void ReloadPages();
		void SaveSitePage(SitePage page, bool storeImages = false);
	}
}