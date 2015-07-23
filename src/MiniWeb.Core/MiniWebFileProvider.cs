using Microsoft.AspNet.FileProviders;
using Microsoft.Framework.Caching;
using Microsoft.Framework.Logging;
using Microsoft.Framework.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MiniWeb.Core
{
    public class MiniWebFileProvider : IFileProvider
	{
		IFileProvider _physicalFileProvider;
		ILogger _logger;
		public const string ADMIN_FILENAME = "/miniweb-resource/adminview.cshtml";

		public MiniWebFileProvider(IApplicationEnvironment applicationEnvironment, ILogger logger = null)
		{
			_logger = logger;
			_physicalFileProvider = new PhysicalFileProvider(applicationEnvironment.ApplicationBasePath);
		}

		public IDirectoryContents GetDirectoryContents(string subpath)
		{
			_logger?.LogInformation($"GetDirectoryContents {subpath}");
			return _physicalFileProvider.GetDirectoryContents(subpath);
		}

		public IFileInfo GetFileInfo(string subpath)
		{
			_logger?.LogInformation($"GetFileInfo {subpath}");
			if (subpath == ADMIN_FILENAME)
			{
				var provider = new EmbeddedFileProvider(this.GetType().GetTypeInfo().Assembly, this.GetType().Namespace);
				return provider.GetFileInfo($"Resources/adminview.cshtml");
			}

			return _physicalFileProvider.GetFileInfo(subpath);
		}

		public IExpirationTrigger Watch(string filter)
		{
			_logger?.LogInformation($"Watch {filter}");
			if (filter == ADMIN_FILENAME)
			{
				return IgnoreTrigger.Singleton;
			}
			return _physicalFileProvider.Watch(filter);
		}


		/// <summary>
		/// gotten from NoobTrigger example in Microsoft.AspNet.FileProviders 
		/// </summary>
		internal class IgnoreTrigger : IExpirationTrigger
		{
			public static IgnoreTrigger Singleton { get; } = new IgnoreTrigger();

			private IgnoreTrigger()
			{
			}

			public bool ActiveExpirationCallbacks
			{
				get { return false; }
			}

			public bool IsExpired
			{
				get { return false; }
			}

			public IDisposable RegisterExpirationCallback(Action<object> callback, object state)
			{
				throw new InvalidOperationException("Trigger does not support registering change notifications.");
			}
		}
	}
}
