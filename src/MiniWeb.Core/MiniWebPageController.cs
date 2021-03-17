using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MiniWeb.Core
{
	public class MiniWebPageController : Controller
	{
		private readonly IMiniWebSite _webSite;

		public MiniWebPageController(IMiniWebSite website)
		{
			_webSite = website;
		}

		public IActionResult Index(string url)
		{
			_webSite.Logger?.LogInformation($"index action {Request.Path.Value}");

			bool editing = _webSite.IsAuthenticated(User);
			var result = _webSite.GetPageByUrl(url, editing);

			//redirect if not editing?
			if (!string.IsNullOrWhiteSpace(result.RedirectUrl))
			{
				return Redirect(result.RedirectUrl);
			}
			if (!result.Found)
			{
				Response.StatusCode = 404;
			}
			return View(result.Page.Template, result.Page);
		}

		public IActionResult Login()
		{
			_webSite.Logger?.LogInformation("login action");
			return base.View(_webSite.Configuration.LoginView, _webSite.ContentStorage.MiniWebLoginPage);
		}

		[HttpPost]
		public async Task<IActionResult> Login(string username, string password)
		{
			_webSite.Logger?.LogInformation("login post");
			if (_webSite.Authenticate(username, password))
			{
				var principal = _webSite.GetClaimsPrincipal(username);
				await HttpContext.SignInAsync(_webSite.Configuration.Authentication.AuthenticationScheme, principal);
				return Redirect(_webSite.Configuration.DefaultPage);
			}
			ViewBag.ErrorMessage = $"Failed to login as {username}";
			return Login();
		}

		public IActionResult SocialLogin()
		{
			if (_webSite.IsAuthenticated(User))
			{
				_webSite.Logger?.LogInformation($"Social login success: {User.Identity.Name}");
				return Redirect(_webSite.Configuration.DefaultPage);
			}
			if (Request.HasFormContentType)
			{
				var provider = Request.Form["provider"].ToString();
				AuthenticationProperties properties = new AuthenticationProperties()
				{
					RedirectUri = _webSite.Configuration.Authentication.SocialLoginPath
				};
				_webSite.Logger?.LogInformation($"Social login {provider}");
				return new ChallengeResult(provider, properties);
			}
			return Login();
		}

		[HttpPost]
		public async Task<IActionResult> Logout(string returnUrl)
		{
			if (_webSite.IsAuthenticated(User))
			{
				_webSite.Logger?.LogInformation($"Logout {User.Identity.Name} and goto {returnUrl}");
				await HttpContext.SignOutAsync(_webSite.Configuration.Authentication.AuthenticationScheme);
				return Redirect(returnUrl);
			}
			return Index(_webSite.Configuration.DefaultPage);
		}
	}

}
