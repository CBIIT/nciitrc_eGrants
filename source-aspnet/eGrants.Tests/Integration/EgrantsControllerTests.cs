using System.Text;

using eGrants.Controllers.Egrants;
using eGrants.Services;
using eGrants.Tests.Infrastructure;
using eGrants.Tests.Utilities;
using eGrants.ViewModels;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eGrants.Tests.Integration
{
    public class EgrantsControllerTests
    {
        public EgrantsController _controller;
        public TestDbContext _context;

        public EgrantsControllerTests()
        {

        }

        //public EgrantsControllerTests()
        //{
        //    var options = new DbContextOptionsBuilder<TestDbContext>()
        //        .UseInMemoryDatabase("EgrantsTestDb")
        //        .Options;

        //    _context = new TestDbContext(options);

        //    // Seed data
        //    _context.AdminCodes.AddRange(
        //        new AdminCodes { admin_phs_org_code = "GM", profile = "GM" },
        //        new AdminCodes { admin_phs_org_code = "AG", profile = "AG" }
        //    );
        //    _context.SaveChanges();

        //    var eGrantsRepository = new TestEGrantsRepository(_context);
        //    var documentRepository = new TestDocumentRepository(_context);
        //    var commonRepository = new TestCommonRepository(_context);

        //    var commonService = new CommonService(commonRepository);
        //    var eGrantsService = new eGrantsService(eGrantsRepository); // Replace with your actual implementation
        //    var sessionInfoService = new SessionInfoService(); // Replace with your actual implementation
        //    var documentService = new DocumentService(documentRepository, sessionInfoService);       // Replace with your actual implementation

        //    _controller = new EgrantsController(eGrantsService, commonService, documentService, sessionInfoService);

        //    var httpContext = new DefaultHttpContext();
        //    httpContext.Session = new TestSession(); // Custom ISession implementation
        //    _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        //}

        [Fact]
        public void Go_to_default_ReturnsCorrectView()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession(); // optional
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var result = _controller.Go_to_default() as ViewResult;

            Assert.NotNull(result);
            Assert.Equal("~/Views/Shared/Go_to_Default.cshtml", result.ViewName);
        }

        [Fact]
        public async Task Index_ReturnsCorrectViewAndModel()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase("EgrantsTestDb")
                .Options;

            var _context = new TestDbContext(options);

            // Seed data
            //_context.AdminCodes.AddRange(
            //    new AdminCodes { admin_phs_org_code = "GM", profile = "GM" },
            //    new AdminCodes { admin_phs_org_code = "AG", profile = "AG" }
            //);
            //_context.SaveChanges();

            var eGrantsRepository = new TestEGrantsRepository(_context);
            var documentRepository = new TestDocumentRepository(_context);
            var commonRepository = new TestCommonRepository(_context);

            var commonService = new CommonService(commonRepository);
            var eGrantsService = new eGrantsService(eGrantsRepository); // Replace with your actual implementation
            var sessionInfoService = new SessionInfoService(); // Replace with your actual implementation
            var documentService = new DocumentService(documentRepository, sessionInfoService);       // Replace with your actual implementation

            var controller2 = new EgrantsController(eGrantsService, commonService, documentService, sessionInfoService);

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession(); // Custom ISession implementation
            httpContext.Session.SetString("UserId", "user123");
            httpContext.Session.SetString("Ic", "1");
            controller2.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = await controller2.Index() as ViewResult;

            Assert.NotNull(result);
            Assert.Equal("~/Views/Index.cshtml", result.ViewName);
            Assert.IsType<eGrantsSearchViewModel>(result.Model);
        }

        [Fact]
        public async Task by_str_ReturnsCorrectViewAndModel()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase("EgrantsTestDb")
                .Options;

            var _context = new TestDbContext(options);

            // Seed data
            //_context.AdminCodes.AddRange(
            //    new AdminCodes { admin_phs_org_code = "GM", profile = "GM" },
            //    new AdminCodes { admin_phs_org_code = "AG", profile = "AG" }
            //);
            //_context.SaveChanges();

            var eGrantsRepository = new TestEGrantsRepository(_context);
            var documentRepository = new TestDocumentRepository(_context);
            var commonRepository = new TestCommonRepository(_context);

            var commonService = new CommonService(commonRepository);
            var eGrantsService = new eGrantsService(eGrantsRepository); // Replace with your actual implementation
            var sessionInfoService = new SessionInfoService(); // Replace with your actual implementation
            var documentService = new DocumentService(documentRepository, sessionInfoService);       // Replace with your actual implementation

            var controller2 = new EgrantsController(eGrantsService, commonService, documentService, sessionInfoService);

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession(); // Custom ISession implementation
            httpContext.Session.SetString("Ic", "NIC");
            httpContext.Session.SetString("Browser", "Chrome");
            httpContext.Session.SetString("UserId", "dehuffdc");

            controller2.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = await controller2.by_str("test") as ViewResult;

            Assert.NotNull(result);
            Assert.Equal("~/Views/Index.cshtml", result.ViewName);
            Assert.IsType<eGrantsSearchViewModel>(result.Model);
        }

        [Fact]
        public async Task by_str_WithDifferentValidParams_ReturnsExpectedModel()
        {
            var session = _controller.HttpContext.Session;
            session.Set("ic", Encoding.UTF8.GetBytes("NIC"));
            session.Set("browser", Encoding.UTF8.GetBytes("Firefox"));
            session.Set("userid", Encoding.UTF8.GetBytes("user123"));

            var result = await _controller.by_str("validSearch", "advanced") as ViewResult;

            Assert.NotNull(result);
            Assert.Equal("~/Views/Index.cshtml", result.ViewName);
            Assert.IsType<eGrantsSearchViewModel>(result.Model);
        }

        [Fact]
        public async Task by_str_NullSearchString_ReturnsEmptyModel()
        {
            var session = _controller.HttpContext.Session;
            session.Set("ic", Encoding.UTF8.GetBytes("NIC"));
            session.Set("browser", Encoding.UTF8.GetBytes("Edge"));
            session.Set("userid", Encoding.UTF8.GetBytes("tester"));

            var result = await _controller.by_str(null, "basic") as ViewResult;

            Assert.NotNull(result);
            Assert.Equal("~/Views/Index.cshtml", result.ViewName);
            Assert.IsType<eGrantsSearchViewModel>(result.Model);
        }

        [Fact]
        public async Task by_str_MissingSessionData_ReturnsDefaultModel()
        {
            var result = await _controller.by_str("test", "mode") as ViewResult;

            Assert.NotNull(result);
            //Assert.Equal("~/Views/Index.cshtml", result.ViewName);
            //Assert.IsType<eGrantsSearchViewModel>(result.Model);
        }
    }
}
