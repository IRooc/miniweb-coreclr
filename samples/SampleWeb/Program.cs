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
			var config = WebApplicationConfiguration.GetDefault(args);

			var application = new WebApplicationBuilder()
				.UseConfiguration(config)
				.UseStartup<Startup>()
				//.UseUrls("http://localhost:5001")
				.Build();

			application.Run();
		}
	}
}
