using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MiniWeb.Core;
using Newtonsoft.Json;

namespace MiniWeb.Storage.EFStorage
{
	public class MiniWebEFStorage : IMiniWebContentStorage
	{
		private MiniWebEFDbContext Context { get; set; }
		private MiniWebEFStorageConfig StorageConfig { get; set; }
		public IMiniWebSite MiniWebSite { get; set; }

		public MiniWebEFStorage(MiniWebEFDbContext context, IOptions<MiniWebEFStorageConfig> options)
		{
			StorageConfig = options.Value;		
			Context = context;
		}

		public IEnumerable<ISitePage> AllPages()
		{
			if (Context.Pages.Any())
			{
				return Context.Pages.Include(p => p.Items).Select(GetSitePage);
			}
			return new List<ISitePage>() { this.MiniWeb404Page };
		}

		private static ISitePage GetSitePage(DbSitePage p)
		{
			//make sure Sections Collection is Set
			p.Sections = p.Items?.GroupBy(i => i.SectionKey).Select(g => new PageSection()
				{
					Key = g.Key,
					Items = g.Select(i => new ContentItem()
					{
						Template = i.Template,
						Page = p,
						Values = JsonConvert.DeserializeObject<Dictionary<string, string>>(i.Values)
					}).ToList<IContentItem>() ?? new List<IContentItem>()
				}).ToList<IPageSection>() ?? new List<IPageSection>();
			return p;
		}

		public bool Authenticate(string username, string password)
		{
			//TODO(RC):Fix hashing and stuff...
			var user = Context.Users.FirstOrDefault(u => u.UserName == username && u.Active);
			return user?.Password == password;
		}

		public void DeleteSitePage(ISitePage sitePage)
		{
			Context.RemoveRange(Context.ContentItems.Where(c => c.PageUrl == sitePage.Url));
			Context.Remove(Context.Pages.First(p => p.Url == sitePage.Url));
			Context.SaveChanges();
		}

		public ISitePage GetSitePageByUrl(string url)
		{
			return Context.Pages.Where(p => p.Url == url).Select(p => GetSitePage(p)).SingleOrDefault();
		}

		public void StoreSitePage(ISitePage sitePage)
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
		
		
		public List<IPageSection> GetDefaultSectionContent(DefaultContent defaultContent)
		{
			return defaultContent?.Content?.Select(c => new PageSection()
			{
				Key = c.Section,
				Items = c.Items?.Select(i => new ContentItem()
				{
					Template = i,
					Values = new Dictionary<string, string>()
				}).ToList<IContentItem>()
			}).ToList<IPageSection>();
		}
		
		public ISitePage MiniWeb404Page
		{
			get
			{
				return AllPages().FirstOrDefault(p => p.Url == "404") ?? new DbSitePage()
				{
					Title = "Page Not Found : 404",
					MetaTitle = "Page Not Found : 404",
					Layout = StorageConfig.Layout,
					Template = $"~{StorageConfig.PageTemplatePath}/OneColumn.cshtml",
					Visible = true,
					Sections = new List<IPageSection>()
					{
						new PageSection()
						{
							Key = "content",
							Items = new List<IContentItem>()
							{
								new ContentItem {
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
		}

		public ISitePage MiniWebLoginPage
		{
			get
			{
				return new DbSitePage()
				{
					Title = "Login",
					MetaTitle = "Login",
					Layout = StorageConfig.Layout,
					Template = $"~{StorageConfig.PageTemplatePath}/OneColumn.cshtml",
					Sections = new List<IPageSection>(),
					Url = "miniweb/login",
					Visible = true
				};
			}
		}

		public JsonConverter JsonInterfaceConverter
		{
			get
			{
				return new JsonInterfaceConverter();
			}
		}
	}
}
