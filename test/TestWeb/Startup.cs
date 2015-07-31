using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Authentication.OAuth;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Identity;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;
using MiniWeb.Core;
using MiniWeb.Storage.JsonStorage;
using MiniWeb.Storage.XmlStorage;
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
			services.AddAuthentication();
			services.AddAntiforgery();
			services.AddMvc();

			services.AddMiniWebJsonStorage(Configuration);

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

			//Registers base cookie authentication method.
			app.UseMiniWebSiteCookieAuth(config.Options);

			//setup other authentications
			app.UseOAuthAuthentication("Github-Account", options =>
			{
				options.Caption = "Login with GitHub account";
				options.ClientId = "eb33fa51e5d1c57985da";
				options.ClientSecret = "b7f4335d15f8f620fc6f15513a017670d43e1453";
				options.CallbackPath = new PathString("/miniweb/github-auth");
				options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
				options.TokenEndpoint = "https://github.com/login/oauth/access_token";
				options.UserInformationEndpoint = "https://api.github.com/user";
				//use cookieauthtype to make issignedin() works
				options.ClaimsIssuer = IdentityOptions.ApplicationCookieAuthenticationType;
				options.SaveTokensAsClaims = false;
				options.SignInScheme = config.Options.Authentication.AuthenticationScheme;
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
						if (loginName == "IRooc")
						{
							var claims = new[] {
								new Claim(ClaimTypes.Name, loginName,
										  ClaimValueTypes.String, notification.Options.ClaimsIssuer),
								new Claim(ClaimTypes.Role, config.Options.Authentication.MiniWebCmsRoleValue,
										  ClaimValueTypes.String, notification.Options.ClaimsIssuer)
							};
							notification.Identity.AddClaims(claims);
						}
					}
				};
			});

			//Registers the miniweb middleware and MVC Routes
			app.UseMiniWebSite(config.Options);

		}
	}
}
