using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MiniWeb.Core;

namespace MiniWeb.Storage.EFStorage
{
	public class MiniWebEFStorageConfig : IMiniWebStorageConfiguration
	{
		public string Connectionstring { get; set; }

	}
}
