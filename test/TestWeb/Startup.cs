using Microsoft.AspNet.Builder;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;
using Microsoft.Framework.Runtime;
using MiniWeb.Core;
using MiniWeb.Storage;
using System.Reflection;
using Microsoft.Framework.Caching;
using System;

namespace aspnet5Web
{

	public class Startup
	{
		public IConfiguration Configuration { get; set; }
		public IApplicationEnvironment ApplicationEnvironment { get; set; }

        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
		{
			ApplicationEnvironment = appEnv;
			// Setup configuration sources.
			var configuration = new ConfigurationBuilder(appEnv.ApplicationBasePath)
											.AddJsonFile("miniweb.json")
											.AddJsonFile($"miniweb.{env.EnvironmentName}.json", optional: true)
											.AddEnvironmentVariables();

			Configuration = configuration.Build();
		}

		public void ConfigureServices(IServiceCollection services)
		{
			// Default services used by MiniWeb
			services.AddAuthentication();
			services.AddAntiforgery();
			services.AddMvc();


			services.ConfigureMiniWeb<MiniWebSite, MiniWebJsonStorage, MiniWebJsonStorageConfig>
									(Configuration, ApplicationEnvironment);

		}

		public void Configure(IApplicationBuilder app, ILoggerFactory loggerfactory, 
									 IApplicationEnvironment appEnv, IOptions<MiniWebConfiguration> config)
		{
			// Add the loggers.
			if (Configuration.Get("Logging:EnableConsole") == true.ToString())
				loggerfactory.AddConsole(LogLevel.Information);

			if (Configuration.Get("Logging:EnableFile") == true.ToString())
				loggerfactory.AddProvider(new Web.FileLoggerProvider((category, logLevel) => logLevel >= LogLevel.Information,
																	  appEnv.ApplicationBasePath + "/logfile.txt"));

			app.UseErrorPage();
			app.UseStaticFiles();

			//Registers the miniweb middleware and MVC Routes
			app.UseMiniWebSite(config.Options);

		}
	}

	
}