
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using MiniWeb.Core;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SampleRazor.Pages
{
	public class LoginModel : PageModel
	{
		public readonly IMiniWebSite Miniweb;

		public LoginModel(IMiniWebSite miniweb)
		{
			Miniweb = miniweb;
		}

        [BindProperty]
        public string Username { get; set; }

        [BindProperty, DataType(DataType.Password)]
        public string Password { get; set; }
		public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
		{
			Miniweb.Logger?.LogInformation("login post");
			if (Miniweb.Authenticate(Username, Password))
			{
				var principal = Miniweb.GetClaimsPrincipal(Username);
				await HttpContext.SignInAsync(Miniweb.Configuration.Authentication.AuthenticationScheme, principal);
				return Redirect(Miniweb.Configuration.DefaultPage);
			}
			ErrorMessage = $"Failed to login as {Username}";
            return Page();
		}
	}
}