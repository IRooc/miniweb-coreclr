using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

namespace MiniWeb.Core
{
	public interface IMiniWebContentStorage
	{
		//Set explicitly to avoid circular dependency injection
		IMiniWebSite MiniWebSite { get; set; }
		bool Authenticate(string username, string password);
		ISitePage GetSitePageByUrl(string url);
		void StoreSitePage(ISitePage sitePage, HttpRequest currentRequest);
		void DeleteSitePage(ISitePage sitePage);
		IEnumerable<ISitePage> AllPages();
		
		ISitePage MiniWebLoginPage { get; }
		ISitePage MiniWeb404Page { get; }

		List<IPageSection> GetDefaultSectionContent(DefaultContent defaultContent);

		//Used to deserialize the Posted JSON to concrete classes.
		JsonConverter JsonInterfaceConverter { get; }

		ISitePage NewPage();
	}
}