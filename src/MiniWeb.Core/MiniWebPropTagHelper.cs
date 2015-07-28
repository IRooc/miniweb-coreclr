using Microsoft.AspNet.Mvc;
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
	[TargetElement(Attributes = MiniWebPropertyTagname)]
	public class MiniWebPropTagHelper : TagHelper
	{
		private const string MiniWebPropertyTagname = "miniweb-prop";
		private const string MiniWebEditTypeTagname = "miniweb-edittype";
		private const string MiniWebEditAttributesTagname = "miniweb-attributes";

		private IMiniWebSite _webSite;
		private IHtmlHelper _htmlHelper;

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


		public MiniWebPropTagHelper(IMiniWebSite webSite, IHtmlHelper helper)
		{
			_webSite = webSite;
			_htmlHelper = helper;
		}

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			//fill property title and content on specified tags
			if (!string.IsNullOrWhiteSpace(Property))
			{
				var view = ViewContext.View as RazorView;
				var viewItem = view.RazorPage as RazorPage<ContentItem>;

				output.Content.SetContent(viewItem.Model.GetValue(Property, context.GetChildContentAsync().Result?.ToString()));

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
					output.PostElement.Append("<div class=\"miniweb-attributes\">");
					foreach (var attr in EditAttributes)
					{
						output.PostElement.AppendFormat("<span data-miniwebprop=\"{0}:{1}\">{2}</span>", Property, attr, output.Attributes[attr].Value);
					}
					output.PostElement.Append("</div>");
				}

			}


		}
	}
}
