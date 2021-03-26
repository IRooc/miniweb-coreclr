using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MiniWeb.Core;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MiniWeb.Storage.XmlStorage
{
	public class MiniWebXmlStorage : IMiniWebContentStorage
	{
		private MiniWebXmlStorageConfig StorageConfig { get; }
		public IMiniWebSite MiniWebSite { get; set; }

		public MiniWebXmlStorage(IOptions<MiniWebXmlStorageConfig> config)
		{
			StorageConfig = config.Value;
		}

		public Task<bool> Authenticate(string username, string password)
		{
			var result = StorageConfig.Users.Any(u => string.Compare(u.Key, username, true) == 0 && string.Compare(u.Value, password) == 0);
			return Task.FromResult(result);
		}

		public Task<ISitePage> GetSitePageByUrl(string url)
		{
			string name = GetSitePageFileName(url.ToLowerInvariant());
			ISitePage result = null;
			if (File.Exists(name))
			{
				result = DeSerializeFile<XmlSitePage>(name);
			}
			return Task.FromResult(result);
		}

		public Task<ISitePage> Deserialize(string filecontent)
		{
			throw new NotImplementedException();
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
			List<ISitePage> pages = new List<ISitePage>();
			if (Directory.Exists(MiniWebSite.HostingEnvironment.ContentRootPath + StorageConfig.SitePageFolder))
			{
				string[] pageFiles = Directory.GetFiles(MiniWebSite.HostingEnvironment.ContentRootPath + StorageConfig.SitePageFolder, "*.xml");
				foreach (string page in pageFiles)
				{
					MiniWebSite.Logger?.LogInformation($"Loading page from disc {page}");
					pages.Add(DeSerializeFile<XmlSitePage>(page));
				}
			}
			return Task.FromResult<IEnumerable<ISitePage>>(pages);
		}

		public Task<List<IPageSection>> GetDefaultSectionContent(DefaultContent defaultContent)
		{
			var result = defaultContent?.Content?.Select(c => new XmlPageSection()
			{
				Key = c.Section,
				Items = c.Items?.Select(i => new XmlContentItem()
				{
					Template = i,
					Values = new Dictionary<string, string>()
				}).ToList<IContentItem>()
			}).ToList<IPageSection>();
			return Task.FromResult<List<IPageSection>>(result);
		}

		public Task<IPageSection> GetPageSection(SitePageSectionPostModel section)
		{
			var result = new XmlPageSection
			{
				Key = section.Key,
				Items = section.Items.Select(i => new XmlContentItem
				{
					Template = i.Template,
					Values = i.Values
				}).ToList<IContentItem>()
			};
			return Task.FromResult<IPageSection>(result);
		}



		public async Task<ISitePage> MiniWeb404Page()
		{
			var result = (await AllPages()).FirstOrDefault(p => p.Url == "404") ?? new XmlSitePage()
			{
				Title = "Page Not Found : 404",
				MetaTitle = "Page Not Found : 404",
				Layout = StorageConfig.Layout,
				Template = $"~{StorageConfig.PageTemplatePath}/OneColumn.cshtml",
				Visible = true,
				Sections = new List<IPageSection>()
					{
						new XmlPageSection()
						{
							Key = "content",
							Items = new List<IContentItem>()
							{
								new XmlContentItem {
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
			return result;
		}

		public Task<ISitePage> MiniWebLoginPage()
		{
			var result = new XmlSitePage()
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
		public Task<ISitePage> NewPage()
		{
			return Task.FromResult<ISitePage>(new XmlSitePage());
		}


		private void SerializeObject(string filename, object obj)
		{
			var serializer = GetSerializer<XmlSitePage>();
			//var serializer = new DataContractSerializer(obj.GetType(), new[] { typeof(XmlSitePage), typeof(PageSection), typeof(ContentItem) });
			using (MemoryStream ms = new MemoryStream())
			{
				serializer.WriteObject(ms, obj);
				ms.Position = 0;
				string fileContent = new StreamReader(ms).ReadToEnd();
				if (File.Exists(filename))
					File.Delete(filename);
				File.WriteAllText(filename, fileContent);
			}
		}

		private T DeSerializeFile<T>(string filename)
		{
			try
			{
				DataContractSerializer serializer = GetSerializer<T>();
				using (var stream = new FileStream(filename, FileMode.Open))
				{
					stream.Position = 0;
					T deserialized = (T)serializer.ReadObject(stream);

					return deserialized;
				}

			}
			catch (Exception ex)
			{
				MiniWebSite.Logger?.LogWarning($"Could not deserialize file: {filename}", ex);
				return default(T);
			}

		}

		private static DataContractSerializer GetSerializer<T>()
		{
			return new DataContractSerializer(typeof(T), new[] { typeof(XmlSitePage), typeof(XmlPageSection), typeof(XmlContentItem) });
		}

		/// <summary>
		/// Gets the name of the site page file.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <returns></returns>
		private string GetSitePageFileName(string url)
		{
			string name = url.Replace('/', '~') + ".xml";
			name = name.TrimStart('~');
			name = MiniWebSite.HostingEnvironment.ContentRootPath + StorageConfig.SitePageFolder + name;
			return name;
		}
		private string GetSitePageVersionFileName(string url)
		{
			string name = url.Replace('/', '~') + ".xml";
			name = name.TrimStart('~');
			name = MiniWebSite.HostingEnvironment.ContentRootPath + StorageConfig.SitePageVersionFolder + name;
			return name;
		}
	}

}