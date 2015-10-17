using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using MiniWeb.Core;

namespace MiniWeb.Storage.JsonStorage
{
	public static class MiniWebJsonStorageExtensions
	{
		public static IServiceCollection AddMiniWebJsonStorage(this IServiceCollection services, IConfiguration Configuration)
		{
			return services.AddMiniWebJsonStorage<MiniWebSite>(Configuration);
		}
		public static IServiceCollection AddMiniWebJsonStorage<T>(this IServiceCollection services, IConfiguration Configuration)
			where T : class, IMiniWebSite
		{
			return services.AddMiniWeb<T, MiniWebJsonStorage, MiniWebJsonStorageConfig>(Configuration);
		}
	}
}
