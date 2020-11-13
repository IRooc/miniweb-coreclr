using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiniWeb.Core;

namespace MiniWeb.Storage.EFStorage
{
	public static class MiniWebEFStorageExtensions
	{
		public static IServiceCollection AddMiniWebEFSqlServerStorage(this IServiceCollection services, IConfiguration configuration)
		{		
			services.AddDbContext<MiniWebEFDbContext>();
			services.Configure<MiniWebEFStorageConfig>(configuration.GetSection("MiniWebStorage"));
			return services.AddScoped<IMiniWebContentStorage, MiniWebEFStorage>();
		}

		public static IApplicationBuilder UseEFMiniWebSite(this IApplicationBuilder app)
		{

			return app.UseMiniWebSite();
		}
	}
}
