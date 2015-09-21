using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Mvc;
using Microsoft.Framework.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc.ActionResults;
using System;

namespace MiniWeb.Core
{
	public class MiniWebApiController : Controller
	{
		private readonly IMiniWebSite _webSite;

		public MiniWebApiController(IMiniWebSite website)
		{
			_webSite = website;
		}

		public IActionResult DebugInfo()
		{
			return Content(_webSite.HostingEnvironment.EnvironmentName + " " + _webSite.AppEnvironment.ApplicationBasePath);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(MiniWebAuthentication.MiniWebCmsRoleValue)]
		public IActionResult SaveContent(string url, string items)
		{
			SitePage page = _webSite.Pages.FirstOrDefault(p => p.Url == url);
			if (page != null)
			{
				_webSite.Logger?.LogInformation($"save PAGE found {page.Url}");
				var newSections = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<PageSection>>(items);
				page.Sections.Clear();
				page.Sections.AddRange(newSections);

				_webSite.SaveSitePage(page, true);
				return new JsonResult(new { result = true });
			}
			return new JsonResult(new { result = false });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(MiniWebAuthentication.MiniWebCmsRoleValue)]
		public IActionResult SavePage(SitePage page)
		{
			//ignore move for now...
			if (Request.Form.ContainsKey("OldUrl") && (string)Request.Form["OldUrl"] != page.Url)
			{
				string message = $"Moving pages not allowed yet, tried to move {Request.Form["OldUrl"]} to new location: {page.Url}";
				_webSite.Logger?.LogError(message);
				return new JsonResult(new { result = false, message = message });
			}

			//keep sections only change page properties
			SitePage oldPage = _webSite.Pages.FirstOrDefault(p => p.Url == page.Url);
			if (oldPage != null)
			{
				page.Sections = oldPage.Sections;
			}
			else
			{
				page.Created = DateTime.Now;

				//TODO(RC): Add default content here maybe based on template?
				//page.Sections = new List<PageSection>() {
				//	new PageSection()
				//	{
				//		Key = "content",
				//		Items = new List<ContentItem>()
				//		{
				//			new ContentItem()
				//			{
				//				Template = "~/Views/Items/item.cshtml",
				//				Values = new Dictionary<string, string>()
				//			}
				//		}
				//	}
				//};
			}
			_webSite.SaveSitePage(page);
			return new JsonResult(new { result = true, url = page.Url });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(MiniWebAuthentication.MiniWebCmsRoleValue)]
		public IActionResult RemovePage(string url)
		{
			_webSite.Logger?.LogInformation($"remove {url}");
			SitePage page = _webSite.Pages.FirstOrDefault(p => p.Url == url);
			_webSite.DeleteSitePage(page);
			return new JsonResult(new { result = true, url = "/" + page.BaseUrl == page.Url ? _webSite.Configuration.DefaultPage : page.BaseUrl });
		}
	}
}