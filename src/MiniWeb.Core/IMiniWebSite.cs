using System.Collections.Generic;
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
		IEnumerable<string> PageTemplates { get; }
		IEnumerable<string> ItemTemplates { get; }
		string GetPageUrl(ISitePage page);
		Task<FindResult> GetPageByUrl(string url, ClaimsPrincipal user);
		Task<IEnumerable<ISitePage>> Pages(bool reload = false);
		Task DeleteSitePage(ISitePage page);
		Task SaveSitePage(ISitePage page, HttpRequest currentRequest, bool storeImages = false);
		IContentItem DummyContent(string template);
		Task<List<IPageSection>> GetDefaultContentForTemplate(string template);		
		Task<IEnumerable<IAsset>> Assets(bool reload = false);
		Task DeleteAsset(IAsset asset);
		Task<bool> Authenticate(string username, string password);
		ClaimsPrincipal GetClaimsPrincipal(string username);
		bool IsAuthenticated(ClaimsPrincipal user);
	}
}