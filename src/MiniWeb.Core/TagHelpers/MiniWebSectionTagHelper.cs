using System;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MiniWeb.Core.TagHelpers
{
    [HtmlTargetElement(Attributes = MiniWebSectionTagname)]
	public class MiniWebSectionTagHelper : TagHelper
	{
		private const string MiniWebSectionTagname = "miniweb-section";
		private readonly IMiniWebSite _webSite;
		private readonly IHtmlHelper _htmlHelper;

		[HtmlAttributeNotBound]
		[ViewContext]
		public ViewContext ViewContext { get; set; }

		[HtmlAttributeName(MiniWebSectionTagname)]
		public string Section { get; set; }
		
		public MiniWebSectionTagHelper(IMiniWebSite webSite, IHtmlHelper helper)
		{
			_webSite = webSite;
			_htmlHelper = helper;
		}

		public async override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			//load the content items in the specified section
			if (!string.IsNullOrWhiteSpace(Section))
			{
				//Set Content edit properties on tags when logged in
				if (_webSite?.IsAuthenticated(ViewContext.HttpContext.User) == true)
				{
					output.Attributes.Add("data-miniwebsection", Section);
				}

				//contextualize the HtmlHelper for the current ViewContext
				(_htmlHelper as IViewContextAware)?.Contextualize(ViewContext);
				//get out the current ViewPage for the Model.
				var view = ViewContext.View as RazorView;
				var viewPage = view?.RazorPage as RazorPage<ISitePage>;
				output.Content.Clear();

				await SectionContent(viewPage.Model, viewPage.Model?.Sections?.FirstOrDefault(s => s?.Key == Section), output);
			}
		}

		private async Task SectionContent(ISitePage sitepage, IPageSection model, TagHelperOutput output)
		{
			if (sitepage == null || model == null) return;

			foreach (var item in model.Items)
			{
				item.Page = sitepage;
				var partial = await _htmlHelper.PartialAsync(item.Template, item);
				output.Content.AppendHtml(partial);
			}
		}
	}
}
