using Microsoft.AspNetCore.Hosting;
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
    public partial class Program
    {
        public static async Task Main(string[] args)
        {
            var config = GeneratorConfig.ParseConfig("-c", @"C:\Rc\Temp\wwwContent", "-o", @"C:\Rc\Temp\output\", "--replace");
            if (args.Length > 0)
            {
                config = GeneratorConfig.ParseConfig(args);
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
}
