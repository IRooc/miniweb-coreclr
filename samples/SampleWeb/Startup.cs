using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.Options;
using MiniWeb.Core;
using Newtonsoft.Json.Linq;
using MiniWeb.Storage.JsonStorage;

namespace SampleWeb
{

	public class Startup
	{
		public IConfiguration Configuration { get; set; }

		public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
		{
			// Setup configuration sources.
			var configuration = new ConfigurationBuilder()
								.SetBasePath(appEnv.ApplicationBasePath)
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

			services.Configure<GithubAuthConfig>(Configuration.GetSection("GithubAuth"));
			//services.AddMiniWebEFSqlServerStorage(Configuration);
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
			app.UseDeveloperExceptionPage();
			app.UseStaticFiles();


			var miniwebConfig = app.GetMiniWebConfig();

			//Registers base cookie authentication method. Do this when you need to register "other" authentications
			app.UseMiniWebSiteCookieAuth();

			//setup other authentications
			var githubConfig = app.GetConcreteOptions<GithubAuthConfig>();
			app.UseOAuthAuthentication(new OAuthOptions 
			{
				AuthenticationScheme = "Github-Auth",
				DisplayName = "Login with GitHub account",
				ClientId = githubConfig.ClientId,
				ClientSecret = githubConfig.ClientSecret,
				CallbackPath = new PathString(githubConfig.CallbackPath),
				AuthorizationEndpoint = "https://github.com/login/oauth/authorize",
				TokenEndpoint = "https://github.com/login/oauth/access_token",
				UserInformationEndpoint = "https://api.github.com/user",
				ClaimsIssuer = miniwebConfig.Authentication.AuthenticationType,
				SignInScheme = miniwebConfig.Authentication.AuthenticationScheme,
				SaveTokensAsClaims = false,
				Events = new OAuthEvents()
				{
					OnCreatingTicket = async notification =>
					{
						var request = new HttpRequestMessage(HttpMethod.Get, notification.Options.UserInformationEndpoint);
						request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", notification.AccessToken);
						request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

						var response = await notification.Backchannel.SendAsync(request, notification.HttpContext.RequestAborted);
						response.EnsureSuccessStatusCode();
						var user = JObject.Parse(await response.Content.ReadAsStringAsync());

						var loginName = user.Value<string>("login");

						//Check allowed users here
						var adminList = (githubConfig.AllowedAdmins ?? string.Empty).Split(',');
						if (adminList?.Any(item => item == loginName) == true)
						{
							var claims = new[] {
								new Claim(ClaimTypes.Name, loginName),
								new Claim(ClaimTypes.Role, MiniWebAuthentication.MiniWebCmsRoleValue)
							};
							notification.Identity.AddClaims(claims);
						}
					}
				}
			});

			//Registers the miniweb middleware and MVC Routes, do not re-register cookieauth
			//app.UseEFMiniWebSite(false);
			app.UseMiniWebSite(false);
		}

	}

	public static class ConfigExtensions
	{
		public static T GetConcreteOptions<T>(this IApplicationBuilder app) where T : class, new()
		{
			return app.ApplicationServices.GetRequiredService<IOptions<T>>().Value;
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