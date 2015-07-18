using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;

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
		public static IApplicationBuilder UseMiniWebSite(this IApplicationBuilder app, MiniWebConfiguration config)
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

	}	
}