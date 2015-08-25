using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNet.Authentication.OAuth;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
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
			if (Configuration.GetValue<bool>("Logging:EnableConsole"))
			{
				loggerfactory.AddConsole(LogLevel.Information);
			}

			if (Configuration.GetValue<bool>("Logging:EnableFile"))
			{
				loggerfactory.AddProvider(new FileLoggerProvider((category, logLevel) => logLevel >= LogLevel.Information,
																	  appEnv.ApplicationBasePath + "/logfile.txt"));
			}
			app.UseErrorPage();
			app.UseStaticFiles();


			var miniwebConfig = app.GetMiniWebConfig();

			//Registers base cookie authentication method.
			app.UseMiniWebSiteCookieAuth();

			//setup other authentications
			app.UseOAuthAuthentication("Github-Account", options =>
			{
				options.Caption = "Login with GitHub account";
				options.ClientId = Configuration["GithubAuth:ClientId"];
				options.ClientSecret = Configuration["GithubAuth:ClientSecret"];
				options.CallbackPath = new PathString(Configuration["GithubAuth:CallbackPath"]);
				options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
				options.TokenEndpoint = "https://github.com/login/oauth/access_token";
				options.UserInformationEndpoint = "https://api.github.com/user";
				options.ClaimsIssuer = miniwebConfig.Authentication.AuthenticationType;
				options.SignInScheme = miniwebConfig.Authentication.AuthenticationScheme;
				options.SaveTokensAsClaims = false;
				options.Notifications = new OAuthAuthenticationNotifications()
				{
					OnAuthenticated = async notification =>
					{
						var request = new HttpRequestMessage(HttpMethod.Get, notification.Options.UserInformationEndpoint);
						request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", notification.AccessToken);
						request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

						var response = await notification.Backchannel.SendAsync(request, notification.HttpContext.RequestAborted);
						response.EnsureSuccessStatusCode();
						var user = JObject.Parse(await response.Content.ReadAsStringAsync());

						var loginName = user.Value<string>("login");

						//Check allowed users here
						var adminList = (Configuration["GithubAuth:AllowedAdmins"] ?? string.Empty).Split(',');
						if (adminList?.Any(item => item == loginName) == true)
						{
							var claims = new[] {
								new Claim(ClaimTypes.Name, loginName),
								new Claim(ClaimTypes.Role, MiniWebAuthentication.MiniWebCmsRoleValue)
							};
							notification.Identity.AddClaims(claims);
						}
					}
				};
			});

			//Registers the miniweb middleware and MVC Routes, not not reregester cookieauth
			app.UseMiniWebSite(false);

		}
	}

	public static class ConfigExtensions {
		public static T GetValue<T>(this IConfiguration configuration, string key)
		{
			try
			{
				return (T)System.Convert.ChangeType(configuration.GetSection(key).Value, typeof(T)); ;
			}
			catch
			{
				return default(T);
			}
		}
	}
}
