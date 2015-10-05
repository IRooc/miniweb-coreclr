using MiniWeb.Core;

namespace MiniWeb.Storage.EFStorage
{
	public class MiniWebEFStorageConfig : IMiniWebStorageConfiguration
	{
		public string Connectionstring { get; set; } = "data source=sqldev2k12;user id=MiniWebTest;password=MiniWebTest;initial catalog=MiniWebTest";

	}
}
