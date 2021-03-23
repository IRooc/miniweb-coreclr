using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MiniWeb.Core;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

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
			Context.Database.EnsureCreated();
		}

		public Task<IEnumerable<ISitePage>> AllPages()
		{
			IEnumerable<ISitePage> result = Enumerable.Empty<ISitePage>();
			if (Context.Pages.Any())
			{
				result = Context.Pages.Include(p => p.Items).Select(GetSitePage);
			}
			return Task.FromResult(result);
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


		public async Task<bool> Authenticate(string username, string password)
		{
			//TODO(RC):Fix hashing and stuff...
			var user = await Context.Users.FirstOrDefaultAsync(u => u.UserName == username && u.Active);
			return user?.Password == password;
		}

		public async Task DeleteSitePage(ISitePage sitePage)
		{
			Context.RemoveRange(Context.ContentItems.Where(c => c.PageUrl == sitePage.Url));
			Context.Remove(Context.Pages.First(p => p.Url == sitePage.Url));
			await Context.SaveChangesAsync();
		}

		public async Task<ISitePage> GetSitePageByUrl(string url)
		{
			return await Context.Pages.Where(p => p.Url == url).Select(p => GetSitePage(p)).SingleOrDefaultAsync();
		}

		public async Task StoreSitePage(ISitePage sitePage, HttpRequest currentRequest)
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
				await Context.SaveChangesAsync();

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
			await Context.SaveChangesAsync();
		}


		public Task<List<IPageSection>> GetDefaultSectionContent(DefaultContent defaultContent)
		{
			var result = defaultContent?.Content?.Select(c => new PageSection()
			{
				Key = c.Section,
				Items = c.Items?.Select(i => new ContentItem()
				{
					Template = i,
					Values = new Dictionary<string, string>()
				}).ToList<IContentItem>()
			}).ToList<IPageSection>();
			return Task.FromResult(result);
		}

		public Task<ISitePage> Deserialize(string filecontent)
		{
			throw new NotImplementedException();
		}

		public async Task<ISitePage> MiniWeb404Page()
		{
			return (await AllPages()).FirstOrDefault(p => p.Url == "404") ?? new DbSitePage()
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

		public Task<ISitePage> MiniWebLoginPage()
		{
			var result = new DbSitePage()
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
			return Task.FromResult<ISitePage>(new DbSitePage());
		}
	}
}
