using System;
using System.Reflection;
using Microsoft.Dnx.Runtime;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNet.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.OptionsModel;

namespace MiniWeb.Core
{
	public class MiniWebFileProvider : IFileProvider
	{
		readonly IFileProvider _physicalFileProvider;
		readonly IFileProvider _embeddedFileProvider;
		readonly ILogger _logger;
		readonly String _embeddedFilePath;

		public const string ADMIN_FILENAME = "adminview.cshtml";

		public MiniWebFileProvider(IApplicationEnvironment applicationEnvironment, String embeddedFilePath, ILogger logger = null)
		{
			_logger = logger;
			_physicalFileProvider = new PhysicalFileProvider(applicationEnvironment.ApplicationBasePath);
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
			var fileInfo = _physicalFileProvider.GetFileInfo(subpath);
			if (subpath == _embeddedFilePath + ADMIN_FILENAME)
			{
				fileInfo = _embeddedFileProvider.GetFileInfo($"Resources/adminview.cshtml");
				if (!fileInfo.Exists)
				{
					//mac linux view workaround
					fileInfo = _physicalFileProvider.GetFileInfo("/wwwroot" + subpath);
				}
			}

			return fileInfo;
		}

		public IChangeToken Watch(string filter)
		{
			_logger?.LogInformation($"Watch {filter}");
			if (filter == _embeddedFilePath + ADMIN_FILENAME)
			{
				return IgnoreTrigger.Singleton;
			}
			return _physicalFileProvider.Watch(filter);
		}


		/// <summary>
		/// gotten from NoobTrigger example in Microsoft.AspNet.FileProviders 
		/// </summary>
		internal class IgnoreTrigger : IChangeToken
		{
			public static IgnoreTrigger Singleton { get; } = new IgnoreTrigger();

			private IgnoreTrigger()
			{
			}

			public bool HasChanged
			{
				get { return false; }
			}

			public bool ActiveChangeCallbacks
			{
				get { return false; }
			}
			
			public IDisposable RegisterChangeCallback(Action<object> callback, object state)
			{
				throw new InvalidOperationException("Trigger does not support registering change notifications.");
			}
		}
	}
}
