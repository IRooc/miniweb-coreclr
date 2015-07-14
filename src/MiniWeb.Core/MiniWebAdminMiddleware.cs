using Microsoft.AspNet.Builder;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.Framework.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MiniWeb.Core
{
	public class MiniWebAdminMiddleware
	{

		private readonly RequestDelegate _next;
		private readonly IMiniWebSite _miniWebSite;
		private readonly IHostingEnvironment _hostingEnv;

		public MiniWebAdminMiddleware(RequestDelegate next, IMiniWebSite miniWeb, IHostingEnvironment hostingEnv)
		{
			_miniWebSite = miniWeb;
			_next = next;
			_hostingEnv = hostingEnv;
		}

		public async Task Invoke(HttpContext context)
		{
			var path = context.Request.Path.Value;
			if (_miniWebSite.IsAuthenticated(context.User) && path.StartsWith("/miniweb-resource/"))
			{
				var provider = new EmbeddedFileProvider(this.GetType().GetTypeInfo().Assembly, this.GetType().Namespace);

				var resource = Path.GetFileName(path);
				
				var fileInfo = provider.GetFileInfo($"Resources/{resource}");
				if (fileInfo.Exists)
				{
					using (var stream = fileInfo.CreateReadStream())
					{
						StreamReader reader = new StreamReader(stream);
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
