﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MiniWeb.Core;

namespace MiniWeb.AssetStorage.FileSystem
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class MiniWebAssetFileSystemStorage : IMiniWebAssetStorage
	{
		public IMiniWebSite MiniWebSite { get; set; }
		public ILogger Logger { get; }
		public IHostingEnvironment HostingEnvironment { get; }
		public MiniWebAssetFileSystemConfig Configuration { get; }

		public MiniWebAssetFileSystemStorage(IHostingEnvironment env, ILoggerFactory loggerfactory, IOptions<MiniWebAssetFileSystemConfig> config)
		{
			HostingEnvironment = env;
			Configuration = config.Value;
			Logger = SetupLogging(loggerfactory);
		}


		public IEnumerable<IAsset> GetAllAssets()
		{
			string folder = Path.Combine(HostingEnvironment.WebRootPath, Configuration.AssetRootPath);
			if (Directory.Exists(folder))
			{
				var allFiles = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);
				return allFiles.Select(f => new FileSystemAsset(HostingEnvironment)
				{
					VirtualPath = GetVirtualPath(f)
				});
			}
			Logger?.LogWarning($"Folder not present {folder} no assets loaded");
			return Enumerable.Empty<IAsset>();
		}

		public IAsset CreateAsset(string fileName, byte[] bytes, string virtualFolder = null)
		{
			Logger?.LogInformation($"Create asset {fileName} in {Configuration.AssetRootPath}");
			var virtualPath = Path.Combine(Configuration.AssetRootPath, virtualFolder ?? string.Empty);
			string path = Path.Combine(HostingEnvironment.WebRootPath, virtualPath);
			Directory.CreateDirectory(path);
			string file = Path.Combine(path, fileName);
			File.WriteAllBytes(file, bytes);
			virtualPath = Path.Combine(virtualPath, fileName);
			IAsset a = new FileSystemAsset(HostingEnvironment)
			{
				VirtualPath = "/" + virtualPath
			};
			return a;
		}

		public IAsset CreateAsset(string fileName, string base64String, string virtualFolder = null)
		{
			byte[] bytes = ConvertToBytes(base64String);
			string extFromBase64Type = Regex.Match(base64String, "^([^/]+)/([a-z]+);base64").Groups[2].Value;
			if (fileName?.EndsWith(extFromBase64Type) == false)
			{
				fileName = Path.GetRandomFileName() + "." + extFromBase64Type;
			}
			return CreateAsset(fileName, bytes, virtualFolder);
		}

		public void RemoveAsset(IAsset asset)
		{
			string file = Path.Combine(HostingEnvironment.WebRootPath, asset.VirtualPath);
			File.Delete(file);
		}
		
		private string GetVirtualPath(string path)
		{
			var virtualPath = path.Substring(HostingEnvironment.WebRootPath.Length);
			return virtualPath.Replace("\\", "/");
		}

		private byte[] ConvertToBytes(string base64)
		{
			int index = base64.IndexOf(";base64,", StringComparison.Ordinal) + 8;
			return Convert.FromBase64String(base64.Substring(index));
		}

		private ILogger SetupLogging(ILoggerFactory loggerfactory)
		{
			if (!string.IsNullOrWhiteSpace(Configuration?.LogCategoryName))
			{
				return loggerfactory.CreateLogger(Configuration.LogCategoryName);
			}
			return null;
		}
	}

	public class FileSystemAsset : IAsset
	{
		private string[] ImageExtensions = new[] { "png", "jpg", "jpeg", "gif", "bmp" };
		public IHostingEnvironment HostingEnvironment { get; }
		public FileSystemAsset(IHostingEnvironment env)
		{
			HostingEnvironment = env;
		}
		public FileInfo Info
		{
			get
			{
				var path = Path.Combine(HostingEnvironment.WebRootPath, VirtualPath);
				return new FileInfo(path);
			}
		}

		public string VirtualPath { get; set; }
		public AssetType Type
		{
			get
			{
				var extension = Path.GetExtension(VirtualPath);
				if (ImageExtensions.Contains(extension))
				{
					return AssetType.Image;
				}
				return AssetType.File;
			}
		}
	}
}