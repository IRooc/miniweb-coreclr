using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;

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

		public static IApplicationBuilder UseMiniWebSiteCookieAuth(this IApplicationBuilder app, MiniWebConfiguration config)
		{
			config.Authentication.CookieRegistered = true;
			app.UseCookieAuthentication(options =>
			{
				options.LoginPath = new PathString(config.Authentication.LoginPath);
				options.LogoutPath = new PathString(config.Authentication.LogoutPath);
				options.AuthenticationScheme = config.Authentication.AuthenticationScheme;
				options.AutomaticAuthentication = true;

			});
			return app;
		}

		/// <summary>
		/// Registers the miniweb Mvc Routes
		/// </summary>
		/// <param name="app"></param>
		/// <returns></returns>
		public static IApplicationBuilder UseMiniWebSite(this IApplicationBuilder app, MiniWebConfiguration config)
		{
			if (!config.Authentication.CookieRegistered)
			{
				app.UseMiniWebSiteCookieAuth(config);
			}

			app.UseMiddleware<MiniWebAdminMiddleware>();

			app.UseMvc(routes =>
			{
				routes.MapRoute("miniwebapi", "miniweb-api/{action}", new { controller = "MiniWebApi" });
				routes.MapRoute("miniwebsociallogin", config.Authentication.SocialLoginPath.Substring(1), new { controller = "MiniWebPage", action = "SocialLogin" });
				routes.MapRoute("miniweblogin", config.Authentication.LoginPath.Substring(1), new { controller = "MiniWebPage", action = "Login" });
				routes.MapRoute("miniweblogout", config.Authentication.LogoutPath.Substring(1), new { controller = "MiniWebPage", action = "Logout" });
				routes.MapRoute("miniweb", "{*url}", new { controller = "MiniWebPage", action = "Index" });
			});

			return app;
		}


		public static IServiceCollection AddMiniWeb<T, U>(this IServiceCollection services, IConfiguration Configuration)
			where T : class, IMiniWebStorage
			where U : class, IMiniWebStorageConfiguration
		{
			return services.AddMiniWeb<MiniWebSite, T, U>(Configuration);
		}

		public static IServiceCollection AddMiniWeb<T, U, V>(this IServiceCollection services, IConfiguration Configuration)
			where T : class, IMiniWebSite
			where U : class, IMiniWebStorage
			where V : class, IMiniWebStorageConfiguration
		{
			//Setup miniweb injection
			services.Configure<V>(Configuration.GetConfigurationSection("Storage"));
			services.Configure<MiniWebConfiguration>(Configuration.GetConfigurationSection("MiniWeb"));

			//make sure embedded view is returned when needed
			var appEnv = services.BuildServiceProvider().GetService<IApplicationEnvironment>();
			services.Configure<RazorViewEngineOptions>(options => { options.FileProvider = new MiniWebFileProvider(appEnv); });


			services.AddSingleton<IMiniWebStorage, U>();
			services.AddSingleton<IMiniWebSite, T>();
			return services;
		}
	}
}