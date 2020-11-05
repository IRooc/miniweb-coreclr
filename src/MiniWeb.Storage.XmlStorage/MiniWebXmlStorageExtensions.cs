using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiniWeb.Core;

namespace MiniWeb.Storage.XmlStorage
{
	public static class MiniWebXmlStorageExtensions
	{
		public static IServiceCollection AddMiniWebXmlStorage(this IServiceCollection services, IConfiguration configuration)
		{		
			services.Configure<MiniWebXmlStorageConfig>(configuration.GetSection("MiniWebStorage"));
			return services.AddSingleton<IMiniWebContentStorage, MiniWebXmlStorage>();
		}
	}
}
