using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Framework.WebEncoders;
using Microsoft.AspNet.Mvc.ViewFeatures;
using Microsoft.AspNet.Mvc.ViewFeatures.Internal;

namespace MiniWeb.Core
{
	[HtmlTargetElement(Attributes = MiniWebTemplateTagname)]
	[HtmlTargetElement(Attributes = MiniWebSectionTagname)]
	public class MiniWebTagHelper : TagHelper
	{
		private const string MiniWebTemplateTagname = "miniweb-template";
		private const string MiniWebSectionTagname = "miniweb-section";

		[HtmlAttributeNotBound]
		[ViewContext]
		public ViewContext ViewContext { get; set; }

		[HtmlAttributeName(MiniWebTemplateTagname)]
		public string Template { get; set; }

		[HtmlAttributeName(MiniWebSectionTagname)]
		public string Section { get; set; }

		private readonly IMiniWebSite _webSite;
		private readonly IHtmlHelper _htmlHelper;
		public MiniWebTagHelper(IMiniWebSite webSite, IHtmlHelper helper)
		{
			_webSite = webSite;
			_htmlHelper = helper;
      }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			//Set Content edit properties on tags when logged in
			if (_webSite.IsAuthenticated(ViewContext.HttpContext.User))
			{
				if (!string.IsNullOrWhiteSpace(Template))
					output.Attributes.Add("data-miniwebtemplate", Template);
				if (!string.IsNullOrWhiteSpace(Section))
					output.Attributes.Add("data-miniwebsection", Section);
			}
			

			//load the content items in the specified section
			if (!string.IsNullOrWhiteSpace(Section))
			{
				//contextualize the HtmlHelper for the current ViewContext
				(_htmlHelper as ICanHasViewContext)?.Contextualize(ViewContext);
				//get out the current ViewPage for the Model.
				var view = ViewContext.View as RazorView;
				var viewPage = view.RazorPage as RazorPage<SitePage>;
				output.Content.Clear();
				output.Content.AppendEncoded(SectionContent(_htmlHelper, viewPage.Model, Section));
			}
		}
		private string SectionContent(IHtmlHelper html, SitePage sitepage, string section)
		{
			if (sitepage?.Sections?.Any(s => s?.Key == section) == true)
			{
				return SectionContent(html, sitepage, sitepage.Sections.First(s => s.Key == section));
			}
			return String.Empty;
		}

		private static string SectionContent(IHtmlHelper html, SitePage sitepage, PageSection model)
		{
			using (StringWriter result = new StringWriter())
			{
				foreach (ContentItem item in model.Items)
				{
					item.Page = sitepage;
					html.Partial(item.Template, item).WriteTo(result, HtmlEncoder.Default);
				}
				return result.ToString();
			}
		}
	}
}
