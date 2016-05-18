using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MiniWeb.Storage.EFStorage
{
	///Only used for the commands...
	// public class Startup
	// {
	// 	public IConfiguration Config { get; set; }

	// 	public Startup(IHostingEnvironment env)
	// 	{
	// 		var config = new ConfigurationBuilder()
	// 			.AddEnvironmentVariables();

	// 		Config = config.Build();
	// 	}

	// 	public void ConfigureServices(IServiceCollection services)
	// 	{
	// 		services.AddEntityFramework()
	// 			.AddSqlServer()
	// 			.AddDbContext<MiniWebEFDbContext>();
	// 	}

	// 	public void Configure(IApplicationBuilder app)
	// 	{
	// 		var db = app.ApplicationServices.GetRequiredService<MiniWebEFDbContext>();
	// 		db.Database.EnsureCreated();

	// 	}
	// }
}
