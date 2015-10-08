﻿using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Framework.DependencyInjection;

namespace MiniWeb.Storage.EFStorage
{
	public class Startup
	{
		public IConfiguration Config { get; set; }

		public Startup(IHostingEnvironment env)
		{
			var config = new ConfigurationBuilder()
				.AddEnvironmentVariables();

			Config = config.Build();
		}

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddEntityFramework()
				.AddSqlServer()
				.AddDbContext<MiniWebEFDbContext>();
		}

		public void Configure(IApplicationBuilder app)
		{
			var db = app.ApplicationServices.GetRequiredService<MiniWebEFDbContext>();
			db.Database.EnsureCreated();

		}
	}
}
