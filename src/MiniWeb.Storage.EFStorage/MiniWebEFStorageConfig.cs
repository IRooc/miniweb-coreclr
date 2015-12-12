using MiniWeb.Core;

namespace MiniWeb.Storage.EFStorage
{
	public class MiniWebEFStorageConfig : IMiniWebStorageConfiguration
	{
		public string Connectionstring { get; set; } = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=MiniWebDemo;Integrated Security=True";
	}
}
