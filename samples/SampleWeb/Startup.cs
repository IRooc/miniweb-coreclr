using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniWeb.AssetStorage.FileSystem;
using MiniWeb.Core;
using MiniWeb.Storage.JsonStorage;
//using MiniWeb.Storage.XmlStorage;
//using MiniWeb.Storage.EFStorage;
using Newtonsoft.Json.Linq;

namespace SampleWeb
{

	public class Startup
	{
		public IConfigurationRoot Configuration { get; set; }

		public Startup(IHostingEnvironment env)
		{
			// Setup configuration sources.
			var configuration = new ConfigurationBuilder()
								.SetBasePath(env.ContentRootPath)
								.AddJsonFile("miniweb.json")
								.AddJsonFile("githubauth.json")
								.AddJsonFile($"miniweb.{env.EnvironmentName}.json", optional: true)
								.AddEnvironmentVariables();

			Configuration = configuration.Build();
		}

		public void ConfigureServices(IServiceCollection services)
		{
			// Default services used by MiniWeb
			services.AddAntiforgery();
			services.AddMvc();



			//services.AddMiniWeb(Configuration).AddMiniWebEFSqlServerStorage(Configuration);
			services.AddMiniWeb(Configuration)
					.AddMiniWebJsonStorage(Configuration)
					.AddMiniWebAssetFileSystemStorage(Configuration);

			MiniWebAuthentication authConfig = Configuration.Get<MiniWebConfiguration>().Authentication;
			
			services.AddAuthentication(c =>
			{
				c.DefaultScheme = authConfig.AuthenticationScheme;
			})
			.AddCookie(authConfig.AuthenticationScheme, o =>
			{
				o.LoginPath = new PathString(authConfig.LoginPath);
				o.LogoutPath = new PathString(authConfig.LogoutPath);
			});
		}

		public void Configure(IApplicationBuilder app, ILoggerFactory loggerfactory)
		{
			// Add the loggers.
			if (Configuration.Value<bool>("Logging:EnableConsole"))
			{
				loggerfactory.AddConsole(LogLevel.Information);
			}

			app.UseDeveloperExceptionPage();
			app.UseStaticFiles();


			var miniwebConfig = app.GetMiniWebConfig();
			
			//Registers the miniweb middleware and MVC Routes, do not re-register cookieauth
			//app.UseEFMiniWebSite(false);
			app.UseMiniWebSite();
		}

	}

	public static class ConfigExtensions
	{
		public static T Value<T>(this IConfiguration configuration, string key)
		{
			try
			{
				return (T)System.Convert.ChangeType(configuration[key], typeof(T)); ;
			}
			catch
			{
				return default(T);
			}
		}
	}

	public class GithubAuthConfig
	{
		public string ClientId { get; set; }
		public string ClientSecret { get; set; }
		public string CallbackPath { get; set; }
		public string AllowedAdmins { get; set; }
	}
}