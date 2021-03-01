using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
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
//using MiniWeb.Storage.XmlStorage;
//using MiniWeb.Storage.EFStorage;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using System;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace SampleWeb
{
	public class Startup
	{
		public IConfiguration Configuration { get; set; }
		public Startup(IConfiguration configuration)
		{
			// Setup configuration sources.
			Configuration = configuration;
		}

		public void ConfigureServices(IServiceCollection services)
		{
			// Default services used by MiniWeb
			services.AddAntiforgery();
			var builder = services.AddMvc(options =>
			{
				options.EnableEndpointRouting = false;  //for now...
			})
			.AddRazorRuntimeCompilation(); //needed for miniweb for now.

			services.AddMiniWeb(Configuration)
					.AddMiniWebJsonStorage(Configuration)
					//        .AddMiniWebXmlStorage(Configuration)
					.AddMiniWebAssetFileSystemStorage(Configuration);

			var authConfig = Configuration.Get<MiniWebConfiguration>().Authentication;
			var githubConfig = new GithubAuthConfig();
			Configuration.GetSection("GithubAuth").Bind(githubConfig);

			services.AddMiniwebBasicAuth(Configuration)
			.AddOAuth("Github-Auth", "Login with GitHub account", o =>
			{
				o.ClientId = githubConfig.ClientId;
				o.ClientSecret = githubConfig.ClientSecret;
				o.CallbackPath = new PathString(githubConfig.CallbackPath);
				o.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
				o.TokenEndpoint = "https://github.com/login/oauth/access_token";
				o.UserInformationEndpoint = "https://api.github.com/user";
				o.SignInScheme = authConfig.AuthenticationScheme;
				o.Events = new OAuthEvents()
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
							var claims = MiniWebSite.GetClaimsFor(loginName);
							notification.Identity.AddClaims(claims);
						}
					}
				};
			});
		}

		public void Configure(IApplicationBuilder app, ILoggerFactory loggerfactory)
		{
			//show this always for now...
			app.UseDeveloperExceptionPage();

			//use static assets
			app.UseStaticFiles();
			app.UseHttpsRedirection();

			//current hosting needs this ignore otherwise
			app.Map("/emonitor.aspx", context =>
			{
				context.Run(async ctx =>
				{
					ctx.Response.ContentType = "text/plain";
					await ctx.Response.WriteAsync("Enterprise Monitor test ASP");
				});
			});

			//Registers the miniweb middleware and MVC Routes, do not re-register cookieauth
			//app.UseEFMiniWebSite(false);
			app.UseMiniWebSite();
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