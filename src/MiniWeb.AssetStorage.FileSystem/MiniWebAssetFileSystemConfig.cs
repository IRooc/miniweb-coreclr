using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniWeb.AssetStorage.FileSystem
{
    public class MiniWebAssetFileSystemConfig
    {
		public string AssetRootPath { get; set; } = "images/";
		public string LogCategoryName { get; set; } = "MiniWeb";
    }
}
