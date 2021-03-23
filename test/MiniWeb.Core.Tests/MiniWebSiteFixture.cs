using MiniWeb.Core;
using System;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

namespace MiniWeb.Core.Tests
{
	public class MiniWebSiteFixture : IDisposable
	{
		public IMiniWebSite MiniWeb { get; set; }
		public MiniWebSiteFixture()
		{
			var hostingEnv = new Mock<IWebHostEnvironment>();
			var loggerFactory = new Mock<ILoggerFactory>();
			var contentStorage = new Mock<IMiniWebContentStorage>();
			var assetStorage = new Mock<IMiniWebAssetStorage>();
			var missingSitePage = Get404SitePage();
			var loginPage = GetLoginPage();

			contentStorage.Setup(x => x.MiniWeb404Page()).Returns(Task.FromResult(missingSitePage.Object));
			contentStorage.Setup(x => x.MiniWebLoginPage()).Returns(Task.FromResult(loginPage.Object));

			var configOptions = Options.Create(new MiniWebConfiguration());

			MiniWeb = new MiniWebSite(hostingEnv.Object, loggerFactory.Object, contentStorage.Object, assetStorage.Object, null, configOptions);
		}

		private Mock<ISitePage> GetLoginPage()
		{
			var sitePage = new Mock<ISitePage>();
			sitePage.SetupGet(x => x.Url).Returns("login");
			sitePage.SetupGet(x => x.Title).Returns("Login");
			sitePage.SetupGet(x => x.Visible).Returns(true);
			return sitePage;
		}

		public Mock<ISitePage> Get404SitePage()
		{
			var sitePage = new Mock<ISitePage>();
			sitePage.SetupGet(x => x.Url).Returns("404");
			sitePage.SetupGet(x => x.Visible).Returns(true);
			return sitePage;
		}


		public void Dispose()
		{
			// ... clean up test data from the database ...
		}
	}
}