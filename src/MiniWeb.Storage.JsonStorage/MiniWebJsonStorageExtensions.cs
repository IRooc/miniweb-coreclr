using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using MiniWeb.Core;

namespace MiniWeb.Storage.JsonStorage
{
    public static class MiniWebJsonStorageExtensions
	{
		public static IServiceCollection ConfigureMiniWebJsonStorage(this IServiceCollection services, IConfiguration Configuration)
		{
			return services.ConfigureMiniWeb<MiniWebJsonStorage, MiniWebJsonStorageConfig>(Configuration);
        }
	}
}
