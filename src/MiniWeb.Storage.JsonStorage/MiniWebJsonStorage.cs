using Microsoft.AspNet.Hosting;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;
using MiniWeb.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MiniWeb.Storage.JsonStorage
{
	public class MiniWebJsonStorage : IMiniWebStorage
	{
		private MiniWebJsonStorageConfig StorageConfig { get; }
		public IMiniWebSite MiniWebSite { get; set; }

		public MiniWebJsonStorage(IOptions<MiniWebJsonStorageConfig> config)
		{
			StorageConfig = config.Value;
		}

		public bool Authenticate(string username, string password)
		{
			return StorageConfig.Users.Any(u => string.Compare(u.Key, username, true) == 0 && string.Compare(u.Value, password) == 0);
		}

		public SitePage GetSitePageByUrl(string url)
		{
			string name = GetSitePageFileName(url.ToLowerInvariant());
			if (File.Exists(name))
			{
				return DeSerializeFile<SitePage>(name);
			}
			return null;
		}


		public void StoreSitePage(SitePage sitePage)
		{
			string name = GetSitePageFileName(sitePage.Url.ToLowerInvariant());
			if (MiniWebSite.Configuration.StoreVersions && File.Exists(name))
			{
				string lowerCaseUrl = sitePage.Url.ToLowerInvariant();
				string version = GetSitePageVersionFileName($"{lowerCaseUrl}[{sitePage.LastModified.Ticks}]");

				File.Copy(name, version);
			}
			SerializeObject(name, sitePage);
		}

		public void DeleteSitePage(SitePage sitePage)
		{
			string name = GetSitePageFileName(sitePage.Url.ToLowerInvariant());
			File.Delete(name);
		}

		public IEnumerable<SitePage> AllPages()
		{
			List<SitePage> pages = new List<SitePage>();
			if (Directory.Exists(MiniWebSite.AppEnvironment.ApplicationBasePath + StorageConfig.SitePageFolder))
			{
				string[] pageFiles = Directory.GetFiles(MiniWebSite.AppEnvironment.ApplicationBasePath + StorageConfig.SitePageFolder, "*.json");
				foreach (string page in pageFiles)
				{
					MiniWebSite.Logger?.LogVerbose($"Loading page from disc {page}");
					pages.Add(DeSerializeFile<SitePage>(page));
				}
			}
			else
			{
				pages.Add(new SitePage { Title = MiniWebSite.AppEnvironment.ApplicationBasePath, Url = MiniWebSite.Configuration.DefaultPage });
			}
			return pages;
		}

		private void SerializeObject(string filename, object obj)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				string fileContent = JsonConvert.SerializeObject(obj);
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
				return JsonConvert.DeserializeObject<T>(jsonString);

			}
			catch (Exception ex)
			{
				MiniWebSite.Logger?.LogWarning($"Could not deserialize file: {filename}", ex);
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
			name = MiniWebSite.AppEnvironment.ApplicationBasePath + StorageConfig.SitePageFolder + name;
			return name;
		}
		private string GetSitePageVersionFileName(string url)
		{
			string name = url.Replace('/', '~') + ".json";
			name = name.TrimStart('~');
			name = MiniWebSite.AppEnvironment.ApplicationBasePath + StorageConfig.SitePageVersionFolder + name;
			return name;
		}
	}

}