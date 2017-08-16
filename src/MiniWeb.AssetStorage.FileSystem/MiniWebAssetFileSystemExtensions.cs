using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiniWeb.Core;

namespace MiniWeb.AssetStorage.FileSystem
{
    public static class MiniWebAssetFileSystemExtensions
    {
		public static IServiceCollection AddMiniWebAssetFileSystemStorage(this IServiceCollection services, IConfigurationRoot configuration)
		{
			services.Configure<MiniWebAssetFileSystemConfig>(configuration.GetSection("MiniWebStorage"));
			return services.AddSingleton<IMiniWebAssetStorage, MiniWebAssetFileSystemStorage>();
		}
    }
}
