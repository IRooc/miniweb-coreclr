using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MiniWeb.Core
{
    public interface IMiniWebAssetStorage
    {	
		//Set explicitly to avoid circular dependency injection
		IMiniWebSite MiniWebSite { get; set; }

		Task<IEnumerable<IAsset>> GetAllAssets();
		
		Task RemoveAsset(IAsset asset);		
		Task<IAsset> CreateAsset(string fileName, byte[] bytes, string virtualFolder = null);
		Task<IAsset> CreateAsset(string fileName, string base64String, string virtualFolder = null);
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
		string FileName { get; set; }
		string Folder { get; set; }
		AssetType Type{ get; }

	}
}
