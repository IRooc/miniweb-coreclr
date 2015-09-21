using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using MiniWeb.Core;

namespace MiniWeb.Storage.EFStorage
{
	public static class MiniWebEFStorageExtensions
	{
		public static IServiceCollection AddMiniWebEFStorage(this IServiceCollection services, IConfiguration configuration)
		{
			return services.AddMiniWebEFStorage<MiniWebSite>(configuration);
		}
		public static IServiceCollection AddMiniWebEFStorage<T>(this IServiceCollection services, IConfiguration configuration)
			where T : class, IMiniWebSite
		{
			services.AddEntityFramework().AddSqlServer().AddDbContext<MiniWebEFDbContext>();
			return services.AddMiniWeb<T, MiniWebEFStorage, MiniWebEFStorageConfig>(configuration);
		}
	}
}
