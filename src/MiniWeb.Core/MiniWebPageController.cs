using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Mvc;
using Microsoft.Framework.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Http.Authentication;
using System.Collections.Generic;

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
				Response.Redirect(page.Pages.First().Url);
			}
			if (page.Url == "404")
			{
				Response.StatusCode = 404;
			}

			return View(page.Template, page);
		}

		[HttpPost]
		public IActionResult Logout(string returnUrl)
		{
			if (_webSite.IsAuthenticated(User))
			{
				Context.Authentication.SignOutAsync(_webSite.Configuration.AuthenticationScheme).Wait();
				return Redirect("~" + returnUrl);
			}
			return Index();
		}

		public IActionResult SocialLogin()
		{

			return View(_webSite.Configuration.LoginView, _webSite.PageLogin);
		}

		public IActionResult Login()
		{
			_webSite.Logger?.LogInformation("login action");
			return View(_webSite.Configuration.LoginView, _webSite.PageLogin);
		}

		[HttpPost]
		public IActionResult Login(string username, string password, bool remember = false)
		{
			_webSite.Logger?.LogInformation("login post");
			if (password == null && username == null)
			{
				var provider = Request.Form["provider"];
				AuthenticationProperties properties = new AuthenticationProperties()
				{
					RedirectUri = "/miniweb/loginsoc"
				};
				properties.Items.Add("LoginProvider", provider);
				return new ChallengeResult(provider, properties);
			}
			if (_webSite.Authenticate(username, password))
			{
				var claims = new[] {
					new Claim(ClaimTypes.Name, username),
					new Claim(ClaimTypes.Role, _webSite.Configuration.MiniWebCmsRoleValue)
				};

				_webSite.Logger?.LogInformation($"signing in as :{username}");
				// use ApplicationCookieAuthenticationType so user.IsSignedIn works...
				var identity = new ClaimsIdentity(claims, IdentityOptions.ApplicationCookieAuthenticationType);
				var principal = new ClaimsPrincipal(identity);
				Context.Authentication.SignInAsync(_webSite.Configuration.AuthenticationScheme, principal).Wait();

				return Redirect("~" + _webSite.Configuration.DefaultPage);
			}

			return View(_webSite.Configuration.LoginView, _webSite.PageLogin);
		}
		//[HttpPost]
		//public IActionResult Login(string provider)
		//{
		//	AuthenticationProperties properties = new AuthenticationProperties()
		//	{
		//		RedirectUri = "/miniweb/login"
		//	};
  //          properties.Items.Add("LoginProvider", provider);
		//	return new ChallengeResult(provider, properties);
  //      }
    }
}
