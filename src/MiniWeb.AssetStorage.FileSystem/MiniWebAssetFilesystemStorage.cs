using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
		public IWebHostEnvironment HostingEnvironment { get; }
		public MiniWebAssetFileSystemConfig Configuration { get; }

		public MiniWebAssetFileSystemStorage(IWebHostEnvironment env, ILogger<MiniWebAssetFileSystemStorage> logger, IOptions<MiniWebAssetFileSystemConfig> config)
		{
			HostingEnvironment = env;
			Configuration = config.Value;
			Logger = logger;
		}


		public Task<IEnumerable<IAsset>> GetAllAssets()
		{
			string folder = Path.Combine(HostingEnvironment.WebRootPath, Configuration.AssetRootPath);
			if (!string.IsNullOrWhiteSpace(Configuration.AssetRootPath) && !Configuration.AssetRootPath.Contains("..") && Directory.Exists(folder))
			{
				var allFiles = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);
				return Task.FromResult<IEnumerable<IAsset>>(allFiles.Select(f => new FileSystemAsset(HostingEnvironment, Configuration, f)));
			}
			Logger?.LogWarning($"Folder not present {folder} no assets loaded");
			return Task.FromResult(Enumerable.Empty<IAsset>());
		}

		public Task<IAsset> CreateAsset(string fileName, byte[] bytes, string virtualFolder = null)
		{
			if (virtualFolder == null) virtualFolder = string.Empty;
			Logger?.LogInformation($"Create asset {virtualFolder}/{fileName} in {Configuration.AssetRootPath}");
			
			if (virtualFolder.StartsWith("/")) virtualFolder = virtualFolder.Substring(1);
			if (virtualFolder.StartsWith(Configuration.AssetRootPath)) virtualFolder = virtualFolder.Substring(Configuration.AssetRootPath.Length);
			
			string filePath = Path.Combine(virtualFolder, fileName);
			filePath = filePath.Replace("\\","/");
			//should we check the assetrootpath?
			string path = Path.Combine(HostingEnvironment.WebRootPath, Configuration.AssetRootPath, filePath);
			string folderName = Path.GetDirectoryName(path);
			Directory.CreateDirectory(folderName);
			File.WriteAllBytes(path, bytes);
			return Task.FromResult<IAsset>(new FileSystemAsset(HostingEnvironment, Configuration, path));
		}

		public Task<IAsset>  CreateAsset(string fileName, string base64String, string virtualFolder = null)
		{
			byte[] bytes = ConvertToBytes(base64String);
			string extFromBase64Type = Regex.Match(base64String, "^([^/]+)/([a-z]+);base64").Groups[2].Value;
			if (fileName?.EndsWith(extFromBase64Type) == false)
			{
				fileName = Path.GetRandomFileName() + "." + extFromBase64Type;
			}
			return CreateAsset(fileName, bytes, virtualFolder);
		}

		public Task<bool> RemoveAsset(IAsset asset)
		{
			string file = Path.Combine(HostingEnvironment.WebRootPath, asset.VirtualPath);
			if (!File.Exists(file)) return Task.FromResult(false);
			File.Delete(file);
			return Task.FromResult(true);
		}

		private byte[] ConvertToBytes(string base64)
		{
			int index = base64.IndexOf(";base64,", StringComparison.Ordinal) + 8;
			return Convert.FromBase64String(base64.Substring(index));
		}
	}

	public class FileSystemAsset : IAsset
	{
		private string[] ImageExtensions = new[] { ".png", ".jpg", ".jpeg", ".gif", ".bmp" };
		public IWebHostEnvironment HostingEnvironment { get; }
		public MiniWebAssetFileSystemConfig Configuration { get; }
		public FileSystemAsset(IWebHostEnvironment env, MiniWebAssetFileSystemConfig config, string fullPath)
		{
			HostingEnvironment = env;
			Configuration = config;
			VirtualPath = GetVirtualPath(fullPath);
			FileName = Path.GetFileName(VirtualPath);
			Folder = Path.GetDirectoryName(VirtualPath).Replace("\\", "/");
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
				var extension = Path.GetExtension(VirtualPath)?.ToLowerInvariant();
				if (ImageExtensions.Contains(extension))
				{
					return AssetType.Image;
				}
				return AssetType.File;
			}
		}

		public string FileName { get; set; }
		public string Folder { get; set; }

		private string GetVirtualPath(string path)
		{
			var virtualPath = path.Substring(HostingEnvironment.WebRootPath.Length);
			return virtualPath.Replace("\\", "/");
		}
	}
}
