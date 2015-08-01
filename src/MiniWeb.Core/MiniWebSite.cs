using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNet.Antiforgery;
using Microsoft.AspNet.Hosting;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;

namespace MiniWeb.Core
{
	public class MiniWebSite : IMiniWebSite
	{
		public IAntiforgery Antiforgery { get; }
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
					Url = "/miniweb/login"
				};
			}
		}

		public MiniWebSite(IHostingEnvironment env, IApplicationEnvironment appEnv, ILoggerFactory loggerfactory,
		IAntiforgery antiforgery, IMiniWebStorage storage, IOptions<MiniWebConfiguration> config)
		{
			Pages = Enumerable.Empty<SitePage>();

			AppEnvironment = appEnv;
			HostingEnvironment = env;
			Antiforgery = antiforgery;
			Logger = SetupLogging(loggerfactory);
			Storage = storage;
			Configuration = config.Options;

			//pass on self to storage module
			//cannot inject because of circular reference.
			storage.MiniWebSite = this;

		}

		private ILogger SetupLogging(ILoggerFactory loggerfactory)
		{
			//TODO(RC) Read settings from config
			return loggerfactory.CreateLogger("MiniWeb");
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
			if (user?.Identities?.Any(i => i.AuthenticationType == Configuration.Authentication.AuthenticationType) == true &&
			   user?.Claims?.Any(c => c.Type == ClaimTypes.Role && c.Value == Configuration.Authentication.MiniWebCmsRoleValue) == true)
			{
				return true;
			}
			return false;
		}

		public bool Authenticate(string user, string password)
		{
			return Storage.Authenticate(user, password);
		}

		public void ReloadPages()
		{
			Logger?.LogInformation("Reload pages");
			Pages = Storage.AllPages().ToList();
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
				//save current with base 64 so at least it's saved.
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
				//NOTE(RC) TEMP FIX FOR MAPPATH BUG https://github.com/aspnet/Hosting/issues/222
				originalDestFile = originalDestFile.Replace("\\~\\", "\\");
				originalDestFile = originalDestFile.Replace("/~/", "/");
				//TEMP FIX FOR MAPPATH BUG
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

			//NOTE(RC) TEMP FIX FOR MAPPATH BUG https://github.com/aspnet/Hosting/issues/222
			file = file.Replace("\\~\\", "\\");
			file = file.Replace("/~/", "/");
			//TEMP FIX FOR MAPPATH BUG

			File.WriteAllBytes(file, bytes);

			//TODO(RC) fix Virtual Path idealy use something like VirtualPathUtility.ToAbsolute(relative);
			var relativeSansTilde = relative.Substring(1);
			return relativeSansTilde;
		}
	}
}
