using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Razor.Hosting;
using System.IO;

namespace MiniWeb.Core
{
	public class MiniWebSite : IMiniWebSite
	{
		public const string EmbeddedBase64FileInHtmlRegex = "(src|href)=\"(data:([^\"]+))\"(\\s+data-filename=\"([^\"]+)\")?";
		public const string EmbeddedBase64FileInValueRegex = "(data:([^\"]+))";
		public const string PagesCacheKey = "MiniWebPagesCacheKey";
		public const string AssetsCacheKey = "MiniWebAssetsCacheKey";
		public MiniWebConfiguration Configuration { get; }
		public IWebHostEnvironment HostingEnvironment { get; }
		public ILogger Logger { get; }
		public IMiniWebContentStorage ContentStorage { get; }
		public IMiniWebAssetStorage AssetStorage { get; }
		public IMemoryCache Cache { get; }
		public IEnumerable<string> PageTemplates
		{
			get
			{
				var templatePath = Configuration.PageTemplatePath;
				return GetTemplatesForPath(templatePath);
			}
		}
		public IEnumerable<string> ItemTemplates
		{
			get
			{
				var templatePath = Configuration.ItemTemplatePath;
				return GetTemplatesForPath(templatePath);
			}
		}
		private IEnumerable<string> GetTemplatesForPath(string templatePath)
		{
			var basePath = HostingEnvironment.ContentRootPath;
			var result = Enumerable.Empty<string>();
			if (HostingEnvironment.IsDevelopment() && Directory.Exists(basePath + templatePath))
			{
				result = System.IO.Directory.GetFiles(basePath + templatePath).Select(t => t = t.Replace(basePath, "~").Replace("\\", "/"));
			}
			if (!result.Any())
			{
				//find assemblies with precompiled views
				var assemblies = System.AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetCustomAttributes(typeof(RazorCompiledItemAttribute), false).Any());
				var resultList = new List<string>();
				foreach (var assembly in assemblies)
				{
					var attribs = assembly.GetCustomAttributes(typeof(RazorCompiledItemAttribute), false).OfType<RazorCompiledItemAttribute>();
					resultList.AddRange(attribs.Where(a => a.Identifier.StartsWith(templatePath)).Select(a => a.Identifier));
				}
				result = resultList.ToArray();
			}
			return result;
		}

		public MiniWebSite(IWebHostEnvironment env, ILogger<MiniWebSite> logger, IMiniWebContentStorage storage, IMiniWebAssetStorage assetStorage,
						   IMemoryCache cache, IOptions<MiniWebConfiguration> config)
		{
			HostingEnvironment = env;
			Configuration = config.Value;
			ContentStorage = storage;
			AssetStorage = assetStorage;
			Cache = cache;
			Logger = logger;

			//pass on self to storage module
			//cannot inject because of circular reference.
			ContentStorage.MiniWebSite = this;
			AssetStorage.MiniWebSite = this;
		}


