
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniWeb.Core;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

namespace MiniWeb.Core.UI.Pages
{
    public class MiniwebModel : PageModel
    {
        public readonly IMiniWebSite Miniweb;
        public ISitePage SitePage { get; set; }

        [BindProperty(SupportsGet = true)]
        public string PageUrl { get; set; }

        public MiniwebModel(IMiniWebSite miniweb)
        {
            Miniweb = miniweb;
        }

        public async Task OnGet()
        {
            Miniweb.Logger?.LogDebug($"index action {Request.Path.Value}");

            var result = await Miniweb.GetPageByUrl(PageUrl, User);

            //redirect if not editing?
            if (!string.IsNullOrWhiteSpace(result.RedirectUrl))
            {
                Redirect(result.RedirectUrl);
            }
            if (!result.Found)
            {
                Response.StatusCode = 404;
            }
            SitePage = result.Page;
        }
    }
}