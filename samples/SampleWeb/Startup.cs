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
        public IWebHostEnvironment Environment { get; set; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            // Setup configuration sources.
            Configuration = configuration;
            Environment = env;
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

            services.AddMiniWeb(Configuration, Environment)
                    .AddMiniWebJsonStorage(Configuration)
            //        .AddMiniWebXmlStorage(Configuration)
                    .AddMiniWebAssetFileSystemStorage(Configuration);

            MiniWebAuthentication authConfig = Configuration.Get<MiniWebConfiguration>().Authentication;
            var githubConfig = new GithubAuthConfig();
            Configuration.GetSection("GithubAuth").Bind(githubConfig);

            services.AddAuthentication(c =>
            {
                c.DefaultScheme = authConfig.AuthenticationScheme;
            })
            .AddCookie(authConfig.AuthenticationScheme, o =>
            {
                o.LoginPath = new PathString(authConfig.LoginPath);
                o.LogoutPath = new PathString(authConfig.LogoutPath);
            })
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
                            var claims = new[] {
                                new Claim(ClaimTypes.Name, loginName),
                                new Claim(ClaimTypes.Role, MiniWebAuthentication.MiniWebCmsRoleValue)
                            };
                            notification.Identity.AddClaims(claims);
                        }
                    }
                };
            });
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerfactory)
        {
            // Add the loggers.
            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();
            
            app.UseHttpsRedirection();

            //current hosting needs this
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