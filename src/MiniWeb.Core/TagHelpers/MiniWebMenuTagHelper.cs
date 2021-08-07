using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;

namespace MiniWeb.Core.TagHelpers
{
	public class MiniWebMenuContext
	{
		public IHtmlContent ItemTemplate;
	}

	[HtmlTargetElement("miniweb-menuitem")]
	public class MiniWebMenuItemTagHelper : TagHelper
	{
		public async override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			if (!context.Items.ContainsKey(typeof(MiniWebMenuContext)))
			{
				throw new InvalidOperationException($"Can only be used inside a tag with the {MiniWebMenuTagHelper.MiniWebMenuAttributename} attribute set");
			}
			else
			{
				var modalContext = (MiniWebMenuContext)context.Items[typeof(MiniWebMenuContext)];
				modalContext.ItemTemplate = await output.GetChildContentAsync();
				output.SuppressOutput();
			}
		}
	}

	[HtmlTargetElement(Attributes = MiniWebMenuAttributename)]
	[RestrictChildren("miniweb-menuitem")]
	public class MiniWebMenuTagHelper : TagHelper
	{
		internal const string MiniWebMenuAttributename = "miniweb-menu";
		private const string MiniWebItemTemplate = "miniweb-menu-template";

		private readonly IMiniWebSite _webSite;

		[HtmlAttributeNotBound]
		[ViewContext]
		public ViewContext ViewContext { get; set; }

		[HtmlAttributeName(MiniWebMenuAttributename)]
		public string MenuRoot { get; set; }

		[HtmlAttributeName(MiniWebItemTemplate)]
		public string MenuItemTemplate { get; set; }

		public MiniWebMenuTagHelper(IMiniWebSite webSite)
		{
			_webSite = webSite;
		}

		public async override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			//setup context
			MiniWebMenuContext menuContext = new MiniWebMenuContext();
			context.Items.Add(typeof(MiniWebMenuContext), menuContext);

			var items = Enumerable.Empty<ISitePage>();
			var pages = await _webSite.Pages();
			if (MenuRoot == "/")
			{
				//get all URL's without / in them.
				items = pages.Where(p => !p.Url.Contains('/') && ( p.VisibleInMenu() || _webSite.IsAuthenticated(ViewContext.HttpContext.User)));
			}
			else if (pages.Any(p => ("/" + p.Url) == MenuRoot))
			{
				items = pages.First(p => ("/" + p.Url) == MenuRoot).Pages.Where(p => p.VisibleInMenu() || _webSite.IsAuthenticated(ViewContext.HttpContext.User));
			}
			else
			{
				_webSite.Logger?.LogWarning($"No menuitems found for {MenuRoot}");
			}

			if (items.Any())
			{
				//remember the current model
				var currentModel = ViewContext.ViewData.Model as ISitePage;
				for(var i =0; i < items.Count(); i++)				
				{
					var page = items.ElementAt(i);
					//set ViewData needed in child template
					ViewContext.ViewData["MenuIteratorIndex"] = i;
					ViewContext.ViewData["CurrentUrl"] = currentModel.Url;
					ViewContext.ViewData["MenuSitePage"]= page;

					//render child without cached results.
					await output.GetChildContentAsync(false);

					//get the current parsed ItemTemplate from the context
					output.Content.AppendHtml(menuContext.ItemTemplate);
				}
			}
			else
			{
				output.SuppressOutput();
			}
		}
	}
}
