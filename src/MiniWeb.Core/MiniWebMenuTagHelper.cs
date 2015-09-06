using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;

namespace MiniWeb.Core
{
	[TargetElement(Attributes = MiniWebMenuAttributename)]
	public class MiniWebMenuTagHelper : TagHelper
	{
		private const string MiniWebMenuAttributename = "miniweb-menu";
		private const string MiniWebItemTemplate = "miniweb-menu-template";

		private IMiniWebSite _webSite;
		private IHtmlHelper _htmlHelper;

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
			IEnumerable<SitePage> items;

			if (MenuRoot == "/")
			{
				items = _webSite.PageHierarchy.Where(p => p.VisibleInMenu() || _webSite.IsAuthenticated(ViewContext.HttpContext.User));
			}
			//NOTE(RC):Still only 2 levels root and sub see MiniWebSite.ReloadPages
			else if (_webSite.PageHierarchy.Any(p => ("/" + p.Url) == MenuRoot))
			{
				items = _webSite.PageHierarchy.First(p => ("/" + p.Url) == MenuRoot).Pages;
			}
			else
			{
				items = Enumerable.Empty<SitePage>();
			}

			if (items.Any())
			{
				(_htmlHelper as ICanHasViewContext)?.Contextualize(ViewContext);
				foreach (var page in items)
				{
					output.Content.Append(_htmlHelper.Partial(MenuItemTemplate, page).ToString());
				}
			}
			else
			{
				output.SuppressOutput();
			}
		}
	}
}
