using Microsoft.AspNet.Mvc.ViewFeatures.Internal;
using Microsoft.AspNet.Mvc.ViewFeatures;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.AspNet.Razor.TagHelpers;

namespace MiniWeb.Core
{
	[HtmlTargetElement(MiniWebAdminTag)]
	public class MiniWebAdminTagHelper : TagHelper
	{
		private const string MiniWebAdminTag = "miniwebadmin";
		private const string MiniWebIgnoreAdminStartTagname = "ignoreadminstart";

		private readonly IMiniWebSite _webSite;
		private readonly IHtmlHelper _htmlHelper;

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
				output.Content.SetContent(output.GetChildContentAsync().Result);

				(_htmlHelper as ICanHasViewContext)?.Contextualize(ViewContext);
				output.PreContent.Append(_htmlHelper.Partial(_webSite.Configuration.EmbeddedResourcePath + MiniWebFileProvider.ADMIN_FILENAME));

				if (!IgnoreAdminStart)
				{
					output.Content.AppendHtml($"<script>$(function(){{$('{MiniWebAdminTag}').miniwebAdmin();}});</script>");
				}
			}
			else
			{
				output.SuppressOutput();
			}
		}
	}
}
