using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using Microsoft.Framework.OptionsModel;
using MiniWeb.Core;

namespace MiniWeb.Storage.EFStorage
{
	public class MiniWebEFDbContext : DbContext
	{
		public DbSet<DbSitePage> Pages { get; set; }
		public DbSet<DbPageSection> Sections { get; set; }
		public DbSet<DbContentItem> ContentItems { get; set; }

		private MiniWebEFStorageConfig Configuration { get; set; }

		public MiniWebEFDbContext(IOptions<MiniWebEFStorageConfig> options)
		{
			Configuration = options.Value;
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.Entity<DbSitePage>().Key(s => s.Url);
			builder.Entity<DbPageSection>().Key(s => s.Key);
			builder.Entity<DbContentItem>().Key(c => c.Id);
			base.OnModelCreating(builder);
		}
	}

	public class DbSitePage
	{
		public string Url { get; set; }
		public string Title { get; set; }
		public string Template { get; set; }
		public string Layout { get; set; }
		public string MetaTitle { get; set; }
		public string MetaDescription { get; set; }

		public bool Visible { get; set; }
		public bool ShowInMenu { get; set; }
		public int SortOrder { get; set; }
		public DateTime Created { get; set; }
		public DateTime LastModified { get; set; }

		public List<DbPageSection> Sections { get; set; } 
	}

	public class DbPageSection
	{
		public string Key { get; set; }
		public List<DbContentItem> ContenItems { get; set; }

		public string DbPageUrl { get; set; }
		public DbSitePage DbPage { get; set; }

	}

	public class DbContentItem
	{
		public Guid Id { get; set; }
		public string Template { get; set; }
		public string Values { get; set; } 


		public string DbPageSectionKey { get; set; }
		public DbPageSection Section { get; set; }
	}
}
