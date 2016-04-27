using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiniWeb.Core;

namespace MiniWeb.Storage.XmlStorage
{
	public static class MiniWebXmlStorageExtensions
	{
		public static IServiceCollection AddMiniWebXmlStorage(this IServiceCollection services, IConfigurationRoot Configuration)
		{
			return services.AddMiniWebXmlStorage<MiniWebSite>(Configuration);
		}

		public static IServiceCollection AddMiniWebXmlStorage<T>(this IServiceCollection services, IConfigurationRoot Configuration)
			where T : class, IMiniWebSite
		{
			return services.AddMiniWeb<T, MiniWebXmlStorage, MiniWebXmlStorageConfig>(Configuration);
		}
	}
}
