using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MiniWeb.Core
{
	[Authorize(MiniWebAuthentication.MiniWebCmsRoleValue)]
	public class MiniWebApiController : Controller
	{
		private readonly IMiniWebSite _webSite;

		public MiniWebApiController(IMiniWebSite website)
		{
			_webSite = website;
		}

		[AllowAnonymous]
		public IActionResult DebugInfo()
		{
			return Content(_webSite.HostingEnvironment.EnvironmentName + " " + _webSite.HostingEnvironment.ContentRootPath);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
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
				//new page
				page.Created = DateTime.Now;
				page.Sections = _webSite.GetDefaultContentForTemplate(page.Template);
			}
			_webSite.SaveSitePage(page);
			return new JsonResult(new { result = true, url = _webSite.GetPageUrl(page) });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult RemovePage(string url)
		{
			_webSite.Logger?.LogInformation($"remove {url}");
			SitePage page = _webSite.Pages.FirstOrDefault(p => p.Url == url);
			_webSite.DeleteSitePage(page);
			var redirectUrl = page.Parent == null ? _webSite.Configuration.DefaultPage : _webSite.GetPageUrl(page.Parent);
			return new JsonResult(new { result = true, url = redirectUrl });
		}
	}
}