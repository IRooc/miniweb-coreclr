using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MiniWeb.Core
{
    public class MiniWebRouteConstraint : IRouteConstraint
	{
		private readonly bool _force;
		private RegexRouteConstraint _regexConstaint;
		public MiniWebRouteConstraint(string extension, bool force)
		{
			if (!string.IsNullOrEmpty(extension)){
				_regexConstaint = new RegexRouteConstraint($".*?\\.{extension}(\\?.*)?");
			}

			this._force = force;
		}

        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
			if (_force && _regexConstaint != null) {
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
		

		/// <summary>
		/// Registers the miniweb Mvc Routes and Custom Middleware
		/// </summary>
		/// <param name="app"></param>
		/// <param name="registerCookieAuth"></param>
		/// <returns></returns>
		public static IApplicationBuilder UseMiniWebSite(this IApplicationBuilder app)
		{
			MiniWebConfiguration config = app.GetMiniWebConfig();

			app.UseAuthentication();

			app.UseMiddleware<MiniWebAdminMiddleware>();

			app.UseMvc(routes =>
			{
				routes.MapRoute("miniwebapi", $"{config.ApiEndpoint.Substring(1)}{{action}}", new { controller = "MiniWebApi" });
				routes.MapRoute("miniwebsociallogin", config.Authentication.SocialLoginPath.Substring(1), new { controller = "MiniWebPage", action = "SocialLogin" });
				routes.MapRoute("miniweblogin", config.Authentication.LoginPath.Substring(1), new { controller = "MiniWebPage", action = "Login" });
				routes.MapRoute("miniweblogout", config.Authentication.LogoutPath.Substring(1), new { controller = "MiniWebPage", action = "Logout" });
				routes.MapRoute("miniweb", "{*url}", new { controller = "MiniWebPage", action = "Index" }, constraints: new { url = new MiniWebRouteConstraint(config.PageExtension, config.PageExtensionForce) });
			});

			return app;
		}


		public static IServiceCollection AddMiniWeb(this IServiceCollection services, IConfigurationRoot configuration)
		{
			return services.AddMiniWeb<MiniWebSite>(configuration);
		}

		public static IServiceCollection AddMiniWeb<T1>(this IServiceCollection services, IConfigurationRoot configuration)
			where T1 : class, IMiniWebSite
		{
			//Setup miniweb configuration
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
			services.AddSingleton<IMiniWebSite, T1>();
			return services;
		}
	}
	
}