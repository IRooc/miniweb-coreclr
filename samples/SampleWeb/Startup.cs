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
using MiniWeb.AssetStorage.FileSystem;
using MiniWeb.Core;
using MiniWeb.Storage.JsonStorage;
using MiniWeb.Storage.XmlStorage;
using MiniWeb.Storage.EFStorage;
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
					.AddMiniWebEFSqlServerStorage(Configuration)
					.AddMiniWebAssetFileSystemStorage(Configuration);
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

			//Registers base cookie authentication method. Do this when you need to register "other" authentications
			app.UseMiniWebSiteCookieAuth();

			//setup other authentications
			var githubConfig = new GithubAuthConfig();
			Configuration.GetSection("GithubAuth").Bind(githubConfig);
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
			app.UseEFMiniWebSite(false);
			//app.UseMiniWebSite(false);
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