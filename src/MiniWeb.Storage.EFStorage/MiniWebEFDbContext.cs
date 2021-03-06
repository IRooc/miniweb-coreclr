﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
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

	public class DbSitePage : ISitePage
	{
		public string Url { get; set; }
		public string RedirectUrl { get; set; }
		public string Title { get; set; }
		public string Template { get; set; }
		public string Layout { get; set; }
		public string MetaTitle { get; set; }
		public string MetaDescription { get; set; }

		public bool Visible { get; set; }
		public bool ShowInMenu { get; set; }
		public int SortOrder { get; set; }
		public DateTime? Date { get; set; }
		public DateTime Created { get; set; }
		public DateTime LastModified { get; set; }

		public List<DbContentItem> Items { get; set; }

		[IgnoreDataMember]
		public string BaseUrl
		{
			get
			{
				return Url.Split('/')[0];
			}
		}

		[IgnoreDataMember]
		public IEnumerable<ISitePage> Pages { get; set; } = new List<DbSitePage>();

		[IgnoreDataMember]
		public ISitePage Parent { get; set; }

		[IgnoreDataMember]
		public List<IPageSection> Sections { get; set; }

		public string GetBodyCss()
		{
			var bodyCss = Url.Replace("/", "-");

			return bodyCss;
		}

		public bool VisibleInMenu()
		{
			return Visible && ShowInMenu;
		}
		public bool IsActiveFor(string url)
		{
			return url.StartsWith(this.Url + "/") || url == this.Url;
		}
	}


	public class DbContentItem
	{
		public int Sortorder { get; set; }
		public string SectionKey { get; set; }

		public string Template { get; set; }
		public string Values { get; set; }

		public string PageUrl { get; set; }
		public DbSitePage Page { get; set; }
	}

	public class DbUser
	{
		public string UserName { get; set; }
		public string Password { get; set; }
		public string Salt { get; set; }
		public bool Active { get; set; }
	}


	public class EFPageSection : IPageSection
	{
		public string Key { get; set; }
		public List<IContentItem> Items { get; set; }
	}

	public class EFContentItem : IContentItem
	{
		public ISitePage Page { get; set; }
		public string Template { get; set; }
		public Dictionary<string, string> Values { get; set; } = new Dictionary<string, string>();

		public string GetValue(string value, string defaultvalue = "")
		{
			if (Values.ContainsKey(value))
			{
				return Values[value];
			}
			return defaultvalue;
		}

		public T Get<T>(string value, T defaultvalue = default(T))
		{
			try
			{
				return (T)Convert.ChangeType(GetValue(value, Convert.ToString(defaultvalue)), typeof(T));
			}
			catch
			{
				return default(T);
			}
		}

	}
}
