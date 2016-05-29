using System;
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

		IEnumerable<Asset> Assets { get; set; }
		
		void StoreAsset(Asset asset);
		void DeleteAsset(Asset asset);

		Asset CreateAsset(byte[] bytes, string fullFilePath);
    }

	public class Asset
	{
		public FileInfo Info { get; set; }
		public string VirtualPath { get; }
		public string Type { get; set; }
	}
}
