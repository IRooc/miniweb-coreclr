using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiniWeb.Core;

namespace MiniWeb.AssetStorage.FileSystem
{
    public static class MiniWebAssetFileSystemExtensions
    {
		public static IServiceCollection AddMiniWebAssetFileSystemStorage(this IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<MiniWebAssetFileSystemConfig>(configuration.GetSection("MiniWebStorage"));
			return services.AddSingleton<IMiniWebAssetStorage, MiniWebAssetFileSystemStorage>();
		}
    }
}
