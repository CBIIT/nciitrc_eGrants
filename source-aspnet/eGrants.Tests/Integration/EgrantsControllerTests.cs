using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using eGrants.Common;
using eGrants.Controllers.Egrants;
using eGrants.DAL;
using eGrants.Models;
using eGrants.Repositories.Interfaces;
using eGrants.Services;
using eGrants.Services.Interfaces;
using eGrants.ViewModels;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Moq;
using Moq.Language.Flow;

using Xunit;

namespace eGrants.Tests.Integration
{
    public class EgrantsControllerTests
    {
        private readonly EgrantsController _controller;
        private readonly Mock<AppDbContext> _mockContext;
        private readonly Mock<IeGrantsService> _mockEGrantsService;
        private readonly Mock<ICommonRepository> _mockCommonRepository;
        private readonly Mock<ICommonService> _mockCommonService;
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<ISession> _mockSession;
        private readonly Mock<IDocumentService> _mockDocumentService;
        private readonly Mock<ISessionInfoService> _mockSessionInfoService;

        public EgrantsControllerTests()
        {
            //_mockContext = new Mock<AppDbContext>();
            _mockEGrantsService = new Mock<IeGrantsService>();
            _mockCommonService = new Mock<ICommonService>();
            _mockHttpContext = new Mock<HttpContext>();
            _mockSession = new Mock<ISession>();
            _mockDocumentService = new Mock<IDocumentService>();
            _mockSessionInfoService = new Mock<ISessionInfoService>();

            _controller = new EgrantsController(_mockEGrantsService.Object, _mockCommonService.Object, _mockDocumentService.Object, _mockSessionInfoService.Object);
            //_controller = new EgrantsController(_mockContext.Object, _mockEGrantsService.Object, _mockCommonService.Object);
            _mockHttpContext.Setup(x => x.Session).Returns(_mockSession.Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _mockHttpContext.Object
            };
        }

        [Fact]
        public void Go_to_default_ReturnsCorrectView()
        {
            var result = _controller.Go_to_default() as ViewResult;

            Assert.NotNull(result);
            Assert.Equal("~/Views/Shared/Go_to_Default.cshtml", result.ViewName);
        }

        [Fact]
        public async Task Index_ReturnsCorrectViewAndModel()
        {
            var codes = new List<AdminCodes>
            {
                new AdminCodes { admin_phs_org_code = "GM", profile = "GM" },
                new AdminCodes { admin_phs_org_code = "AG", profile = "AG" }
            };

            _mockCommonService.Setup(s => s.LoadAdminCodes()).ReturnsAsync(codes);

            var result = await _controller.Index() as ViewResult;

            Assert.NotNull(result);
            Assert.Equal("~/Views/Index.cshtml", result.ViewName);
            Assert.IsType<eGrantsSearchViewModel>(result.Model);
        }

        [Fact]
        public async Task by_str_ReturnsCorrectViewAndModel()
        {
            string testStr = "test";
            string testMode = "mode";
            var icbytes = Encoding.UTF8.GetBytes("NIC");
            var browserbytes = Encoding.UTF8.GetBytes("Chrome");
            var useridbytes = Encoding.UTF8.GetBytes("dehuffdc");

            _mockSession.Setup(s => s.TryGetValue("ic", out icbytes)).Returns(true);
            _mockSession.Setup(s => s.TryGetValue("browser", out browserbytes)).Returns(true);
            _mockSession.Setup(s => s.TryGetValue("userid", out useridbytes)).Returns(true);

            _mockSessionInfoService.Setup(s => s.GetSessionInfo(It.IsAny<ISession>())).Returns(new SessionInfo { Ic = "NIC", Browser = "Chrome", UserId = "dehuffdc" });

            var sessionInfo = _mockSessionInfoService.Object.GetSessionInfo(_mockSession.Object);

            var expectedModel = new eGrantsSearchViewModel();
            _mockEGrantsService.Setup(s => s.GetEgrantsByStrAsync(testStr, 0, 0, 0, sessionInfo))
                              .ReturnsAsync(expectedModel);

            var result = await _controller.by_str(testStr, testMode) as ViewResult;

            Assert.NotNull(result);
            Assert.Equal("~/Views/Index.cshtml", result.ViewName);
            Assert.Equal(expectedModel, result.Model);
        }

        [Fact]
        public async Task by_str_WithDifferentValidParams_ReturnsExpectedModel()
        {
            string testStr = "validSearch";
            string testMode = "advanced";
            var icbytes = Encoding.UTF8.GetBytes("NIC");
            var browserbytes = Encoding.UTF8.GetBytes("Firefox");
            var useridbytes = Encoding.UTF8.GetBytes("user123");

            _mockSession.Setup(s => s.TryGetValue("ic", out icbytes)).Returns(true);
            _mockSession.Setup(s => s.TryGetValue("browser", out browserbytes)).Returns(true);
            _mockSession.Setup(s => s.TryGetValue("userid", out useridbytes)).Returns(true);

            _mockSessionInfoService.Setup(s => s.GetSessionInfo(It.IsAny<ISession>())).Returns(new SessionInfo { Ic = "NIC", Browser = "Firefox", UserId = "user123" });

            var sessionInfo = _mockSessionInfoService.Object.GetSessionInfo(_mockSession.Object);

            var expectedModel = new eGrantsSearchViewModel();// { SearchTerm = testStr };
            _mockEGrantsService.Setup(s => s.GetEgrantsByStrAsync(testStr, 0, 0, 0, sessionInfo))
                               .ReturnsAsync(expectedModel);

            var result = await _controller.by_str(testStr, testMode) as ViewResult;

            Assert.NotNull(result);
            Assert.Equal("~/Views/Index.cshtml", result.ViewName);
            Assert.Equal(expectedModel, result.Model);
        }

