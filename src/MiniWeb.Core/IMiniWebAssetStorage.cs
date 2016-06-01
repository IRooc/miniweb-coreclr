﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MiniWeb.Core
{
    public interface IMiniWebAssetStorage
    {	
		//Set explicitly to avoid circular dependency injection
		IMiniWebSite MiniWebSite { get; set; }

		IEnumerable<IAsset> GetAllAssets();
		
		void StoreAsset(IAsset asset);
		void DeleteAsset(IAsset asset);
		
		IAsset CreateAsset(string fileName, byte[] bytes, string virtualFolder = null);
		IAsset CreateAsset(string fileName, string base64String, string virtualFolder = null);
    }

	public enum AssetType
	{
		Image,
		File
	}

	public interface IAsset
	{
		FileInfo Info { get; }
		string VirtualPath { get; set; }
		AssetType Type{ get; set; }
	}
}
