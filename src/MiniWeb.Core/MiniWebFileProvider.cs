using System;
using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace MiniWeb.Core
{
    public class MiniWebFileProvider : IFileProvider
	{
		readonly IFileProvider _embeddedFileProvider;
		readonly ILogger _logger;
		readonly String _embeddedFilePath;

		public const string ADMIN_FILENAME = "adminview.cshtml";

		public MiniWebFileProvider(string embeddedFilePath, ILogger logger = null)
		{
			_logger = logger;
			_embeddedFileProvider = new EmbeddedFileProvider(this.GetType().GetTypeInfo().Assembly, this.GetType().Namespace);
			_embeddedFilePath = embeddedFilePath;
		}

		public IDirectoryContents GetDirectoryContents(string subpath)
		{
			return null;
		}

		public IFileInfo GetFileInfo(string subpath)
		{
			if (subpath == _embeddedFilePath + ADMIN_FILENAME)
			{
				return _embeddedFileProvider.GetFileInfo($"Resources/{ADMIN_FILENAME}");
			} 
			return null;
		}

		public IChangeToken Watch(string filter)
		{
			if (filter == _embeddedFilePath + ADMIN_FILENAME)
			{
				return _embeddedFileProvider.Watch(filter);
			}
			return null;
		}

	}
}
