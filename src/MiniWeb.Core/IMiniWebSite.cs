﻿using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace MiniWeb.Core
{
    public interface IMiniWebSite
	{
		IWebHostEnvironment HostingEnvironment { get; }
		ILogger Logger { get; }
		MiniWebConfiguration Configuration { get; }

		IMiniWebContentStorage ContentStorage { get; }
		IMiniWebAssetStorage AssetStorage { get; }

		IEnumerable<ISitePage> Pages { get; set; }
		IEnumerable<ISitePage> PageHierarchy { get; set; }
		IEnumerable<string> PageTemplates { get; }
		IEnumerable<string> ItemTemplates { get; }

		Task DeleteSitePage(ISitePage page);
		Task<FindResult> GetPageByUrl(string url, ClaimsPrincipal user);
		string GetPageUrl(ISitePage page);
		Task SaveSitePage(ISitePage page, HttpRequest currentRequest, bool storeImages = false);
		Task ReloadPages(bool forceReload = false);

		IContentItem DummyContent(string template);

		Task<List<IPageSection>> GetDefaultContentForTemplate(string template);
		IEnumerable<IAsset> Assets { get; set; }
		Task DeleteAsset(IAsset asset);
		Task ReloadAssets(bool forceReload = false);

		Task<bool> Authenticate(string username, string password);
		bool IsAuthenticated(ClaimsPrincipal user);
		ClaimsPrincipal GetClaimsPrincipal(string username);

	}
}