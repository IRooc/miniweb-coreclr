using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;

namespace MiniWeb.Core
{
	public class MiniWebRouteConstraint : IRouteConstraint
	{

		private RegexRouteConstraint _regexConstaint;
		public MiniWebRouteConstraint(string extension)
		{
			if (!string.IsNullOrEmpty(extension)){
				_regexConstaint = new RegexRouteConstraint($".*?\\.{extension}(\\?.*)?");
			}
		}

        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
			if (_regexConstaint != null) {
				object routeValue;

				if (values.TryGetValue(routeKey, out routeValue))
				{
					return routeValue == null || _regexConstaint.Match(httpContext, route, routeKey, values, routeDirection);
				}
				return false;
			}
			return true;
		}
	}

	public static class MiniWebExtensions
	{

		public static MiniWebConfiguration GetMiniWebConfig(this IApplicationBuilder app)
		{
			return app.ApplicationServices.GetRequiredService<IOptions<MiniWebConfiguration>>().Value;
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
				routes.MapRoute("miniweb", "{*url}", new { controller = "MiniWebPage", action = "Index" }, constraints: new { url = new MiniWebRouteConstraint(config.PageExtension) });
			});

			return app;
		}


		public static IServiceCollection AddMiniWeb<T1, T2>(this IServiceCollection services, IConfigurationRoot configuration)
			where T1 : class, IMiniWebContentStorage
			where T2 : class, IMiniWebStorageConfiguration
		{
			return services.AddMiniWeb<MiniWebSite, T1, T2>(configuration);
		}

		public static IServiceCollection AddMiniWeb<T1, T2, T3>(this IServiceCollection services, IConfigurationRoot configuration)
			where T1 : class, IMiniWebSite
			where T2 : class, IMiniWebContentStorage
			where T3 : class, IMiniWebStorageConfiguration
		{
			//Setup miniweb configuration
			services.Configure<T3>(configuration.GetSection("MiniWebStorage"));
			services.Configure<MiniWebConfiguration>(configuration.GetSection("MiniWeb"));
			
			//get the config locally
			var config = new MiniWebConfiguration();
			configuration.GetSection("MiniWeb").Bind(config);

			//how to do this in ConfigureServices??
			string embeddedFilePath = config.EmbeddedResourcePath;

			//make sure embedded view is returned when needed
			services.Configure<RazorViewEngineOptions>(options => 
			{ 
				options.FileProviders.Insert(0, new MiniWebFileProvider(embeddedFilePath)); 
			});

			services.AddAuthorization(options =>
			{
				options.AddPolicy(MiniWebAuthentication.MiniWebCmsRoleValue, policyBuilder =>
				{
					policyBuilder.RequireClaim(ClaimTypes.Role, MiniWebAuthentication.MiniWebCmsRoleValue);
				});
			});

			//Setup miniweb injection
			services.AddSingleton<IMiniWebContentStorage, T2>();
			services.AddSingleton<IMiniWebSite, T1>();
			return services;
		}
	}
	
}