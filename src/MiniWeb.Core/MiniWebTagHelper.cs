﻿using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWeb.Core
{
	[TargetElement(Attributes = MiniWebTemplateTagname)]
	[TargetElement(Attributes = MiniWebPropertyTagname)]
	[TargetElement(Attributes = MiniWebSectionTagname)]
	public class MiniWebTagHelper : TagHelper
	{

		private const string MiniWebTemplateTagname = "miniweb-template";
		private const string MiniWebPropertyTagname = "miniweb-prop";
		private const string MiniWebEditTypeTagname = "miniweb-edittype";
		private const string MiniWebSectionTagname = "miniweb-section";

		[HtmlAttributeNotBound]
		[ViewContext]
		public ViewContext ViewContext { get; set; }

		[HtmlAttributeName(MiniWebTemplateTagname)]
		public string Template { get; set; }

		[HtmlAttributeName(MiniWebPropertyTagname)]
		public string Property { get; set; }

		[HtmlAttributeName(MiniWebEditTypeTagname)]
		public string EditType { get; set; }

		[HtmlAttributeName(MiniWebSectionTagname)]
		public string Section { get; set; }

		private IMiniWebSite _webSite;
		private IHtmlHelper _htmlHelper;
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
				if (!string.IsNullOrWhiteSpace(Property))
					output.Attributes.Add("data-miniwebprop", Property);
				if (!string.IsNullOrWhiteSpace(Section))
					output.Attributes.Add("data-miniwebsection", Section);
				if (!string.IsNullOrWhiteSpace(EditType))
					output.Attributes.Add("data-miniwebedittype", EditType);
			}

			//fill property title and content on specified tags
			if (!string.IsNullOrWhiteSpace(Property))
			{
				var view = ViewContext.View as RazorView;
				var viewItem = view.RazorPage as RazorPage<ContentItem>;

				output.Content.SetContent(viewItem.Model.GetValue(Property));
			
			}

			//load the content items in the specified section
			if (!string.IsNullOrWhiteSpace(Section))
			{
				//contextualize the HtmlHelper for the current ViewContext
				(_htmlHelper as ICanHasViewContext)?.Contextualize(ViewContext);
				//get out the current ViewPage for the Model.
				var view = ViewContext.View as RazorView;
				var viewPage = view.RazorPage as RazorPage<SitePage>;
				output.Content.SetContent(SectionContent(_htmlHelper, viewPage.Model, Section));
			}
		}
		private string SectionContent(IHtmlHelper html, SitePage sitepage, string section)
		{
			if (sitepage?.Sections?.Any(s => s?.Key == section) == true)
			{
				return SectionContent(html, sitepage.Sections.First(s => s.Key == section));
			}
			return String.Empty;
		}

		private static string SectionContent(IHtmlHelper html, PageSection model)
		{
			StringBuilder result = new StringBuilder();

			foreach (ContentItem item in model.Items)
			{
				result.Append(html.Partial(item.Template, item));
			}
			return result.ToString();
		}
	}
}
