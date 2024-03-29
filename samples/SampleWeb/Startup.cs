using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniWeb.AssetStorage.FileSystem;
using MiniWeb.Core;
//TODO: Decide you storage type (also check the csproj file
using MiniWeb.Storage.JsonStorage;
//using MiniWeb.Storage.XmlStorage;
//using MiniWeb.Storage.EFStorage;
using Newtonsoft.Json.Linq;
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
			services.AddMvc(); 

			services.AddMiniWeb(Configuration)
					//TODO: Sample website uses Json storage comment out this line if you pick another provider
                    .AddMiniWebJsonStorage(Configuration)
					// If you want SQL backend uncomment this line, you need to manually add a User in the database
                    //        .AddMiniWebEFSqlServerStorage(Configuration)
					// If you want XML storage uncomment this line
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

		public void Configure(IApplicationBuilder app)
		{
			//show this always for now...
			app.UseDeveloperExceptionPage();

			//use static assets
			app.UseStaticFiles();
			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();
						
			app.UseEndpoints(endpoints =>
            {
                //Registers the miniweb Routes,
                endpoints.MapMiniWebSite();
			});
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