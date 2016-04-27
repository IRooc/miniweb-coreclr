using System;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace MiniWeb.Core
{
	public class MiniWebFileProvider : IFileProvider
	{
		readonly IFileProvider _physicalFileProvider;
		readonly IFileProvider _embeddedFileProvider;
		readonly ILogger _logger;
		readonly String _embeddedFilePath;

		public const string ADMIN_FILENAME = "adminview.cshtml";

		public MiniWebFileProvider(IHostingEnvironment hostingEnv, String embeddedFilePath, ILogger logger = null)
		{
			_logger = logger;
			_physicalFileProvider = new PhysicalFileProvider(hostingEnv.ContentRootPath);
			_embeddedFileProvider = new EmbeddedFileProvider(this.GetType().GetTypeInfo().Assembly, this.GetType().Namespace);
			_embeddedFilePath = embeddedFilePath;
		}

		public IDirectoryContents GetDirectoryContents(string subpath)
		{
			_logger?.LogInformation($"GetDirectoryContents {subpath}");
			return _physicalFileProvider.GetDirectoryContents(subpath);
		}

		public IFileInfo GetFileInfo(string subpath)
		{
			if (subpath == _embeddedFilePath + ADMIN_FILENAME)
			{
				return _embeddedFileProvider.GetFileInfo($"Resources/{ADMIN_FILENAME}");
			} 
			return  _physicalFileProvider.GetFileInfo(subpath);
		}

		public IChangeToken Watch(string filter)
		{
			_logger?.LogInformation($"Watch {filter}");
			if (filter == _embeddedFilePath + ADMIN_FILENAME)
			{
				return _embeddedFileProvider.Watch(filter);
			}
			return _physicalFileProvider.Watch(filter);
		}

	}
}
