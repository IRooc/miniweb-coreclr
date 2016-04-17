
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.PlatformAbstractions;

namespace SampleWeb
{
	public class Program
	{

		public static void Main(string[] args)
		{
			var host = new WebHostBuilder()
                        .UseKestrel()
                        .UseContentRoot(GetApplicationPath("wwwroot"))
                        .UseDefaultHostingConfiguration(args)
						.UseUrls("http://localhost:5001")
                        .UseStartup<Startup>()
                        .Build();

			host.Run();
		}
		private static string GetApplicationPath(string relativePath)
        {
            var applicationBasePath = PlatformServices.Default.Application.ApplicationBasePath;
            return Path.GetFullPath(Path.Combine(applicationBasePath, relativePath));
        }
	}
}
