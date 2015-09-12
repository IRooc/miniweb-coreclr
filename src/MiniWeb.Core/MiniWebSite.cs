using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNet.Hosting;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;

namespace MiniWeb.Core
{
	public class MiniWebSite : IMiniWebSite
	{
		public IApplicationEnvironment AppEnvironment { get; }
		public MiniWebConfiguration Configuration { get; }
		public IHostingEnvironment HostingEnvironment { get; }

		public ILogger Logger { get; }
		public IMiniWebStorage Storage { get; }

		public IEnumerable<SitePage> PageHierarchy { get; set; }
		public IEnumerable<SitePage> Pages { get; set; }
		public IEnumerable<string> PageTemplates
		{
			get
			{
				string basePath = AppEnvironment.ApplicationBasePath;
				return System.IO.Directory.GetFiles(basePath + Configuration.PageTemplatePath).Select(t => t = t.Replace(basePath, "~").Replace("\\", "/"));
			}
		}

		public IEnumerable<string> ItemTemplates
		{
			get
			{
				string basePath = AppEnvironment.ApplicationBasePath;
				return System.IO.Directory.GetFiles(basePath + Configuration.ItemTemplatePath).Select(t => t = t.Replace(basePath, "~").Replace("\\", "/"));
			}
		}

		public SitePage Page404
		{
			get
			{
				return Pages.FirstOrDefault(p => p.Url == "404") ?? new SitePage()
				{
					Title = "Page Not Found : 404",
					MetaTitle = "Page Not Found : 404",
					Layout = Configuration.Layout,
					Template = $"~{Configuration.PageTemplatePath}/OneColumn.cshtml",
					Sections = new List<PageSection>()
					{
						new PageSection()
						{
							Key = "content",
							Items = new List<ContentItem>()
							{
								new ContentItem {
									Template = $"~{Configuration.ItemTemplatePath}/item.cshtml",
									Values =
									{
										["title"] = "404",
										["content"] = "Page not found"
									}
								}
							}
						}
						},
					Url = "404"
				};
			}
		}

		public SitePage PageLogin
		{
			get
			{
				return new SitePage()
				{
					Title = "Login",
					MetaTitle = "Login",
					Layout = Configuration.Layout,
					Template = $"~{Configuration.PageTemplatePath}/OneColumn.cshtml",
					Sections = new List<PageSection>(),
					Url = "/miniweb/login",
					Visible = true
				};
			}
		}

		public MiniWebSite(IHostingEnvironment env, IApplicationEnvironment appEnv, ILoggerFactory loggerfactory,
								 IMiniWebStorage storage, IOptions<MiniWebConfiguration> config)
		{
			Pages = Enumerable.Empty<SitePage>();

			AppEnvironment = appEnv;
			HostingEnvironment = env;
			Configuration = config.Value;
			Storage = storage;
			Logger = SetupLogging(loggerfactory);			

			//pass on self to storage module
			//cannot inject because of circular reference.
			Storage.MiniWebSite = this;

		}

		private ILogger SetupLogging(ILoggerFactory loggerfactory)
		{
			if (!string.IsNullOrWhiteSpace(Configuration?.LogCategoryName))
			{
				return loggerfactory.CreateLogger(Configuration.LogCategoryName);
			}
			return null;
		}

		public SitePage GetPageByUrl(string url, bool editing = false)
		{
			Logger?.LogVerbose($"Finding page {url}");
			if (url?.StartsWith("/") == true)
			{
				url = url.Substring(1);
			}

			var pageByUrl = Pages.FirstOrDefault(p => p.Url == url) ?? Storage.GetSitePageByUrl(url) ?? Page404;
			var foundPage = pageByUrl.Visible || editing ? pageByUrl : Page404;
			if (foundPage == Page404)
			{
				Logger?.LogWarning($"Could not find page [{url}] found page: [{foundPage.Url}]");
			}
			else
			{
				Logger?.LogInformation($"Found page [{foundPage.Url}] from url: [{url}]");
			}
			return foundPage;
		}

