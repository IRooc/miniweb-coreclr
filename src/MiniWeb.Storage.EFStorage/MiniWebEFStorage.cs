using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.AspNet.Cryptography.KeyDerivation;
using Microsoft.AspNet.Identity;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Query;
using Microsoft.Framework.OptionsModel;
using MiniWeb.Core;
using Newtonsoft.Json;

namespace MiniWeb.Storage.EFStorage
{
	public class MiniWebEFStorage : IMiniWebStorage
	{
		private MiniWebEFDbContext Context { get; set; } 
		public IMiniWebSite MiniWebSite { get; set; }

		public MiniWebEFStorage(MiniWebEFDbContext context)
		{
			Context = context;
		}

		public IEnumerable<SitePage> AllPages()
		{
			if (Context.Pages.Any())
			{
				return Context.Pages.Include(p => p.Items).Select(GetSitePage);
			}
			return new List<SitePage>() {MiniWebSite.Page404};
		}

		private static SitePage GetSitePage(DbSitePage p)
		{
			var sitePage = new SitePage()
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
				Visible = p.Visible
			};
			sitePage.Sections = p.Items?.GroupBy(i => i.SectionKey).Select(g => new PageSection()
				{
					Key = g.Key,
					Items = g.Select(i => new ContentItem()
					{
						Template = i.Template,
						Page = sitePage,
						Values = JsonConvert.DeserializeObject<Dictionary<string, string>>(i.Values)
					}).ToList() ?? new List<ContentItem>()
				}).ToList() ?? new List<PageSection>();
			return sitePage;
		}

		public bool Authenticate(string username, string password)
		{
			return false;
		}

		private static void WriteNetworkByteOrder(byte[] buffer, int offset, uint value)
		{
			buffer[offset + 0] = (byte)(value >> 24);
			buffer[offset + 1] = (byte)(value >> 16);
			buffer[offset + 2] = (byte)(value >> 8);
			buffer[offset + 3] = (byte)(value >> 0);
		}

		public void DeleteSitePage(SitePage sitePage)
		{
			Context.RemoveRange(Context.ContentItems.Where(c => c.PageUrl == sitePage.Url));
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
				Context.Pages.Update(oldPage);

				//todo(rc): can this be done without?
				Context.RemoveRange(Context.ContentItems.Where(c => c.PageUrl == sitePage.Url));
				Context.SaveChanges();

				//need to clear because items are removed but still in page.items collection
				oldPage.Items.Clear();

				foreach (var section in sitePage.Sections.Where(s => s.Items.Any()))
				{
					Context.AddRange(section.Items.Select((item, index) => new DbContentItem()
					{
						Template = item.Template,
						PageUrl = sitePage.Url,
						SectionKey = section.Key,
						Sortorder = index + 1,
						Values = JsonConvert.SerializeObject(item.Values)
					}));
				}
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
				foreach (var section in sitePage.Sections.Where(s => s.Items.Any()))
				{
					Context.AddRange(section.Items.Select((item, index) => new DbContentItem()
					{
						Template = item.Template,
						PageUrl = sitePage.Url,
						SectionKey = section.Key,
						Sortorder = index + 1,
						Values = JsonConvert.SerializeObject(item.Values)
					}));
				}
			}
			Context.SaveChanges();
		}
	}
}
