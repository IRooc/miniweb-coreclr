using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MiniWeb.Core;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MiniWeb.Storage.JsonStorage
{
	public class MiniWebJsonStorage : IMiniWebContentStorage
	{
		private MiniWebJsonStorageConfig StorageConfig { get; }
		public IMiniWebSite MiniWebSite { get; set; }

		public MiniWebJsonStorage(IOptions<MiniWebJsonStorageConfig> config)
		{
			StorageConfig = config.Value;
		}

		public Task<bool> Authenticate(string username, string password)
		{
			var result = StorageConfig.Users?.Any(u => string.Compare(u.Key, username, true) == 0 && !string.IsNullOrEmpty(u.Value) && string.Compare(u.Value, password) == 0) == true;
			return Task.FromResult(result);
		}

		public Task<ISitePage> GetSitePageByUrl(string url)
		{
			string name = GetSitePageFileName(url.ToLowerInvariant());
			ISitePage result = null;
			if (File.Exists(name))
			{
				result = DeSerializeFile<JsonSitePage>(name);
			}
			return Task.FromResult(result);
		}

		public Task<ISitePage> Deserialize(string filecontent)
		{
			return Task.FromResult<ISitePage>(JsonConvert.DeserializeObject<JsonSitePage>(filecontent, JsonInterfaceConverter));
		}

		public Task StoreSitePage(ISitePage sitePage, HttpRequest currentRequest)
		{
			string name = GetSitePageFileName(sitePage.Url.ToLowerInvariant());
			if (MiniWebSite.Configuration.StoreVersions && File.Exists(name))
			{
				string lowerCaseUrl = sitePage.Url.ToLowerInvariant();
				string version = GetSitePageVersionFileName($"{lowerCaseUrl}[{sitePage.LastModified.Ticks}]");

				File.Copy(name, version);
			}
			SerializeObject(name, sitePage);
			return Task.FromResult(0);
		}

		public Task DeleteSitePage(ISitePage sitePage)
		{
			string name = GetSitePageFileName(sitePage.Url.ToLowerInvariant());
			File.Delete(name);
			return Task.FromResult(0);
		}

		public Task<IEnumerable<ISitePage>> AllPages()
		{
			List<JsonSitePage> pages = new List<JsonSitePage>();
			if (Directory.Exists(MiniWebSite.HostingEnvironment.ContentRootPath + StorageConfig.SitePageFolder))
			{
				string[] pageFiles = Directory.GetFiles(MiniWebSite.HostingEnvironment.ContentRootPath + StorageConfig.SitePageFolder, "*.json");
				foreach (string page in pageFiles)
				{
					MiniWebSite.Logger?.LogDebug($"Loading page from disc {page}");
					pages.Add(DeSerializeFile<JsonSitePage>(page));
				}
			}
			return Task.FromResult<IEnumerable<ISitePage>>(pages);
		}

		public Task<List<IPageSection>> GetDefaultSectionContent(DefaultContent defaultContent)
		{
			var result = defaultContent?.Content?.Select(c => new JsonPageSection()
			{
				Key = c.Section,
				Items = c.Items?.Select(i => new JsonContentItem()
				{
					Template = i,
					Values = new Dictionary<string, string>()
				}).ToList<IContentItem>()
			}).ToList<IPageSection>();
			return Task.FromResult(result);
		}

		public async Task<ISitePage> MiniWeb404Page()
		{
			return (await AllPages()).FirstOrDefault(p => p.Url == "404") ?? new JsonSitePage()
			{
				Title = "Page Not Found : 404",
				MetaTitle = "Page Not Found : 404",
				Layout = StorageConfig.Layout,
				Template = $"~{StorageConfig.PageTemplatePath}/OneColumn.cshtml",
				Visible = true,
				Sections = new List<IPageSection>()
					{
						new JsonPageSection()
						{
							Key = "content",
							Items = new List<IContentItem>()
							{
								new JsonContentItem {
									Template = $"~{StorageConfig.ItemTemplatePath}/item.cshtml",
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

		public Task<ISitePage> MiniWebLoginPage()
		{
			var result = new JsonSitePage()
			{
				Title = "Login",
				MetaTitle = "Login",
				Layout = StorageConfig.Layout,
				Template = $"~{StorageConfig.PageTemplatePath}/OneColumn.cshtml",
				Sections = new List<IPageSection>(),
				Url = "miniweb/login",
				Visible = true
			};
			return Task.FromResult<ISitePage>(result);
		}

		public JsonConverter JsonInterfaceConverter
		{
			get
			{
				return new JsonInterfaceConverter();
			}
		}

		public Task<ISitePage> NewPage()
		{
			return Task.FromResult<ISitePage>(new JsonSitePage());
		}

		private void SerializeObject(string filename, object obj)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				string fileContent = JsonConvert.SerializeObject(obj, new JsonSerializerSettings { Formatting = Formatting.Indented });
				var folder = Path.GetDirectoryName(filename);
				if (!Directory.Exists(folder))
				{
					Directory.CreateDirectory(folder);
				}
				if (File.Exists(filename))
					File.Delete(filename);
				File.WriteAllText(filename, fileContent);
			}
		}

		private T DeSerializeFile<T>(string filename)
		{
			try
			{
				string jsonString = File.ReadAllText(filename);
				return JsonConvert.DeserializeObject<T>(jsonString, JsonInterfaceConverter);

			}
			catch (Exception ex)
			{
				MiniWebSite.Logger?.LogWarning($"Could not deserialize file: {filename} exception {ex.Message}");
				return default(T);
			}

		}
		/// <summary>
		/// Gets the name of the site page file.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <returns></returns>
		private string GetSitePageFileName(string url)
		{
			string name = url.Replace('/', '~') + ".json";
			name = name.TrimStart('~');
			name = MiniWebSite.HostingEnvironment.ContentRootPath + StorageConfig.SitePageFolder + name;
			return name;
		}
		private string GetSitePageVersionFileName(string url)
		{
			string name = url.Replace('/', '~') + ".json";
			name = name.TrimStart('~');
			name = MiniWebSite.HostingEnvironment.ContentRootPath + StorageConfig.SitePageVersionFolder + name;
			return name;
		}

	}

}