		public async Task<FindResult> GetPageByUrl(string url, ClaimsPrincipal user)
		{
			bool editing = IsAuthenticated(user);
			var result = new FindResult();
			var urlToFind = url;
			Logger?.LogDebug($"Finding page {url}");
			if (string.IsNullOrWhiteSpace(urlToFind) || urlToFind == "/")
			{
				Logger?.LogDebug("Homepage");
				urlToFind = Configuration.DefaultPage;
			}
			var suffix = string.Empty;
			if (urlToFind?.StartsWith("/") == true)
			{
				urlToFind = urlToFind.Substring(1);
			}
			if (!string.IsNullOrWhiteSpace(Configuration.PageExtension) && urlToFind.Contains(Configuration.PageExtension))
			{
				string urlPattern = $"^(.*?)\\.{Configuration.PageExtension}(.*?)$";
				Match match = Regex.Match(urlToFind, urlPattern);
				if (match.Success)
				{
					urlToFind = match.Groups[1].Value;
					suffix = match.Groups[2].Value;
				}
			}

			ISitePage notFoundPage = await ContentStorage.MiniWeb404Page();
			var pageByUrl = (await Pages()).FirstOrDefault(p => p.Url == urlToFind) ?? (await ContentStorage.GetSitePageByUrl(urlToFind)) ?? notFoundPage;
			var foundPage = pageByUrl.Visible || editing ? pageByUrl : notFoundPage;
			if (foundPage == notFoundPage)
			{
				Logger?.LogWarning($"Could not find page [{url}] found page: [{foundPage.Url}]");
			}
			else
			{
				result.Found = true;
				if (string.IsNullOrWhiteSpace(foundPage.RedirectUrl))
				{
					if (foundPage.Url != urlToFind && $"{foundPage.Url}.{Configuration.PageExtension}" != urlToFind && (foundPage.Url != "404"))
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

		public async Task<bool> Authenticate(string username, string password)
		{
			return await ContentStorage.Authenticate(username, password);
		}

		public ClaimsPrincipal GetClaimsPrincipal(string username)
		{
			Claim[] claims = GetClaimsFor(username);
			Logger?.LogInformation($"signing in as: {username}");
			var identity = new ClaimsIdentity(claims, Configuration.Authentication.AuthenticationScheme);
			var principal = new ClaimsPrincipal(identity);
			return principal;
		}

		public static Claim[] GetClaimsFor(string username)
		{
			return new [] {
					new Claim(ClaimTypes.Name, username),
					new Claim(ClaimTypes.Role, MiniWebAuthentication.MiniWebCmsRoleValue)
				};
		}

		public async Task<IEnumerable<ISitePage>> Pages(bool forceReload = false)
		{
			// Look for cache key.
			IEnumerable<ISitePage> result = null;
			if (forceReload || (Cache?.TryGetValue(PagesCacheKey, out result)) != true)
			{
				Logger?.LogInformation("Reload pages");
				result = (await ContentStorage.AllPages()).OrderBy(p => p.SortOrder).ThenBy(p => p.Title);

				Cache?.Set(PagesCacheKey, result);
			}
			//Create Hierarchy
			foreach (var page in result)
			{
				string urlStart = page.Url + "/";
				page.Pages = result.Where(p => p.Url.StartsWith(urlStart) && !p.Url.Replace(urlStart, "").Contains("/")).OrderBy(p => p.SortOrder).ThenBy(p => p.Title);
				if (page.Url.Contains("/"))
				{
					//set parent
					var parentUrl = page.Url.Substring(0, page.Url.LastIndexOf("/"));
					page.Parent = result.FirstOrDefault(p => p.Url == parentUrl);
				}
			}
			return result;
		}

		public async Task SaveSitePage(ISitePage page, HttpRequest currentRequest, bool storeImages = false)
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
				await ContentStorage.StoreSitePage(page, currentRequest);
				//NOTE(RC): can this be done saner?
				foreach (var item in page.Sections.SelectMany(s => s.Items).Where(i => i.Values.Any(kv => kv.Value.Contains("data:"))))
				{
					for (var i = 0; i < item.Values.Count; i++)
					{
						var kv = item.Values.ElementAt(i);
						item.Values[kv.Key] = await SaveEmbeddedImages(item.Values[kv.Key]);
					}
				}
			}
			await ContentStorage.StoreSitePage(page, currentRequest);
			Cache?.Remove(PagesCacheKey);
		}

		public async Task DeleteSitePage(ISitePage page)
		{
			Logger?.LogInformation($"Deleting page {page.Url}");
			await ContentStorage.DeleteSitePage(page);
			Cache?.Remove(PagesCacheKey);
		}

		public async Task<List<IPageSection>> GetDefaultContentForTemplate(string template)
		{
			var defaultContent = Configuration.DefaultContent?.FirstOrDefault(t => string.CompareOrdinal(t.Template, template) == 0);
			return await ContentStorage.GetDefaultSectionContent(defaultContent);
		}


		public async Task<IEnumerable<IAsset>> Assets(bool forceReload = false)
		{
			IEnumerable<IAsset> result = null;
			if (forceReload || (Cache?.TryGetValue(AssetsCacheKey, out result)) != true)
			{
				Logger?.LogInformation("Reload assets");
				result = await AssetStorage.GetAllAssets();

				Cache?.Set(AssetsCacheKey, result);
			}
			return result;

		}

		public async Task DeleteAsset(IAsset asset)
		{
			await AssetStorage.RemoveAsset(asset);
			Cache?.Remove(AssetsCacheKey);
		}


		public IContentItem DummyContent(string template)
		{
			return new DummyContentItem
			{
				Template = template
			};
		}


		private async Task<string> SaveEmbeddedImages(string html)
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
					var newAsset = await AssetStorage.CreateAsset(filename, base64String);
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
				var newAsset = await AssetStorage.CreateAsset(fileName, base64String);
				html = newAsset.VirtualPath;
			}
			return html;
		}
	}
}
