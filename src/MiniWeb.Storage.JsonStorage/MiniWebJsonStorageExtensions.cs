using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiniWeb.Core;

namespace MiniWeb.Storage.JsonStorage
{
	public static class MiniWebJsonStorageExtensions
	{
		public static IServiceCollection AddMiniWebJsonStorage(this IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<MiniWebJsonStorageConfig>(configuration.GetSection("MiniWebStorage"));
			return services.AddSingleton<IMiniWebContentStorage, MiniWebJsonStorage>();
		}
	}
}
