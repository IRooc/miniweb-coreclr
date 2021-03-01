using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace MiniWeb.Core
{
	public class MiniWebRouteConstraint : IRouteConstraint
	{
		private readonly bool _force;
		private RegexRouteConstraint _regexConstaint;
		public MiniWebRouteConstraint(string extension, bool force)
		{
			if (!string.IsNullOrEmpty(extension))
			{
				_regexConstaint = new RegexRouteConstraint($".*?\\.{extension}(\\?.*)?");
			}

			this._force = force;
		}

		public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
		{
			if (_force && _regexConstaint != null)
			{
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
		/// <returns></returns>
		public static IApplicationBuilder UseMiniWebSite(this IApplicationBuilder app)
		{
			MiniWebConfiguration config = app.GetMiniWebConfig();

			app.UseAuthentication();

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


		public static IServiceCollection AddMiniWeb(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
		{
			return services.AddMiniWeb<MiniWebSite>(configuration, env);
		}

		public static IServiceCollection AddMiniWeb<T1>(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
			where T1 : class, IMiniWebSite
		{
			//Setup miniweb configuration
			services.Configure<MiniWebConfiguration>(configuration.GetSection("MiniWeb"));

			services.AddAuthorizationCore(options =>
			{
				options.AddPolicy(MiniWebAuthentication.MiniWebCmsRoleValue, policyBuilder =>
				{
					policyBuilder.RequireClaim(ClaimTypes.Role, MiniWebAuthentication.MiniWebCmsRoleValue);
				});
			});

			//Setup miniweb injection
			services.AddScoped<IMiniWebSite, T1>();
			return services;
		}

		public static AuthenticationBuilder AddMiniwebBasicAuth(this IServiceCollection services, IConfiguration configuration)
		{
			if (services == null)
			{
				throw new ArgumentNullException("services");
			}
			if (configuration == null)
			{
				throw new ArgumentNullException("configuration");
			}

            var authConfig = configuration.Get<MiniWebConfiguration>().Authentication;

            var result = services.AddAuthentication(c =>
            {
                c.DefaultScheme = authConfig.AuthenticationScheme;
            })
            .AddCookie(authConfig.AuthenticationScheme, o =>
            {
                o.LoginPath = new PathString(authConfig.LoginPath);
                o.LogoutPath = new PathString(authConfig.LogoutPath);
				o.ExpireTimeSpan = TimeSpan.FromMinutes(authConfig.LoginTimeoutInMinutes);
            });
			return result;
		}
	}

}