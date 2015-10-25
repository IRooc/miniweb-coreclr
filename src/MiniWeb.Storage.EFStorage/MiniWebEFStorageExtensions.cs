using Microsoft.AspNet.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiniWeb.Core;

namespace MiniWeb.Storage.EFStorage
{
	public static class MiniWebEFStorageExtensions
	{
		public static IServiceCollection AddMiniWebEFSqlServerStorage(this IServiceCollection services, IConfiguration configuration)
		{
			return services.AddMiniWebEFSqlServerStorage<MiniWebSite>(configuration);
		}
		public static IServiceCollection AddMiniWebEFSqlServerStorage<T>(this IServiceCollection services, IConfiguration configuration)
			where T : class, IMiniWebSite
		{
			services.AddEntityFramework().AddSqlServer().AddDbContext<MiniWebEFDbContext>();
			return services.AddMiniWeb<T, MiniWebEFStorage, MiniWebEFStorageConfig>(configuration);
		}

		public static IApplicationBuilder UseEFMiniWebSite(this IApplicationBuilder app, bool registerCookie = true)
		{
			//validate db is created
			app.ApplicationServices.GetRequiredService<MiniWebEFDbContext>().Database.EnsureCreated();

			return app.UseMiniWebSite(registerCookie);
		}
	}
}
