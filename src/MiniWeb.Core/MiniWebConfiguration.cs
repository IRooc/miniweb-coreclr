using System.Collections.Generic;

namespace MiniWeb.Core
{
    public class MiniWebConfiguration
	{
		public string Title { get; set; } = "MiniWeb by Rooc";
		public string DefaultPage { get; set; } = "/home.html";
		public string PageExtension { get; set; } = "html";
		public bool PageExtensionForce { get; set; } = false;

		public string LoginView { get; set; } = "~/Views/login.cshtml";
		public string ApiEndpoint {get;set;} = "/miniweb-api/";

		public string PageTemplatePath { get; set; } = "/Views/Pages";
		public string ItemTemplatePath { get; set; } = "/Views/Items";
		public string ImageSavePath { get; set; } = "images/saved/";
		public string EmbeddedResourcePath { get; set; } = "/miniweb-resource/";

		public bool RedirectToFirstSub { get; set; } = false;
		public bool StoreVersions { get; set; } = false;

		public List<DefaultContent> DefaultContent { get; set; } = new List<DefaultContent>();

		public MiniWebAuthentication Authentication { get; set; } = new MiniWebAuthentication();

		public string LogCategoryName { get; set; } = "MiniWeb";
	}

	public class DefaultContent
	{
		public string Template { get; set; }
		public List<SectionContent> Content { get; set; } = new List<SectionContent>();
	}
	public class SectionContent
	{
		public string Section { get; set; }
		public List<string> Items { get; set; } = new List<string>();
	}

	public class DummyContentItem : IContentItem
	{
		public ISitePage Page { get; set; }

		public string Template { get; set; }

		public Dictionary<string, string> Values { get; set; }

		public string GetValue(string value, string defaultvalue = "")
		{
			return defaultvalue;
		}

		public T Get<T>(string value, T defaultvalue = default(T))
		{
			return defaultvalue;
		}
	}

	public class MiniWebAuthentication
	{
		public const string MiniWebCmsRoleValue = "MiniWeb-CmsRole";
		public string AuthenticationScheme { get; set; } = "MiniWebCms";
		public string LoginPath { get; set; } = "/miniweb/login.html";
		public string SocialLoginPath { get; set; } = "/miniweb/social-login.html";
		public string LogoutPath { get; set; } = "/miniweb/logout.html";
		public string AuthenticationType { get; set; } = typeof(MiniWebAuthentication).Namespace + ".MiniWebAuth";
	}
}