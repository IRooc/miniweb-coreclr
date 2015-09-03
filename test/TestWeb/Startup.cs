using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;
using MiniWeb.Core;
using MiniWeb.Storage.JsonStorage;
using Newtonsoft.Json.Linq;

namespace aspnet5Web
{

	public class Startup
	{
		public IConfiguration Configuration { get; set; }

		public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
		{
			// Setup configuration sources.
			var configuration = new ConfigurationBuilder(appEnv.ApplicationBasePath)
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

			services.AddMiniWebJsonStorage(Configuration);


		}

		public void Configure(IApplicationBuilder app, ILoggerFactory loggerfactory, IApplicationEnvironment appEnv)
		{
			// Add the loggers.
			if (Configuration.Value<bool>("Logging:EnableConsole"))
			{
				loggerfactory.AddConsole(LogLevel.Information);
			}

			if (Configuration.Value<bool>("Logging:EnableFile"))
			{
				loggerfactory.AddProvider(new FileLoggerProvider((category, logLevel) => logLevel >= LogLevel.Information,
																	  appEnv.ApplicationBasePath + "/logfile.txt"));
			}
			app.UseErrorPage();
			app.UseStaticFiles();


			var miniwebConfig = app.GetMiniWebConfig();

			
			app.UseMiniWebSite();

		}
	}

	public static class ConfigExtensions
	{
		public static T GetConcreteOptions<T>(this IApplicationBuilder app) where T : class, new()
		{
			return app.ApplicationServices.GetRequiredService<IOptions<T>>().Options;
		}

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
