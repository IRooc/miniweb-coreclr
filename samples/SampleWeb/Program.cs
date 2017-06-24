using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace SampleWeb
{
    public class Program
	{

		public static void Main(string[] args)
		{
			var config = new ConfigurationBuilder()
							 .AddCommandLine(args)
							 .AddEnvironmentVariables(prefix: "ASPNETCORE_")
							 .Build();

            var host = new WebHostBuilder()
                .UseConfiguration(config)
                        .UseKestrel()
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseUrls("http://127.0.0.1:5001")
						.UseIISIntegration()
                        .UseStartup<Startup>()
                        .Build();

			host.Run();
		}
	}
}
