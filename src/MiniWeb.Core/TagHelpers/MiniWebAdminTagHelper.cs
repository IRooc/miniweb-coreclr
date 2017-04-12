using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MiniWeb.Core.TagHelpers
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
				output.Content.AppendHtml("<script type=\"text/javascript\" src=\"//code.jquery.com/jquery-2.2.4.min.js\"></script>");
				output.Content.AppendHtml("<script type=\"text/javascript\" src=\"//code.jquery.com/jquery-2.2.4.min.js\"></script>");
				output.Content.AppendHtml("<script type=\"text/javascript\" src=\"//maxcdn.bootstrapcdn.com/bootstrap/3.3.7/js/bootstrap.min.js\"></script>");

				//add the own contents.
				output.Content.AppendHtml(output.GetChildContentAsync().Result);

				(_htmlHelper as IViewContextAware )?.Contextualize(ViewContext);
				//admin content
				var content = _htmlHelper.Partial(_webSite.Configuration.EmbeddedResourcePath + MiniWebFileProvider.ADMIN_FILENAME);

				output.PreContent.AppendHtml(content);

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
