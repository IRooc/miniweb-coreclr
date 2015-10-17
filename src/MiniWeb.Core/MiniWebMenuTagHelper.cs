using System.Linq;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Mvc.ViewFeatures;
using Microsoft.AspNet.Mvc.ViewFeatures.Internal;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.Framework.Logging;

namespace MiniWeb.Core
{
	[HtmlTargetElement(Attributes = MiniWebMenuAttributename)]
	public class MiniWebMenuTagHelper : TagHelper
	{
		private const string MiniWebMenuAttributename = "miniweb-menu";
		private const string MiniWebItemTemplate = "miniweb-menu-template";

		private readonly IMiniWebSite _webSite;
		private readonly IHtmlHelper _htmlHelper;

		[HtmlAttributeNotBound]
		[ViewContext]
		public ViewContext ViewContext { get; set; }

		[HtmlAttributeName(MiniWebMenuAttributename)]
		public string MenuRoot { get; set; }

		[HtmlAttributeName(MiniWebItemTemplate)]
		public string MenuItemTemplate { get; set; }

		public MiniWebMenuTagHelper(IMiniWebSite webSite, IHtmlHelper helper)
		{
			_webSite = webSite;
			_htmlHelper = helper;
		}

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			var items = Enumerable.Empty<SitePage>();

			if (MenuRoot == "/")
			{
				items = _webSite.PageHierarchy.Where(p => p.VisibleInMenu() || _webSite.IsAuthenticated(ViewContext.HttpContext.User));
			}
			else if (_webSite.Pages.Any(p => ("/" + p.Url) == MenuRoot))
			{
				items = _webSite.Pages.First(p => ("/" + p.Url) == MenuRoot).Pages.Where(p => p.VisibleInMenu() || _webSite.IsAuthenticated(ViewContext.HttpContext.User));
			}
			else
			{
				_webSite.Logger?.LogWarning($"No menuitems found for {MenuRoot}");				
			}

			if (items.Any())
			{
				(_htmlHelper as ICanHasViewContext)?.Contextualize(ViewContext);
				foreach (var page in items)
				{
					output.Content.Append(_htmlHelper.Partial(MenuItemTemplate, page));
				}
			}
			else
			{
				output.SuppressOutput();
			}
		}
	}
}
