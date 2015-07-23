using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;
using Microsoft.Framework.Runtime;

namespace MiniWeb.Core
{
	public static class MiniWebExtensions
	{
		/// <summary>
		/// Registers the miniweb Mvc Routes with default config
		/// </summary>
		/// <param name="app"></param>
		/// <returns></returns>
		public static IApplicationBuilder UseMiniWebSite(this IApplicationBuilder app)
		{
			return UseMiniWebSite(app, new MiniWebConfiguration());
		}
		/// <summary>
		/// Registers the miniweb Mvc Routes
		/// </summary>
		/// <param name="app"></param>
		/// <returns></returns>
		public static IApplicationBuilder UseMiniWebSite(this IApplicationBuilder app, MiniWebConfiguration config, ILogger logger = null)
		{
			app.UseCookieAuthentication(options =>
			{
				options.LoginPath = new PathString(config.LoginPath);
				options.LogoutPath = new PathString(config.LogoutPath);
				options.AuthenticationScheme = config.AuthenticationScheme;
				options.AutomaticAuthentication = true;

			});

			app.UseMiddleware<MiniWebAdminMiddleware>();

			app.UseMvc(routes =>
			{
				routes.MapRoute("miniwebapi", "miniweb-api/{action}", new { controller = "MiniWebApi" });
				routes.MapRoute("miniweblogin", config.LoginPath.Substring(1), new { controller = "MiniWebPage", action = "Login" });
				routes.MapRoute("miniweblogout", config.LogoutPath.Substring(1), new { controller = "MiniWebPage", action = "Logout" });
				routes.MapRoute("miniweb", "{*url}", new { controller = "MiniWebPage", action = "Index" });
			});
			
			return app;
		}

		public static IServiceCollection ConfigureMiniWeb<T, U, V>(this IServiceCollection services, IConfiguration Configuration, IApplicationEnvironment ApplicationEnvironment)
			where T : class, IMiniWebSite
			where U : class, IMiniWebStorage
			where V : class
		{

			//Setup miniweb injection
			services.Configure<MiniWebConfiguration>(Configuration.GetConfigurationSection("MiniWeb"));
			services.Configure<V>(Configuration.GetConfigurationSection("Storage"));
			services.Configure<RazorViewEngineOptions>(options =>
			{
				//make sure embedded view is returned when needed
				options.FileProvider = new MiniWebFileProvider(ApplicationEnvironment);
			});
			services.AddSingleton<IMiniWebStorage, U>();
			services.AddSingleton<IMiniWebSite, T>();
			return services;
		}
	}	
}