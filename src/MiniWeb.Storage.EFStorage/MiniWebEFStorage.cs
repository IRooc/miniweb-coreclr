using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Framework.OptionsModel;
using MiniWeb.Core;
using Newtonsoft.Json;

namespace MiniWeb.Storage.EFStorage
{
	public class MiniWebEFStorage : IMiniWebStorage
	{
		private MiniWebEFDbContext Context { get; set; } 
		private MiniWebEFStorageConfig StorageConfig { get; }
		public IMiniWebSite MiniWebSite { get; set; }

		public MiniWebEFStorage(IOptions<MiniWebEFStorageConfig> config, MiniWebEFDbContext context)
		{
			StorageConfig = config.Value;
			Context = context;
		}

		public IEnumerable<SitePage> AllPages()
		{
			if (Context.Pages.Any())
				return Context.Pages.Select(p => GetSitePage(p));
			return new List<SitePage>() {GetSitePage(new DbSitePage())};
		}

		private static SitePage GetSitePage(DbSitePage p)
		{
			return new SitePage()
			{
				Url = p.Url,
				Created = p.Created,
				LastModified = p.LastModified,
				Layout = p.Layout,
				MetaDescription = p.MetaDescription,
				MetaTitle = p.MetaTitle,
				ShowInMenu = p.ShowInMenu,
				SortOrder = p.SortOrder,
				Template = p.Template,
				Title = p.Title,
				Visible = p.Visible,
				Sections = (p.Sections?.Select(s => new PageSection()
				{
					Key = s.Key,
					Items = s.ContenItems?.Select(i => new ContentItem()
					{
						Template = i.Template,
						Values = JsonConvert.DeserializeObject<Dictionary<string, string>>(i.Values)
					}).ToList() ?? new List<ContentItem>()
				}).ToList()) ?? new List<PageSection>()
			};
		}

		public bool Authenticate(string username, string password)
		{
			throw new NotImplementedException();
		}

		public void DeleteSitePage(SitePage sitePage)
		{
			Context.Remove(Context.Pages.First(p => p.Url == sitePage.Url));
			Context.SaveChanges();
		}

		public SitePage GetSitePageByUrl(string url)
		{
			return Context.Pages.Where(p => p.Url == url).Select(p => GetSitePage(p)).SingleOrDefault();
		}

		public void StoreSitePage(SitePage sitePage)
		{
			var oldPage = Context.Pages.FirstOrDefault(p => p.Url == sitePage.Url);
			if (oldPage != null)
			{
				oldPage.Url = sitePage.Url;
				oldPage.Template = sitePage.Template;
				oldPage.Layout = sitePage.Layout;
				oldPage.Title = sitePage.Title;
				oldPage.Created = sitePage.Created;
				oldPage.LastModified = DateTime.Now;
				oldPage.MetaTitle = sitePage.MetaTitle;
				oldPage.MetaDescription = sitePage.MetaDescription;
				oldPage.ShowInMenu = sitePage.ShowInMenu;
				oldPage.SortOrder = sitePage.SortOrder;
				oldPage.Visible = sitePage.Visible;
				oldPage.Sections = sitePage.Sections?.Select(s => new DbPageSection()
				{
					Key = s.Key,
					ContenItems = s.Items?.Select(i => new DbContentItem()
					{
						Template = i.Template,
						Values = string.Empty
					}).ToList()
				}).ToList();
				Context.Pages.Update(oldPage);
			}
			else
			{
				DbSitePage dbPage = new DbSitePage()
				{
					Url = sitePage.Url,
					Template = sitePage.Template,
					Layout = sitePage.Layout,
					Title = sitePage.Title,
					Created = sitePage.Created,
					LastModified = DateTime.Now,
					MetaTitle = sitePage.MetaTitle,
					MetaDescription = sitePage.MetaDescription,
					ShowInMenu = sitePage.ShowInMenu,
					SortOrder = sitePage.SortOrder,
					Visible = sitePage.Visible
				};
				Context.Pages.Add(dbPage);
			}
			Context.SaveChanges();
		}
	}
}
