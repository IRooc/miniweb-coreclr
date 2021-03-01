using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace MiniWeb.Core
{
	public class MiniWebSite : IMiniWebSite
	{
		public const string EmbeddedBase64FileInHtmlRegex = "(src|href)=\"(data:([^\"]+))\"(\\s+data-filename=\"([^\"]+)\")?";
		public const string EmbeddedBase64FileInValueRegex = "(data:([^\"]+))";
		public MiniWebConfiguration Configuration { get; }
		public IHostingEnvironment HostingEnvironment { get; }

		public ILogger Logger { get; }
		public IMiniWebContentStorage ContentStorage { get; }
		public IMiniWebAssetStorage AssetStorage { get; }
		public IMemoryCache Cache { get; }

		public IEnumerable<ISitePage> PageHierarchy { get; set; }
		public IEnumerable<ISitePage> Pages { get; set; }
		public IEnumerable<string> PageTemplates
		{
			get
			{
				string basePath = HostingEnvironment.ContentRootPath;
				return System.IO.Directory.GetFiles(basePath + Configuration.PageTemplatePath).Select(t => t = t.Replace(basePath, "~").Replace("\\", "/"));
			}
		}

		public IEnumerable<string> ItemTemplates
		{
			get
			{
				string basePath = HostingEnvironment.ContentRootPath;
				return System.IO.Directory.GetFiles(basePath + Configuration.ItemTemplatePath).Select(t => t = t.Replace(basePath, "~").Replace("\\", "/"));
			}
		}


		public MiniWebSite(IHostingEnvironment env, ILoggerFactory loggerfactory, IMiniWebContentStorage storage, IMiniWebAssetStorage assetStorage,
						   IMemoryCache cache, IOptions<MiniWebConfiguration> config)
		{
			Pages = Enumerable.Empty<ISitePage>();

			HostingEnvironment = env;
			Configuration = config.Value;
			ContentStorage = storage;
			AssetStorage = assetStorage;
			Cache = cache;
			Logger = SetupLogging(loggerfactory);

			//pass on self to storage module
			//cannot inject because of circular reference.
			ContentStorage.MiniWebSite = this;
			AssetStorage.MiniWebSite = this;

			this.ReloadPages();
			this.ReloadAssets();

		}

		private ILogger SetupLogging(ILoggerFactory loggerfactory)
		{
			if (!string.IsNullOrWhiteSpace(Configuration?.LogCategoryName))
			{
				return loggerfactory.CreateLogger(Configuration.LogCategoryName);
			}
			return null;
		}

		public FindResult GetPageByUrl(string url, bool editing = false)
		{
			var result = new FindResult();
			Logger?.LogDebug($"Finding page {url}");
			if (string.IsNullOrWhiteSpace(url) || url == "/")
			{
				Logger?.LogDebug("Homepage");
				url = Configuration.DefaultPage;
			}
			var suffix = string.Empty;
			if (url?.StartsWith("/") == true)
			{
				url = url.Substring(1);
			}
			if (!string.IsNullOrWhiteSpace(Configuration.PageExtension) && url.Contains(Configuration.PageExtension))
			{
				string urlPattern = $"^(.*?)\\.{Configuration.PageExtension}(.*?)$";
				Match match = Regex.Match(url, urlPattern);
				if (match.Success)
				{
					url = match.Groups[1].Value;
					suffix = match.Groups[2].Value;
				}
			}

			var pageByUrl = Pages.FirstOrDefault(p => p.Url == url) ?? ContentStorage.GetSitePageByUrl(url) ?? ContentStorage.MiniWeb404Page;
			var foundPage = pageByUrl.Visible || editing ? pageByUrl : ContentStorage.MiniWeb404Page;
			if (foundPage == ContentStorage.MiniWeb404Page)
			{
				Logger?.LogWarning($"Could not find page [{url}] found page: [{foundPage.Url}]");
			}
			else
			{
				result.Found = true;
				if (string.IsNullOrWhiteSpace(foundPage.RedirectUrl))
				{
					if (foundPage.Url != url && $"{foundPage.Url}.{Configuration.PageExtension}" != url && (foundPage.Url != "404"))
					{
						if (!string.IsNullOrWhiteSpace(Configuration.PageExtension))
						{
							result.RedirectUrl = $"/{foundPage.Url}.{Configuration.PageExtension}";
						}
						else
						{
							result.RedirectUrl = $"/{foundPage.Url}";
						}
					}
					if (Configuration.RedirectToFirstSub && foundPage.Pages.Any())
					{
						result.RedirectUrl = foundPage.Pages.First().Url;
					}
				}
				else if (!editing)
				{
					//remove the redirectUrl s
					result.RedirectUrl = foundPage.RedirectUrl;
				}
				Logger?.LogInformation($"Found page [{foundPage.Url}] from url: [{url}]");
			}
			result.Page = foundPage;
			return result;
		}

		public string GetPageUrl(ISitePage page)
		{
			if (page == null)
			{
				return "/";
			}
			if (string.IsNullOrWhiteSpace(Configuration.PageExtension))
			{
				return $"/{page.Url}";
			}
			return $"/{page.Url}.{Configuration.PageExtension}";
		}

		public bool IsAuthenticated(ClaimsPrincipal user)
		{
			return user?.IsInRole(MiniWebAuthentication.MiniWebCmsRoleValue) == true;
		}

		public bool Authenticate(string username, string password)
		{
			return ContentStorage.Authenticate(username, password);
		}

		public ClaimsPrincipal GetClaimsPrincipal(string username)
		{
			var claims = new[] {
					new Claim(ClaimTypes.Name, username),
					new Claim(ClaimTypes.Role, MiniWebAuthentication.MiniWebCmsRoleValue)
				};

			Logger?.LogInformation($"signing in as :{username}");
			var identity = new ClaimsIdentity(claims, Configuration.Authentication.AuthenticationScheme);
			var principal = new ClaimsPrincipal(identity);
			return principal;
		}

		public void ReloadPages(bool forceReload = false)
		{
			// Look for cache key.
			IEnumerable<ISitePage> pages = null;
			if (!forceReload && Cache?.TryGetValue("MWPAGES", out pages) == true)
			{
				Logger?.LogInformation("Cached pages");
				Pages = pages;
			}
			else
			{
				Logger?.LogInformation("Reload pages");
				Pages = ContentStorage.AllPages().ToList();

				Cache?.Set("MWPAGES", Pages);
			}
			PageHierarchy = Pages.Where(p => !p.Url.Contains("/")).OrderBy(p => p.SortOrder).ThenBy(p => p.Title);
			foreach (var page in Pages)
			{
				string urlStart = page.Url + "/";
				page.Pages = Pages.Where(p => p.Url.StartsWith(urlStart) && !p.Url.Replace(urlStart, "").Contains("/")).OrderBy(p => p.SortOrder).ThenBy(p => p.Title);
				if (page.Url.Contains("/"))
				{
					//set parent
					var parentUrl = page.Url.Substring(0, page.Url.LastIndexOf("/"));
					page.Parent = Pages.FirstOrDefault(p => p.Url == parentUrl);
				}
			}
		}

		public void SaveSitePage(ISitePage page, HttpRequest currentRequest, bool storeImages = false)
		{
			Logger?.LogInformation($"Saving page {page.Url}");
			page.LastModified = DateTime.Now;
			if (page.Sections == null)
			{
				page.Sections = new List<IPageSection>();
			}
			if (storeImages)
			{
				//NOTE(RC): save current with base 64 so at least it's saved.
				ContentStorage.StoreSitePage(page, currentRequest);
				//NOTE(RC): can this be done saner?
				foreach (var item in page.Sections.SelectMany(s => s.Items).Where(i => i.Values.Any(kv => kv.Value.Contains("data:"))))
				{
					for (var i = 0; i < item.Values.Count; i++)
					{
						var kv = item.Values.ElementAt(i);
						item.Values[kv.Key] = SaveEmbeddedImages(item.Values[kv.Key]);
					}
				}
			}
			ContentStorage.StoreSitePage(page, currentRequest);
			ReloadPages(true);
		}

		public void DeleteSitePage(ISitePage page)
		{
			Logger?.LogInformation($"Deleting page {page.Url}");
			ContentStorage.DeleteSitePage(page);
			ReloadPages(true);
		}

		public List<IPageSection> GetDefaultContentForTemplate(string template)
		{
			var defaultContent = Configuration.DefaultContent?.FirstOrDefault(t => string.CompareOrdinal(t.Template, template) == 0);
			return ContentStorage.GetDefaultSectionContent(defaultContent);
		}


		public IEnumerable<IAsset> Assets { get; set; }
		public void DeleteAsset(IAsset asset)
		{
			AssetStorage.RemoveAsset(asset);
			ReloadAssets(true);
		}

		public void ReloadAssets(bool forceReload = false)
		{
			IEnumerable<IAsset> assets = null;
			if (!forceReload && Cache?.TryGetValue("MWASSETS", out assets) == true)
			{
				Logger?.LogInformation("Cached assets");
				Assets = assets;
			}
			else
			{
				Logger?.LogInformation("Reload assets");
				Assets = AssetStorage.GetAllAssets();

				Cache?.Set("MWASSETS", Assets);
			}
		}

		public IContentItem DummyContent(string template)
		{
			return new DummyContentItem
			{
				Template = template
			};
		}


		private string SaveEmbeddedImages(string html)
		{
			//handle each match individually, so multiple the same images are not stored twice but parsed once and replaced multiple times
			Match match = Regex.Match(html, EmbeddedBase64FileInHtmlRegex);
			while (match.Success && !string.IsNullOrEmpty(match?.Value))
			{
				string filename = match.Groups[5].Value;
				string base64String = match.Groups[2].Value;
				if (!string.IsNullOrWhiteSpace(base64String))
				{
					//byte[] bytes = ConvertToBytes(base64String);
					var newAsset = AssetStorage.CreateAsset(filename, base64String);
					//string extension = Regex.Match(match.Value, "data:([^/]+)/([a-z]+);base64").Groups[2].Value;
					string path = newAsset.VirtualPath;// SaveFileToDisk(bytes, extension, filename);

					//replace URL in content
					string value = string.Format("src=\"{0}\" alt=\"\" ", path);

					if (match.Groups[1].Value == "href")
						value = string.Format("href=\"{0}\"", path);

					html = html.Replace(match.Value, value);
					//next match.
					match = Regex.Match(html, EmbeddedBase64FileInHtmlRegex);
				}
			}
			match = Regex.Match(html, EmbeddedBase64FileInValueRegex);
			if (match.Success && !string.IsNullOrEmpty(match?.Value))
			{
				string base64String = match.Groups[2].Value;
				var fileNameMatch = Regex.Match(base64String, "(;filename=(.*?));base64");
				var fileName = fileNameMatch.Groups[2].Value;
				base64String = base64String.Replace(fileNameMatch.Groups[1].Value, "");
				var newAsset = AssetStorage.CreateAsset(fileName, base64String);
				html = newAsset.VirtualPath;
			}
			return html;
		}
	}
}
