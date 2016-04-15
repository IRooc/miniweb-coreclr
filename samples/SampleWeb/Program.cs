
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace SampleWeb
{
	public class Program
	{

		public static void Main(string[] args)
		{
			var host = new WebHostBuilder()
                        .UseKestrel()
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseDefaultHostingConfiguration(args)
						.UseUrls("http://localhost:5001")
                        .UseStartup<Startup>()
                        .Build();

			host.Run();
		}
	}
}
