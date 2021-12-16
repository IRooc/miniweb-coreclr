using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MiniWeb.Core;

namespace MiniWeb.Storage.EFStorage
{
    public class MiniWebEFDbContext : DbContext
	{
		public DbSet<DbSitePage> Pages { get; set; }
		public DbSet<DbContentItem> ContentItems { get; set; }
		public DbSet<DbUser> Users { get; set; }
		private MiniWebEFStorageConfig Configuration { get; set; }

		public MiniWebEFDbContext(IOptions<MiniWebEFStorageConfig> options)
		{
			Configuration = options.Value;
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer(Configuration.Connectionstring);
			base.OnConfiguring(optionsBuilder);
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.Entity<DbSitePage>().HasKey(s => s.Url);
			builder.Entity<DbSitePage>().Ignore(s => s.BaseUrl);
			builder.Entity<DbSitePage>().Ignore(s => s.Pages);
			builder.Entity<DbSitePage>().Ignore(s => s.Parent);
			builder.Entity<DbSitePage>().Ignore(s => s.Sections);
			builder.Entity<DbContentItem>().HasKey(c => new { c.PageUrl, c.Sortorder, c.SectionKey });
			builder.Entity<DbUser>().HasKey(u => u.UserName);
			base.OnModelCreating(builder);
		}
	}
}
