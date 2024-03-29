using System;
using System.Linq;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MiniWeb.Core.TagHelpers
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

		[HtmlAttributeName("miniweb-editonly")]
		public bool EditOnly { get; set; }

		[HtmlAttributeName("miniweb-form-input")]
		public bool FormInput { get; set; }

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
				var viewItem = view.RazorPage as RazorPage<IContentItem>;
				var htmlContent = viewItem.Model.GetValue(Property, output.GetChildContentAsync().Result?.GetContent(HtmlEncoder.Default));
				if (FormInput)
				{
					output.AddClass("miniweb-input-value",HtmlEncoder.Default);
					output.Attributes.SetAttribute("data-miniwebinputvalue", htmlContent);
				}
				else
				{
					output.Content.Clear();
					output.Content.AppendHtml(htmlContent);
				}
				foreach (var attr in EditAttributes)
				{
					var curAttr = attr;
					var attrEls = attr.Split('|');
					if (attrEls.Length > 1)
					{
						curAttr = attrEls[0];
					}
					output.Attributes.SetAttribute(curAttr, viewItem.Model.GetValue(Property + ":" + curAttr, context.AllAttributes[attr]?.Value?.ToString()));
				}
			}
			//Set Content edit properties on tags when logged in
			if (_webSite.IsAuthenticated(ViewContext.HttpContext.User))
			{
				if (!string.IsNullOrWhiteSpace(Property))
					output.Attributes.Add("data-miniwebprop", Property);
				if (!string.IsNullOrWhiteSpace(EditType))
					output.Attributes.Add("data-miniwebedittype", EditType);

				if (EditOnly)
				{
					output.AddClass("miniweb-editonly", HtmlEncoder.Default);
				}

				if (EditAttributes.Any())
				{
					output.PostElement.AppendHtml("<div class=\"miniweb-attributes\">");
					foreach (var attr in EditAttributes)
					{
						var curAttr = attr;
						var curType = string.Empty;
						var attrEls = attr.Split('|');
						if (attrEls.Length > 1)
						{
							curAttr = attrEls[0];
							curType = $" data-miniwebedittype=\"{attrEls[1]}\"";
						}
						var attrEditItem = string.Format("<span data-miniwebprop=\"{0}:{1}\" {3}>{2}</span>", Property, curAttr, output.Attributes[curAttr].Value, curType);
						output.PostElement.AppendHtml(attrEditItem);
					}
					output.PostElement.AppendHtml("</div>");
				}
			}
			else if (EditOnly)
			{
				output.SuppressOutput();
			}
		}
	}
}
