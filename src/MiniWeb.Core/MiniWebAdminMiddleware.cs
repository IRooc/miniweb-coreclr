using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;

namespace MiniWeb.Core
{
	public class MiniWebAdminMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly IMiniWebSite _miniWebSite;
		private readonly EmbeddedFileProvider _provider;

		public MiniWebAdminMiddleware(RequestDelegate next, IMiniWebSite miniWeb)
		{
			_miniWebSite = miniWeb;
			_next = next;
			_provider = new EmbeddedFileProvider(this.GetType().GetTypeInfo().Assembly, this.GetType().Namespace);
		}

		public async Task Invoke(HttpContext context)
		{
			var path = context.Request.Path.Value;
			//TODO(RC): fix application paths with ~/
			if (_miniWebSite.IsAuthenticated(context.User) && path.StartsWith(_miniWebSite.Configuration.EmbeddedResourcePath))
			{
				var resource = Path.GetFileName(path);

				var fileInfo = _provider.GetFileInfo($"Resources/{resource}");
				if (fileInfo.Exists)
				{
					using (var stream = fileInfo.CreateReadStream())
					{
						StreamReader reader = new StreamReader(stream);
						//TODO(RC): read from file?
						string contentType = "text/css";
						if (resource.EndsWith(".js"))
						{
							contentType = "application/javascript";
						}
						_miniWebSite.Logger?.LogVerbose($"Embedded: {resource}");
						context.Response.ContentType = contentType;
						await context.Response.WriteAsync(reader.ReadToEnd());
					}
				}
				else
				{
					_miniWebSite.Logger?.LogWarning($"Embedded resource not found: {resource}");
					context.Response.StatusCode = 404;
					await context.Response.WriteAsync("404:" + resource);
				}
			}
			else
			{
				await _next(context);
			}
		}

	}
}
