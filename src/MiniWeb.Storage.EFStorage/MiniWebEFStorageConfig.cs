namespace MiniWeb.Storage.EFStorage
{
    public class MiniWebEFStorageConfig 
	{
		public string Connectionstring { get; set; } = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=MiniWebDemo;Integrated Security=True";
		public string Layout { get; set; } = "~/Views/_layout.cshtml";
		public string LoginView { get; set; } = "~/Views/login.cshtml";
		public string PageTemplatePath { get; set; } = "/Views/Pages";
		public string ItemTemplatePath { get; set; } = "/Views/Items";
	}
}
