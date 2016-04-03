using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Html.Abstractions;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Mvc.ViewFeatures;
using Microsoft.AspNet.Mvc.ViewFeatures.Internal;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.AspNet.Razor.TagHelpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.WebEncoders;

namespace MiniWeb.Core
{
	public class MiniWebMenuContext
	{
		public TagHelperContent ItemTemplate;
	}

	[HtmlTargetElement("miniweb-menuitem")]
	public class MiniWebMenuItemTagHelper : TagHelper
	{

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
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
		
		public string MenuRoot { get; set; }

		[HtmlAttributeName(MiniWebItemTemplate)]
		public string MenuItemTemplate { get; set; }

		public MiniWebMenuTagHelper(IMiniWebSite webSite)
		{
			_webSite = webSite;
		}

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			MenuRoot = Convert.ToString(context.AllAttributes.FirstOrDefault(a => a.Name == MiniWebMenuAttributename)?.Value ?? string.Empty);

			var items = Enumerable.Empty<SitePage>();

			if (string.IsNullOrWhiteSpace(MenuRoot) || MenuRoot == "/")
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
				//setup context
				MiniWebMenuContext menuContext = new MiniWebMenuContext();
				context.Items.Add(typeof(MiniWebMenuContext), menuContext);

				//remember the current model
				object currentModel = ViewContext.ViewData.Model;
				foreach (var page in items)
				{
					//override the model to the current child page
					ViewContext.ViewData.Model = page;

					//render child without cached results.
					await output.GetChildContentAsync(false);

					//get the current parsed ItemTemplate from the context
					output.Content.AppendHtml(menuContext.ItemTemplate.GetContent(HtmlEncoder.Default));
				}
				//reset the current model
				ViewContext.ViewData.Model = currentModel;
			}
			else
			{
				output.SuppressOutput();
			}
		}
	}
}
