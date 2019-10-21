using System.Threading.Tasks;
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

		public async override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			if (_webSite.IsAuthenticated(ViewContext.HttpContext.User))
			{
				output.TagMode = TagMode.StartTagAndEndTag;
				output.Content.AppendHtml("<script type=\"text/javascript\" src=\"//code.jquery.com/jquery-2.2.4.min.js\"></script>");
				output.Content.AppendHtml("<script type=\"text/javascript\" src=\"//maxcdn.bootstrapcdn.com/bootstrap/3.3.7/js/bootstrap.min.js\"></script>");

				//add the own contents.
				var ownContent = await output.GetChildContentAsync();
				output.Content.AppendHtml(ownContent);

				(_htmlHelper as IViewContextAware )?.Contextualize(ViewContext);
				//admin content
				var content = await _htmlHelper.PartialAsync(_webSite.Configuration.EmbeddedResourcePath + MiniWebFileProvider.ADMIN_FILENAME);

				output.PreContent.AppendHtml(content);

				if (!IgnoreAdminStart)
				{
					output.Content.AppendHtml($"<script>$(function(){{ window.currentMiniweb = $('{MiniWebAdminTag}').miniwebAdmin({{ \"apiEndpoint\":\"{_webSite.Configuration.ApiEndpoint}\"}});}});</script>");
				}
			}
			else
			{
				output.SuppressOutput();
			}
		}
	}
}
