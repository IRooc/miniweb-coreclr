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
        public MiniWebPageControllerTests(MiniWebSiteFixture fixture){
            _fixture = fixture;
            _pageController = new MiniWebPageController(fixture.MiniWeb);

            var request = new Mock<HttpRequest>();
            var context = new Mock<HttpContext>();
            context.SetupGet(x => x.Request).Returns(request.Object);
            request.SetupGet(f => f.Query).Returns(Mock.Of<IQueryCollection>());
            _pageController.ControllerContext.HttpContext = context.Object;
        }

        [Fact]
        public void RootPage()
        {
            var result = _pageController.Index("/");
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ISitePage>(viewResult.ViewData.Model);
            Assert.Equal("/home", model.Url);
        }
    }
}
