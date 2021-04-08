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

		public async Task<IActionResult> Index(string url)
		{
			_webSite.Logger?.LogDebug($"index action {url}");

			var result = await _webSite.GetPageByUrl(url, User);

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

		public async Task<IActionResult> Login()
		{
			_webSite.Logger?.LogInformation("login action");
			var model =  await _webSite.ContentStorage.MiniWebLoginPage();
			return base.View(_webSite.Configuration.LoginView, model);
		}

		[HttpPost]
		public async Task<IActionResult> Login(string username, string password)
		{
			_webSite.Logger?.LogInformation("login post");
			if (await _webSite.Authenticate(username, password))
			{
				var principal = _webSite.GetClaimsPrincipal(username);
				await HttpContext.SignInAsync(_webSite.Configuration.Authentication.AuthenticationScheme, principal);
				return Redirect(_webSite.Configuration.DefaultPage);
			}
			ViewBag.ErrorMessage = $"Failed to login as {username}";
			return await Login();
		}

		public async Task<IActionResult> SocialLogin()
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
			return await Login();
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
			return await Index(_webSite.Configuration.DefaultPage);
		}
	}
}