		public bool IsAuthenticated(ClaimsPrincipal user)
		{
			return user?.IsInRole(MiniWebAuthentication.MiniWebCmsRoleValue) == true;		
		}

		public bool Authenticate(string user, string password)
		{
			return Storage.Authenticate(user, password);
		}

		public void ReloadPages()
		{
			Logger?.LogInformation("Reload pages");
			Pages = Storage.AllPages().ToList();
			//NOTE(RC): only two levels, recurse??
			PageHierarchy = Pages.Where(p => !p.Url.Contains("/")).OrderBy(p => p.SortOrder).ThenBy(p => p.Title);
			foreach (var page in PageHierarchy)
			{
				string urlStart = page.Url + "/";
				page.Pages = Pages.Where(p => p.Url.StartsWith(urlStart) && !p.Url.Replace(urlStart, "").Contains("/")).OrderBy(p => p.SortOrder).ThenBy(p => p.Title);
			}
		}

		public void SaveSitePage(SitePage page, bool storeImages = false)
		{
			Logger?.LogInformation($"Saving page {page.Url}");
			page.LastModified = DateTime.Now;
			if (page.Sections == null)
			{
				page.Sections = new List<PageSection>();
			}
			if (storeImages)
			{
				//NOTE(RC): save current with base 64 so at least it's saved.
				Storage.StoreSitePage(page);
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
			Storage.StoreSitePage(page);
			ReloadPages();
		}

		public void DeleteSitePage(SitePage page)
		{
			Logger?.LogInformation($"Deleting page {page.Url}");
			Storage.DeleteSitePage(page);
			ReloadPages();
		}

		private string SaveEmbeddedImages(string html)
		{
			//handle each match individually, so multiple the same images are not stored twice but parsed once and replaced multiple times
			Match match = Regex.Match(html, "(data-filename=\"([^\"]+)\"\\s+)(src|href)=\"(data:([^\"]+))\"?");
			while (!string.IsNullOrEmpty(match.Value))
			{
				string extension = Regex.Match(match.Value, "data:([^/]+)/([a-z]+);base64").Groups[2].Value;
				string filename = match.Groups[2].Value;
				byte[] bytes = ConvertToBytes(match.Groups[5].Value);
				string path = SaveFileToDisk(bytes, extension, filename);

				string value = string.Format("src=\"{0}\" alt=\"\" ", path);

				if (match.Groups[1].Value == "href")
					value = string.Format("href=\"{0}\"", path);

				html = html.Replace(match.Value, value);
				//next match.
				match = Regex.Match(html, "(data-filename=\"([^\"]+)\"\\s+)(src|href)=\"(data:([^\"]+))\"?");
			}
			return html;
		}
		private byte[] ConvertToBytes(string base64)
		{
			int index = base64.IndexOf("base64,", StringComparison.Ordinal) + 7;
			return Convert.FromBase64String(base64.Substring(index));
		}
		public string SaveFileToDisk(byte[] bytes, string extension, string originalFilename)
		{
			string relative = Configuration.ImageSavePath + Guid.NewGuid() + "." + extension.Trim('.');
			if (!string.IsNullOrWhiteSpace(originalFilename))
			{
				var originalDestFile = HostingEnvironment.MapPath(Configuration.ImageSavePath + originalFilename);
				if (!File.Exists(originalDestFile))
				{
					relative = Configuration.ImageSavePath + originalFilename;
				}
				else
				{
					//TODO(RC) find unique imagefilename without guid...
					relative = Configuration.ImageSavePath + Guid.NewGuid() + "." + originalFilename;
				}
				relative = relative.ToLowerInvariant();
			}
			string file = HostingEnvironment.MapPath(relative);
			File.WriteAllBytes(file, bytes);

			//TODO(RC) is this correct, path for browser to wwwroot;
			var absolutepath = $"/{relative}";
         return absolutepath;
		}
	}
}
