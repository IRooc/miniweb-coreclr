using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Data.Entity;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;

namespace MiniWeb.Storage.EFStorage
{
	public class Startup
	{
		public Microsoft.Framework.Configuration.IConfiguration Config { get; set; }

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
				.AddDbContext<MiniWebEFDbContext>(options =>
				{
					options.UseSqlServer("data source=sqldev2k12;user id=MiniWebTest;password=MiniWebTest;initial catalog=MiniWebTest");
				});
		}

		public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider)
		{
			var db = serviceProvider.GetRequiredService<MiniWebEFDbContext>();
			db.Database.EnsureCreated();
			
		}
	}
}
