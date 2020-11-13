using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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
			return Content($"{Environment.MachineName} {_webSite.HostingEnvironment.EnvironmentName} {_webSite.HostingEnvironment.ContentRootPath}");
		}

		public IActionResult LoadAssets()
		{
			_webSite.ReloadAssets();
			return new JsonResult(_webSite.Assets.Select(a => a.VirtualPath));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult SaveContent(string url, string items)
		{
			ISitePage page = _webSite.GetPageByUrl(url, _webSite.IsAuthenticated(User));
			if (page != null && page.Url != "404")
			{
				_webSite.Logger?.LogInformation($"save PAGE found {page.Url}");
				var newSections = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<IPageSection>>(items, _webSite.ContentStorage.JsonInterfaceConverter);
				page.Sections.Clear();
				page.Sections.AddRange(newSections);

				_webSite.SaveSitePage(page, Request, true);
				return new JsonResult(new { result = true });
			}
			return new JsonResult(new { result = false });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult SaveAssets(string virtualFolder, List<IFormFile> files)
		{
			if (files.Count > 0)
			{
				List<IAsset> assets = new List<IAsset>();
				foreach (var file in files)
				{
					using (var ms = new MemoryStream())
					{
						file.CopyTo(ms);
						var fileBytes = ms.ToArray();
						var newAsset = _webSite.AssetStorage.CreateAsset(file.FileName, fileBytes, virtualFolder);
						assets.Add(newAsset);
					}
				}
				return new JsonResult(new { result = true, assets = assets.Select(a => new { a.FileName, a.Folder, a.VirtualPath, a.Type }) });
			}
			return new JsonResult(new { result = false });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> MultiplePages(List<IFormFile> files, bool force)
		{
			if (files.Count > 0)
			{
				foreach (var file in files)
				{
					using (var reader = new StreamReader(file.OpenReadStream()))
					{
						var content = await reader.ReadToEndAsync();
						var sitePage = _webSite.ContentStorage.Deserialize(content);
						if (_webSite.Pages.FirstOrDefault(p => p.Url == sitePage.Url) == null || force)
						{
							_webSite.SaveSitePage(sitePage, Request);
						} 
						else if (!force)
						{
							return new JsonResult(new { result = false, message = $"Page with url {sitePage.Url} already exists" });
						}
					}
				}
				return new JsonResult(new { result = true });
			}
			return new JsonResult(new { result = false });
		}
		
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult SavePage([FromForm]SitePageBasicPostModel posted)
		{
			//ignore move for now...
			if (Request.Form.ContainsKey("OldUrl") && (string)Request.Form["OldUrl"] != posted.Url)
			{
				string message = $"Moving pages not allowed yet, tried to move {Request.Form["OldUrl"]} to new location: {posted.Url}";
				_webSite.Logger?.LogError(message);
				return new JsonResult(new { result = false, message = message });
			}

			//find current page
			ISitePage page = _webSite.Pages.FirstOrDefault(p => p.Url == posted.Url);
			if (page == null)
			{
				//new page
				page = _webSite.ContentStorage.NewPage();
				page.Url = posted.Url;
				page.Created = DateTime.Now;
				page.Sections = _webSite.GetDefaultContentForTemplate(page.Template);
			}
			//only reset properties of posted model on page
			page.RedirectUrl = posted.RedirectUrl;
			page.Layout = posted.Layout;
			page.MetaDescription = posted.MetaDescription;
			page.MetaTitle = posted.MetaTitle;
			page.ShowInMenu = posted.ShowInMenu;
			page.SortOrder = posted.SortOrder;
			page.Template = posted.Template;
			page.Title = posted.Title;
			page.Visible = posted.Visible;

			_webSite.SaveSitePage(page, Request, false);
			return new JsonResult(new { result = true, url = _webSite.GetPageUrl(page) });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult RemovePage(string url)
		{
			_webSite.Logger?.LogInformation($"remove {url}");
			ISitePage page = _webSite.Pages.FirstOrDefault(p => p.Url == url);
			_webSite.DeleteSitePage(page);
			var redirectUrl = page.Parent == null ? _webSite.Configuration.DefaultPage : _webSite.GetPageUrl(page.Parent);
			return new JsonResult(new { result = true, url = redirectUrl });
		}
	}
}