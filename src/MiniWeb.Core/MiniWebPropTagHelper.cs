using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;

namespace MiniWeb.Core
{
	[HtmlTargetElement(Attributes = MiniWebPropertyTagname)]
	public class MiniWebPropTagHelper : TagHelper
	{
		private const string MiniWebPropertyTagname = "miniweb-prop";
		private const string MiniWebEditTypeTagname = "miniweb-edittype";
		private const string MiniWebEditAttributesTagname = "miniweb-attributes";

		private readonly IMiniWebSite _webSite;

		[HtmlAttributeNotBound]
		[ViewContext]
		public ViewContext ViewContext { get; set; }

		[HtmlAttributeName(MiniWebPropertyTagname)]
		public string Property { get; set; }

		[HtmlAttributeName(MiniWebEditTypeTagname)]
		public string EditType { get; set; }

		[HtmlAttributeName(MiniWebEditAttributesTagname)]
		public string EditAttributeString { get; set; }

		public string[] EditAttributes
		{
			get
			{
				return (EditAttributeString ?? string.Empty).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			}
		}


		public MiniWebPropTagHelper(IMiniWebSite webSite)
		{
			_webSite = webSite;
		}

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			//fill property title and content on specified tags
			if (!string.IsNullOrWhiteSpace(Property))
			{
				var view = ViewContext.View as RazorView;
				var viewItem = view.RazorPage as RazorPage<ContentItem>;
				var htmlContent = viewItem.Model.GetValue(Property, output.GetChildContentAsync().Result?.GetContent(HtmlEncoder.Default));
                output.Content.Clear();
				output.Content.AppendHtml(htmlContent);

				foreach (var attr in EditAttributes)
				{
					output.Attributes[attr].Value = viewItem.Model.GetValue(Property + ":" + attr, context.AllAttributes[attr]?.Value?.ToString());
				}
			}
			//Set Content edit properties on tags when logged in
			if (_webSite.IsAuthenticated(ViewContext.HttpContext.User))
			{
				if (!string.IsNullOrWhiteSpace(Property))
					output.Attributes.Add("data-miniwebprop", Property);
				if (!string.IsNullOrWhiteSpace(EditType))
					output.Attributes.Add("data-miniwebedittype", EditType);

				if (EditAttributes.Any())
				{
					output.PostElement.AppendHtml("<div class=\"miniweb-attributes\">");
					foreach (var attr in EditAttributes)
					{
						var attrEditItem = string.Format("<span data-miniwebprop=\"{0}:{1}\">{2}</span>", Property, attr, output.Attributes[attr].Value);
						output.PostElement.AppendHtml(attrEditItem);
					}
					output.PostElement.AppendHtml("</div>");
				}
			}
		}
	}
}
