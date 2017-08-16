using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiniWeb.Core;

namespace MiniWeb.Storage.EFStorage
{
	public static class MiniWebEFStorageExtensions
	{
		public static IServiceCollection AddMiniWebEFSqlServerStorage(this IServiceCollection services, IConfigurationRoot configuration)
		{		
			services.AddDbContext<MiniWebEFDbContext>();
			services.Configure<MiniWebEFStorageConfig>(configuration.GetSection("MiniWebStorage"));
			return services.AddSingleton<IMiniWebContentStorage, MiniWebEFStorage>();
		}

		public static IApplicationBuilder UseEFMiniWebSite(this IApplicationBuilder app)
		{
			//validate db is created
			app.ApplicationServices.GetRequiredService<MiniWebEFDbContext>().Database.EnsureCreated();

			return app.UseMiniWebSite();
		}
	}
}
