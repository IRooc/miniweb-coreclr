using System;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
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

		[HtmlAttributeNotBound]
		[ViewContext]
		public ViewContext ViewContext { get; set; }

		[HtmlAttributeName(MiniWebSectionTagname)]
		public string Section { get; set; }

		private readonly IMiniWebSite _webSite;
		private readonly IHtmlHelper _htmlHelper;
		public MiniWebSectionTagHelper(IMiniWebSite webSite, IHtmlHelper helper)
		{
			_webSite = webSite;
			_htmlHelper = helper;
		}

		public override void Process(TagHelperContext context, TagHelperOutput output)
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

				if (viewPage != null)
				{
					var sectionContent = SectionContent(_htmlHelper, viewPage.Model, Section);
					output.Content.AppendHtml(sectionContent);
				}
			}
		}
		private string SectionContent(IHtmlHelper html, ISitePage sitepage, string section)
		{
			if (sitepage?.Sections?.Any(s => s?.Key == section) == true)
			{
				return SectionContent(html, sitepage, sitepage.Sections.First(s => s.Key == section));
			}
			return String.Empty;
		}

		private static string SectionContent(IHtmlHelper html, ISitePage sitepage, IPageSection model)
		{
			using (StringWriter result = new StringWriter())
			{
				foreach (var item in model.Items)
				{
					item.Page = sitepage;
					html.Partial(item.Template, item).WriteTo(result, HtmlEncoder.Default);
				}
				return result.ToString();
			}
		}
	}
}
