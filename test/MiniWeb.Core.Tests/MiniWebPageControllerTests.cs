using System;
using Xunit;
using Moq;
using MiniWeb.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;

namespace MiniWeb.Core.Tests
{
	public class MiniWebPageControllerTests : IClassFixture<MiniWebSiteFixture>
	{
		MiniWebSiteFixture _fixture;
		MiniWebPageController _pageController;
		public MiniWebPageControllerTests(MiniWebSiteFixture fixture)
		{
			_fixture = fixture;
			_pageController = new MiniWebPageController(fixture.MiniWeb);

			var request = new Mock<HttpRequest>();
			var response = new Mock<HttpResponse>();
			var context = new Mock<HttpContext>();
			context.SetupGet(x => x.Request).Returns(request.Object);
			context.SetupGet(x => x.Response).Returns(response.Object);
			request.SetupGet(f => f.Query).Returns(Mock.Of<IQueryCollection>());
			_pageController.ControllerContext.HttpContext = context.Object;
		}

		[Fact]
		public void Show404Page()
		{
			var result = _pageController.Index("/nonexistent").Result;
			var viewResult = Assert.IsType<ViewResult>(result);
			var model = Assert.IsAssignableFrom<ISitePage>(viewResult.ViewData.Model);
			Assert.Equal("404", model.Url);
		}

		[Fact]
		public void ShowLoginPage()
		{
			var result = _pageController.Login().Result;
			var viewResult = Assert.IsType<ViewResult>(result);
			var model = Assert.IsAssignableFrom<ISitePage>(viewResult.ViewData.Model);
			Assert.Equal("Login", model.Title);
		}
	}
}
