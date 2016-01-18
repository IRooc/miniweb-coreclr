using System.Security.Claims;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.Dnx.Runtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;

namespace MiniWeb.Core
{
	public static class MiniWebExtensions
	{
		public static MiniWebConfiguration GetMiniWebConfig(this IApplicationBuilder app)
		{
			return  app.ApplicationServices.GetRequiredService<IOptions<MiniWebConfiguration>>().Value;
		}

		public static IApplicationBuilder UseMiniWebSiteCookieAuth(this IApplicationBuilder app)
		{
			MiniWebAuthentication authConfig = app.GetMiniWebConfig().Authentication;
			app.UseCookieAuthentication(new CookieAuthenticationOptions()
			{
				LoginPath = new PathString(authConfig.LoginPath),
				LogoutPath = new PathString(authConfig.LogoutPath),
				AuthenticationScheme = authConfig.AuthenticationScheme,
				AutomaticAuthenticate = true

			});
			return app;
		}

		/// <summary>
		/// Registers the miniweb Mvc Routes and Custom Middleware
		/// </summary>
		/// <param name="app"></param>
		/// <param name="registerCookieAuth"></param>
		/// <returns></returns>
		public static IApplicationBuilder UseMiniWebSite(this IApplicationBuilder app, bool registerCookieAuth = true)
		{
			MiniWebConfiguration config = app.GetMiniWebConfig();
			if (registerCookieAuth)
			{
				app.UseMiniWebSiteCookieAuth();
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


		public static IServiceCollection AddMiniWeb<T1, T2>(this IServiceCollection services, IConfiguration configuration)
			where T1 : class, IMiniWebStorage
			where T2 : class, IMiniWebStorageConfiguration
		{
			return services.AddMiniWeb<MiniWebSite, T1, T2>(configuration);
		}

		public static IServiceCollection AddMiniWeb<T1, T2, T3>(this IServiceCollection services, IConfiguration configuration)
			where T1 : class, IMiniWebSite
			where T2 : class, IMiniWebStorage
			where T3 : class, IMiniWebStorageConfiguration
		{
			//Setup miniweb configuration
			services.Configure<T3>(configuration.GetSection("MiniWebStorage"));
			services.Configure<MiniWebConfiguration>(configuration.GetSection("MiniWeb"));

			//how to do this in ConfigureServices??
			string embeddedFilePath = configuration.GetSection("MiniWeb:EmbeddedResourcePath")?.Value ?? new MiniWebConfiguration().EmbeddedResourcePath;

			//make sure embedded view is returned when needed
			var appEnv = services.BuildServiceProvider().GetService<IApplicationEnvironment>();
			services.Configure<RazorViewEngineOptions>(options => { options.FileProviders.Insert(0, new MiniWebFileProvider(appEnv, embeddedFilePath)); });

			services.AddAuthorization(options =>
			{
				options.AddPolicy(MiniWebAuthentication.MiniWebCmsRoleValue, policyBuilder =>
				{
					policyBuilder.RequireClaim(ClaimTypes.Role, MiniWebAuthentication.MiniWebCmsRoleValue);
				});
			});

			//Setup miniweb injection
			services.AddSingleton<IMiniWebStorage, T2>();
			services.AddSingleton<IMiniWebSite, T1>();
			return services;
		}
	}
}