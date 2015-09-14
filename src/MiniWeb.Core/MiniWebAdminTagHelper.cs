using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniWeb.Core
{
	[HtmlTargetElement(MiniWebAdminTag)]
	public class MiniWebAdminTagHelper : TagHelper
	{
		private const string MiniWebAdminTag = "miniwebadmin";
		private const string MiniWebIgnoreAdminStartTagname = "ignoreadminstart";

		private IMiniWebSite _webSite;
		private IHtmlHelper _htmlHelper;

		[ViewContext]
		public ViewContext ViewContext { get; set; }

		[HtmlAttributeName(MiniWebIgnoreAdminStartTagname)]
		public bool IgnoreAdminStart { get; set; }

		public MiniWebAdminTagHelper(IMiniWebSite webSite, IHtmlHelper helper)
		{
			_webSite = webSite;
			_htmlHelper = helper;
		}

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			if (_webSite.IsAuthenticated(ViewContext.HttpContext.User))
			{
				output.TagMode = TagMode.StartTagAndEndTag;
				//add the own contents.
				output.Content.SetContent(context.GetChildContentAsync().Result);

				(_htmlHelper as ICanHasViewContext)?.Contextualize(ViewContext);
				output.PreContent.Append(_htmlHelper.Partial(MiniWebFileProvider.ADMIN_FILENAME));

				if (!IgnoreAdminStart)
				{
					output.Content.AppendEncoded($"<script>$(function(){{$('{MiniWebAdminTag}').miniwebAdmin();}});</script>");
				}
			}
			else
			{
				output.SuppressOutput();
			}
		}
	}
}