        [Fact]
        public async Task by_str_NullSearchString_ReturnsEmptyModel()
        {
            string testStr = null;
            string testMode = "basic";
            var icbytes = Encoding.UTF8.GetBytes("NIC");
            var browserbytes = Encoding.UTF8.GetBytes("Edge");
            var useridbytes = Encoding.UTF8.GetBytes("tester");

            _mockSession.Setup(s => s.TryGetValue("ic", out icbytes)).Returns(true);
            _mockSession.Setup(s => s.TryGetValue("browser", out browserbytes)).Returns(true);
            _mockSession.Setup(s => s.TryGetValue("userid", out useridbytes)).Returns(true);

            _mockSessionInfoService.Setup(s => s.GetSessionInfo(It.IsAny<ISession>())).Returns(new SessionInfo { Ic = "NIC", Browser = "Edge", UserId = "tester" });

            var sessionInfo = _mockSessionInfoService.Object.GetSessionInfo(_mockSession.Object);

            var expectedModel = new eGrantsSearchViewModel(); // Assume service returns empty model on null input
            _mockEGrantsService.Setup(s => s.GetEgrantsByStrAsync(null, 0, 0, 0, sessionInfo))
                               .ReturnsAsync(expectedModel);

            var result = await _controller.by_str(testStr, testMode) as ViewResult;

            Assert.NotNull(result);
            Assert.Equal("~/Views/Index.cshtml", result.ViewName);
            Assert.Equal(expectedModel, result.Model);
        }

        //[Fact]
        //public async Task by_str_ServiceThrowsException_ReturnsErrorView()
        //{
        //    string testStr = "errorTrigger";
        //    string testMode = "mode";
        //    var icbytes = Encoding.UTF8.GetBytes("NIC");
        //    var browserbytes = Encoding.UTF8.GetBytes("Chrome");
        //    var useridbytes = Encoding.UTF8.GetBytes("dehuffdc");

        //    _mockSession.Setup(s => s.TryGetValue("ic", out icbytes)).Returns(true);
        //    _mockSession.Setup(s => s.TryGetValue("browser", out browserbytes)).Returns(true);
        //    _mockSession.Setup(s => s.TryGetValue("userid", out useridbytes)).Returns(true);

        //    _mockSessionInfoService.Setup(s => s.GetSessionInfo(It.IsAny<ISession>())).Returns(new SessionInfo { Ic = "NIC", Browser = "Chrome", UserId = "dehuffdc" });

        //    var sessionInfo = _mockSessionInfoService.Object.GetSessionInfo(_mockSession.Object);

        //    _mockEGrantsService.Setup(s => s.GetEgrantsByStrAsync(testStr, 0, 0, 0, sessionInfo))
        //                       .ThrowsAsync(new Exception("Database error"));

        //    var result = await _controller.by_str(testStr, testMode) as ViewResult;

        //    Assert.NotNull(result);
        //    Assert.Equal("Error", result.ViewName); // Assuming controller redirects to Error view
        //}

        [Fact]
        public async Task by_str_MissingSessionData_ReturnsDefaultModel()
        {
            string testStr = "test";
            string testMode = "mode";
            byte[] dummy;
            var icbytes = Encoding.UTF8.GetBytes("NIC");
            var browserbytes = Encoding.UTF8.GetBytes("Chrome");
            var useridbytes = Encoding.UTF8.GetBytes("dehuffdc");

            _mockSession.Setup(s => s.TryGetValue("ic", out icbytes)).Returns(true);
            _mockSession.Setup(s => s.TryGetValue("browser", out browserbytes)).Returns(true);
            _mockSession.Setup(s => s.TryGetValue("userid", out useridbytes)).Returns(true);

            _mockSessionInfoService.Setup(s => s.GetSessionInfo(It.IsAny<ISession>())).Returns(new SessionInfo { Ic = "", Browser = "", UserId = "" });

            var sessionInfo = _mockSessionInfoService.Object.GetSessionInfo(_mockSession.Object);

            var expectedModel = new eGrantsSearchViewModel();
            _mockEGrantsService.Setup(s => s.GetEgrantsByStrAsync(testStr, 0, 0, 0, sessionInfo))
                               .ReturnsAsync(expectedModel);

            var result = await _controller.by_str(testStr, testMode) as ViewResult;

            Assert.NotNull(result);
            Assert.Equal("~/Views/Index.cshtml", result.ViewName);
            Assert.Equal(expectedModel, result.Model);
        }
    }
}



