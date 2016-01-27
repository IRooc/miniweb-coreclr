using System;
using System.Linq;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;

namespace MiniWeb.Core
{
	public class MiniWebMenuContext
	{
		public IHtmlContent ItemTemplate;
	}

	[HtmlTargetElement("miniweb-menuitem")]
	public class MiniWebMenuItemTagHelper : TagHelper
	{
		public override async void Process(TagHelperContext context, TagHelperOutput output)
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

		public override async void Process(TagHelperContext context, TagHelperOutput output)
		{
			//setup context
			MiniWebMenuContext menuContext = new MiniWebMenuContext();
			context.Items.Add(typeof(MiniWebMenuContext), menuContext);

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
				//remember the current model
				object currentModel = ViewContext.ViewData.Model;
				foreach (var page in items)
				{
					//override the model to the current child page
					ViewContext.ViewData.Model = page;

					//render child without cached results.
					await output.GetChildContentAsync(false);

					//get the current parsed ItemTemplate form the context
					output.Content.AppendHtml(menuContext.ItemTemplate);
				}
				//reste the current model
				ViewContext.ViewData.Model = currentModel;
			}
			else
			{
				output.SuppressOutput();
			}
		}
	}
}
