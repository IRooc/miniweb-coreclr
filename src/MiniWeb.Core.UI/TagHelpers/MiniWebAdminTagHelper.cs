using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.FileProviders;

namespace MiniWeb.Core.UI.TagHelpers
{
	[HtmlTargetElement(MiniWebAdminTag)]
	public class MiniWebAdminTagHelper : TagHelper
	{
		private const string MiniWebAdminTag = "miniwebadmin";
		private const string MiniWebIgnoreAdminStartTagname = "ignoreadminstart";

		private readonly IMiniWebSite _webSite;
		private readonly IHtmlHelper _htmlHelper;
		private readonly IAntiforgery _antiforgery;

		[ViewContext]
		public ViewContext ViewContext { get; set; }

		[HtmlAttributeName(MiniWebIgnoreAdminStartTagname)]
		public bool IgnoreAdminStart { get; set; }

		public MiniWebAdminTagHelper(IMiniWebSite webSite, IHtmlHelper helper, IAntiforgery antiforgery)
		{
			_webSite = webSite;
			_htmlHelper = helper;
			_antiforgery = antiforgery;
		}

		public async override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			if (_webSite.IsAuthenticated(ViewContext.HttpContext.User))
			{
				output.TagMode = TagMode.StartTagAndEndTag;

				//add the own contents.
				var ownContent = await output.GetChildContentAsync();
				output.Content.AppendHtml(ownContent);

				(_htmlHelper as IViewContextAware)?.Contextualize(ViewContext);
				var view = ViewContext.View as RazorView;
				var viewPage = view?.RazorPage as RazorPage<ISitePage>;
				//admin content
				if (viewPage != null)
				{
					var content = await _htmlHelper.PartialAsync("/Areas/MiniWeb/Views/adminview.cshtml", viewPage.Model);

					output.PreContent.AppendHtml(content);

					if (!IgnoreAdminStart)
					{
						var tokens = _antiforgery.GetAndStoreTokens(ViewContext.HttpContext);
						output.Content.AppendHtml($"<script type=\"module\">import {{ miniwebAdminInit }} from '/miniweb-resources/admin.js';miniwebAdminInit({{ \"apiEndpoint\":\"{_webSite.Configuration.ApiEndpoint}\", \"afToken\":\"{tokens.RequestToken}\"}});</script>");
					}
				}
				else
				{
					output.SuppressOutput();
				}
			}
			else
			{
				output.SuppressOutput();
			}
		}
	}
}
