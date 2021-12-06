using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ObjectPool;
using MiniWeb.AssetStorage.FileSystem;
using MiniWeb.Core;
using MiniWeb.Storage.JsonStorage;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MiniWeb.StaticSiteGenerator
{
    public class Program
    {
        public static async Task Main(string[] args)
        {

            var config = ParseConfig("-c", @"C:\Rc\Temp\wwwContent", "-o", @"C:\Rc\Temp\output\", "--replace");
            if (args.Length > 0)
            {
                config = ParseConfig(args);
            }

            Console.WriteLine($"Starting generation with config");
            Console.WriteLine($"{JsonConvert.SerializeObject(config, Formatting.Indented)}");


            var configBuilder = new ConfigurationBuilder();
            var configuration = configBuilder.AddJsonFile("miniweb.json", optional: true, reloadOnChange: true).Build();

            var builder = new HostBuilder()
              .ConfigureWebHost(h =>
              {
                  h.UseContentRoot(config.ContentRoot);
              })
              .ConfigureServices(services =>
              {
                  services.AddLogging();

                  services.AddMiniWeb(configuration)
                          .AddMiniWebJsonStorage(configuration)
                          .AddMiniWebAssetFileSystemStorage(configuration);

                  services.AddMvc().AddRazorRuntimeCompilation();
                  services.AddTransient<RazorViewToStringRenderer>();

              });

            var host = builder.Build();

            using (var serviceScope = host.Services.CreateScope())
            {
                var helper = serviceScope.ServiceProvider.GetRequiredService<RazorViewToStringRenderer>();
                var miniweb = serviceScope.ServiceProvider.GetRequiredService<IMiniWebSite>();
                if (config.ReplaceOutput && Directory.Exists(config.OutputFolder))
                {
                    Console.WriteLine($"Removing folder {config.OutputFolder}");
                    Directory.Delete(config.OutputFolder, true);
                }
                var redirects = new Dictionary<string, string>();
                var pages = await miniweb.Pages(true);
                Console.WriteLine($"Processing {pages.Count(p => p.Visible)} pages");
                foreach (var page in pages.OrderBy(p => p.Url))
                {
                    if (!page.Visible) continue;
                    if (!string.IsNullOrWhiteSpace(page.RedirectUrl))
                    {
                        redirects.Add(page.Url, page.RedirectUrl);
                        Console.WriteLine($" - {page.Url} => {page.RedirectUrl}");
                    }
                    else
                    {
                        var pageresult = await helper.RenderViewToStringAsync(page.Template, page);
                        var folder = Path.Combine(config.OutputFolder, Path.GetDirectoryName(page.Url) ?? string.Empty);
                        var file = Path.GetFileName(page.Url) + "." + miniweb.Configuration.PageExtension;
                        Directory.CreateDirectory(folder);
                        File.WriteAllText(Path.Combine(folder, file), pageresult);
                        Console.WriteLine($" - {page.Url}");
                    }
                }
                Console.WriteLine($"Copy static content {config.ContentRoot}/wwwroot to {config.OutputFolder}");
                DirectoryInfo diSource = new DirectoryInfo($"{config.ContentRoot}/wwwroot");
                DirectoryInfo diTarget = new DirectoryInfo(config.OutputFolder);
                CopyAll(diSource, diTarget);

                Console.WriteLine($"Creating static staticwebapp config");
                var staticWebsiteConfig = new StaticWebAppConfig
                {
                    navigationFallback = new NavigationFallback
                    {
                        rewrite = miniweb.Configuration.DefaultPage
                    },
                    routes = redirects.Select((kv) => new NavigationRoute
                    {
                        route = kv.Key,
                        rewrite = kv.Value + "." + miniweb.Configuration.PageExtension
                    }).ToArray(),
                    responseOverrides = new Dictionary<string, NavigationOverride>
                    {
                        ["404"] = new NavigationOverride
                        {
                            rewrite = (await miniweb.ContentStorage.MiniWeb404Page()).Url + "." + miniweb.Configuration.PageExtension
                        }
                    }
                };
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                };
                File.WriteAllText($"{config.OutputFolder}/staticwebapp.config.json", JsonConvert.SerializeObject(staticWebsiteConfig, settings));
            }
            Console.WriteLine($"Done");
        }

        public class GeneratorConfig
        {

            [ConsoleArgument("-c")]
            public string ContentRoot { get; set; } = string.Empty;

            [ConsoleArgument("-o")]
            public string OutputFolder { get; set; } = string.Empty;

            [ConsoleArgument("--replace")]
            public bool ReplaceOutput { get; set; }

        }

        public static GeneratorConfig ParseConfig(params string[] args)
        {
            var config = new GeneratorConfig();

            foreach (var prop in config.GetType().GetProperties())
            {
                foreach (var attr in prop.GetCustomAttributes(false))
                {
                    if (attr is ConsoleArgumentAttribute consoleArgument)
                    {
                        var index = Array.IndexOf(args, consoleArgument.CommandPrefix);
                        if (index == -1)
                        {
                            index = Array.IndexOf(args, $"-{prop.Name.ToLower()}");
                        }
                        if (index != -1)
                        {
                            if (prop.PropertyType == typeof(bool))
                            {
                                if (index + 1 < args.Length && !args[index + 1].StartsWith("-"))
                                    prop.SetValue(config, bool.Parse(args[index + 1]));
                                else
                                    prop.SetValue(config, true);
                            }
                            else if (index + 1 < args.Length && !args[index + 1].StartsWith("-"))
                            {
                                prop.SetValue(config, args[index + 1]);
                            }
                        }
                    }
                }
            }

            return config;
        }



        //MSDN: https://docs.microsoft.com/en-us/azure/static-web-apps/configuration
        public class StaticWebAppConfig
        {
            public NavigationRoute[]? routes { get; set; }
            public NavigationFallback? navigationFallback { get; set; }
            public Dictionary<string, NavigationOverride>? responseOverrides { get; set; }
            public Dictionary<string, string>? globalHeaders { get; set; }
            public Dictionary<string, string>? mimeTypes { get; set; }
        }

        public class NavigationFallback
        {
            public string? rewrite { get; set; }
            public string[]? exclude { get; set; }
        }

        public class NavigationOverride
        {
            public string? rewrite { get; set; }
            public string? redirect { get; set; }
            public int? statusCode { get; set; }
        }


        public class NavigationRoute
        {
            public string? route { get; set; }
            public string[]? allowedRoles { get; set; }
            public Dictionary<string, string>? headers { get; set; }
            public string[]? methods { get; set; }
            public string? rewrite { get; set; }
            public int? statusCode { get; set; }
            public string? redirect { get; set; }
        }




        //MSDN: https://docs.microsoft.com/en-us/dotnet/api/system.io.directoryinfo?view=net-6.0
        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                if (fi.Name == "web.config") continue;

                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                if (diSourceSubDir.Name == "miniweb-resources") continue;

                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }


    }


    internal class ConsoleArgumentAttribute : Attribute
    {
        public ConsoleArgumentAttribute(string prefix)
        {
            CommandPrefix = prefix;
        }

        public string CommandPrefix { get; }
    }


    //https://github.com/aspnet/samples/blob/ae4ae5d560c1feca18818c0d696cd1dc89163fd4/samples/aspnetcore/mvc/renderviewtostring/RazorViewToStringRenderer.cs
    public class RazorViewToStringRenderer
    {
        private IRazorViewEngine _viewEngine;
        private ITempDataProvider _tempDataProvider;
        private IServiceProvider _serviceProvider;

        public RazorViewToStringRenderer(
            IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }

        public async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model)
        {
            var actionContext = GetActionContext();
            var view = FindView(actionContext, viewName);

            using (var output = new StringWriter())
            {
                var viewContext = new ViewContext(
                        actionContext,
                        view,
                        new ViewDataDictionary<TModel>(new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()), model),
                        new TempDataDictionary(
                            actionContext.HttpContext,
                            _tempDataProvider),
                        output,
                        new HtmlHelperOptions());

                await view.RenderAsync(viewContext);

                return output.ToString();
            }
        }

        private IView FindView(ActionContext actionContext, string viewName)
        {
            var getViewResult = _viewEngine.GetView(executingFilePath: null, viewPath: viewName, isMainPage: true);
            if (getViewResult.Success)
            {
                return getViewResult.View;
            }

            var findViewResult = _viewEngine.FindView(actionContext, viewName, isMainPage: true);
            if (findViewResult.Success)
            {
                return findViewResult.View;
            }

            var searchedLocations = getViewResult.SearchedLocations.Concat(findViewResult.SearchedLocations);
            var errorMessage = string.Join(
                Environment.NewLine,
                new[] { $"Unable to find view '{viewName}'. The following locations were searched:" }.Concat(searchedLocations)); ;

            throw new InvalidOperationException(errorMessage);
        }

        private ActionContext GetActionContext()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = _serviceProvider;
            return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        }
    }
}
