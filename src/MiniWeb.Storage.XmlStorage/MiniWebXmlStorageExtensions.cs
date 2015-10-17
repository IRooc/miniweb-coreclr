using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using MiniWeb.Core;

namespace MiniWeb.Storage.XmlStorage
{
	public static class MiniWebXmlStorageExtensions
	{
		public static IServiceCollection AddMiniWebXmlStorage(this IServiceCollection services, IConfiguration Configuration)
		{
			return services.AddMiniWebXmlStorage<MiniWebSite>(Configuration);
		}

		public static IServiceCollection AddMiniWebXmlStorage<T>(this IServiceCollection services, IConfiguration Configuration)
			where T : class, IMiniWebSite
		{
			return services.AddMiniWeb<T, MiniWebXmlStorage, MiniWebXmlStorageConfig>(Configuration);
		}
	}
}
