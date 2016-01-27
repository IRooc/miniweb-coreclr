using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

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
