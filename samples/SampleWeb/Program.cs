using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.PlatformAbstractions;

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
                        .UseContentRoot(PlatformServices.Default.Application.ApplicationBasePath)
                        .UseUrls("http://localhost:5001")
                        .UseStartup<Startup>()
                        .Build();

			host.Run();
		}
	}
}
