
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
			var contentRoot = PlatformServices.Default.Application.ApplicationBasePath;
			
			var host = new WebHostBuilder()
						.UseKestrel()
						.UseContentRoot(contentRoot)
						.UseDefaultHostingConfiguration(args)
						.UseUrls("http://localhost:5001")
						.UseStartup<Startup>()
						.Build();

			host.Run();
		}
	}
}
