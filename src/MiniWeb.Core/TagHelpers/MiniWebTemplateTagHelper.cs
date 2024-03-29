﻿using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MiniWeb.Core.TagHelpers
{
	[HtmlTargetElement(Attributes = MiniWebTemplateTagname)]
	public class MiniWebTemplateTagHelper : TagHelper
	{
		private const string MiniWebTemplateTagname = "miniweb-template";
		private readonly IMiniWebSite _webSite;

		[HtmlAttributeNotBound]
		[ViewContext]
		public ViewContext ViewContext { get; set; }

		[HtmlAttributeName(MiniWebTemplateTagname)]
		public string Template { get; set; }

		public MiniWebTemplateTagHelper(IMiniWebSite webSite)
		{
			_webSite = webSite;
		}

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			//Set Content edit properties on tags when logged in
			if (!string.IsNullOrWhiteSpace(Template) && _webSite?.IsAuthenticated(ViewContext.HttpContext.User) == true)
			{
				output.Attributes.Add("data-miniwebtemplate", Template);
			}
		}
	}
}
