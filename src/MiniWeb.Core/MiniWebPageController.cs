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
			if (Request.Query["reloadpages"] == "true")
			{
				_webSite.ReloadPages();
				_webSite.ReloadAssets();
			}
			if (string.IsNullOrWhiteSpace(url) || url == "/")
			{
				_webSite.Logger?.LogDebug("Homepage");
				url = _webSite.Configuration.DefaultPage;
			}
			ISitePage page = _webSite.GetPageByUrl(url, _webSite.IsAuthenticated(User));
			if (page.Url != url && $"{page.Url}.{_webSite.Configuration.PageExtension}" != url && (page.Url != "404"))
			{
				if (!string.IsNullOrWhiteSpace(_webSite.Configuration.PageExtension))
				{
					return Redirect($"/{page.Url}.{_webSite.Configuration.PageExtension}");
				}
				return Redirect($"/{page.Url}");
			}
			ViewBag.CurrentUrl = page.Url;
			if (_webSite.Configuration.RedirectToFirstSub && page.Pages.Any())
			{
				return Redirect(page.Pages.First().Url);
			}
			if (page.Url == "404")
			{
				Response.StatusCode = 404;
			}

			return View(page.Template, page);
		}

		public IActionResult Login()
		{
			_webSite.Logger?.LogInformation("login action");
			var page = _webSite.ContentStorage.MiniWebLoginPage;
			ViewBag.CurrentUrl = page.Url;

			return View(_webSite.Configuration.LoginView, page);
		}

		[HttpPost]
		public async Task<IActionResult> Login(string username, string password, bool remember = false)
		{
			_webSite.Logger?.LogInformation("login post");
			if (_webSite.Authenticate(username, password))
			{
				var claims = new[] {
					new Claim(ClaimTypes.Name, username),
					new Claim(ClaimTypes.Role, MiniWebAuthentication.MiniWebCmsRoleValue)
				};

				_webSite.Logger?.LogInformation($"signing in as :{username}");
				// use ApplicationCookieAuthenticationType so user.IsSignedIn works...
				var identity = new ClaimsIdentity(claims,_webSite.Configuration.Authentication.AuthenticationScheme);
				var principal = new ClaimsPrincipal(identity);
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
				_webSite.Logger?.LogInformation($"Social login {provider}");
				AuthenticationProperties properties = new AuthenticationProperties()
				{
					RedirectUri = _webSite.Configuration.Authentication.SocialLoginPath
				};
				properties.Items.Add("LoginProvider", provider);
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
