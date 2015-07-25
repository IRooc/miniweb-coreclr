using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using MiniWeb.Core;

namespace MiniWeb.Storage.XmlStorage
{
    public static class MiniWebXmlStorageExtensions
	{
		public static IServiceCollection AddMiniWebXmlStorage(this IServiceCollection services, IConfiguration Configuration)
		{
			return services.AddMiniWeb<MiniWebXmlStorage, MiniWebXmlStorageConfig>(Configuration);
		}
	}
}
