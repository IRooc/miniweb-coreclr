using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.DependencyInjection;

namespace SampleWeb
{
	public class Program
	{

		public static void Main(string[] args)
		{
			var host = new WebHostBuilder()
       				 .UseDefaultConfiguration(args)
     				 .UseServer("Microsoft.AspNet.Server.Kestrel")
			         .UseUrls("http://localhost:5001")
			         .UseStartup<Startup>()
			         .Build();

			host.Run();
		}
	}
}
