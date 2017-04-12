using MiniWeb.Core;
using System;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MiniWeb.Core.Tests
{
	public class MiniWebSiteFixture : IDisposable 
	{
		public IMiniWebSite MiniWeb {get;set;}
		public MiniWebSiteFixture() {

    		var mockEnvironment = new Mock<IHostingEnvironment>();
			var loggerFactory = new Mock<ILoggerFactory>();
			var contentStorage = new Mock<IMiniWebContentStorage>();
			var assetStorage = new Mock<IMiniWebAssetStorage>();
			Mock<ISitePage> homeSitePage = GetHomeSitePage();
            contentStorage.SetupGet(x => x.MiniWeb404Page).Returns(homeSitePage.Object);

			var configOptions = Options.Create(new MiniWebConfiguration());
			//public MiniWebSite(IHostingEnvironment env, ILoggerFactory loggerfactory, IMiniWebContentStorage storage, IMiniWebAssetStorage assetStorage,
			// 					   IOptions<MiniWebConfiguration> config)
			MiniWeb = new MiniWebSite(mockEnvironment.Object, loggerFactory.Object, contentStorage.Object, assetStorage.Object, configOptions);
		}

		public Mock<ISitePage> GetHomeSitePage() {
			var sitePage = new Mock<ISitePage>();
			sitePage.SetupGet(x => x.Url).Returns("/home");
			sitePage.SetupGet(x => x.Visible).Returns(true);
			return sitePage;
		}


		 public void Dispose()
		{
			// ... clean up test data from the database ...
		}
	}
}