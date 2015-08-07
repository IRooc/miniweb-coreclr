using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.Mvc;
using Microsoft.Framework.Logging;

namespace MiniWeb.Core
{

	public class MiniWebPageController : Controller
	{
		private IMiniWebSite _webSite;

		public MiniWebPageController(IMiniWebSite website)
		{
			_webSite = website;
			if (!_webSite.Pages.Any())
			{
				_webSite.ReloadPages();
			}
		}

		public IActionResult Index()
		{
			_webSite.Logger?.LogInformation($"index action {Request.Path.Value}");
			if (Request.Query["reloadpages"] == "true")
			{
				_webSite.ReloadPages();
			}
			var url = Request.Path.Value;
			if (url == string.Empty || url == "/")
			{
				_webSite.Logger?.LogVerbose("Homepage");
				url = _webSite.Configuration.DefaultPage;
			}
			SitePage page = _webSite.GetPageByUrl(url, _webSite.IsAuthenticated(User));

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

		[HttpPost]
		public async Task<IActionResult> Logout(string returnUrl)
		{
			if (_webSite.IsAuthenticated(User))
			{
				await Context.Authentication.SignOutAsync(_webSite.Configuration.Authentication.AuthenticationScheme);
				return Redirect(returnUrl);
			}
			return Index();
		}

		public IActionResult SocialLogin()
		{
			if (_webSite.IsAuthenticated(User))
			{
				_webSite.Logger?.LogInformation("Social login success");
				return Redirect("~" + _webSite.Configuration.DefaultPage);
			}
			else if (Request.HasFormContentType)
			{
				var provider = Request.Form["provider"];
				_webSite.Logger?.LogInformation($"Social login {provider}");
				AuthenticationProperties properties = new AuthenticationProperties()
				{
					RedirectUri = _webSite.Configuration.Authentication.SocialLoginPath
				};
				properties.Items.Add("LoginProvider", provider);
				return new ChallengeResult(provider, properties);
			}
			return Redirect(_webSite.PageLogin.Url);
		}
		
		public IActionResult Login()
		{
			_webSite.Logger?.LogInformation("login action");
			return View(_webSite.Configuration.LoginView, _webSite.PageLogin);
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
				var identity = new ClaimsIdentity(claims, _webSite.Configuration.Authentication.AuthenticationType);
				var principal = new ClaimsPrincipal(identity);
				await Context.Authentication.SignInAsync(_webSite.Configuration.Authentication.AuthenticationScheme, principal);

				return Redirect("~" + _webSite.Configuration.DefaultPage);
			}

			return View(_webSite.Configuration.LoginView, _webSite.PageLogin);
		}
    }
}
