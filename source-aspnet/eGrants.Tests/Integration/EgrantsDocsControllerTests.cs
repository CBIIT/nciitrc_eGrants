using eGrants.Controllers.Egrants;
using eGrants.Services;
using eGrants.Tests.Infrastructure;
using eGrants.Tests.Utilities;
using eGrants.ViewModels;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace eGrants.Tests.Integration
{
    public class EgrantsDocsControllerTests
    {
        //private readonly EgrantsDocController _eGrantsDocController;
        //private readonly Mock<AppDbContext> _mockContext;
        //private readonly Mock<IeGrantsService> _eGrantsServiceMock;
        //private readonly Mock<ICommonRepository> _mockCommonRepository;
        //private readonly Mock<ICommonService> _mockCommonService;
        //private readonly Mock<HttpContext> _mockHttpContext;
        //private readonly Mock<ISession> _mockSession;
        //private readonly Mock<IDocumentService> _documentServiceMock;
        //private readonly Mock<ISessionInfoService> _sessionInfoServiceMock;

        public EgrantsDocsControllerTests()
        {

        }

        #region LoadSupplement Tests

        [Fact]
        public async Task LoadSupplement_ReturnsViewWithCorrectModel_Integration()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase("TestSupplementDb")
                .Options;

            using var context = new TestDbContext(options);

            // Seed test data
            await TestDataSeeder.SeedTestDataAsync(context);

            // Create real service instances using the seeded context
            var eGrantsRepository = new TestEGrantsRepository(context);
            var documentRepository = new TestDocumentRepository(context);
            var commonRepository = new TestCommonRepository(context);

            var sessionInfoService = new SessionInfoService();
            var eGrantsService = new eGrantsService(eGrantsRepository);
            var documentService = new DocumentService(documentRepository, sessionInfoService);
            var commonService = new CommonService(commonRepository);

            var controller = new EgrantsDocController(eGrantsService, commonService, documentService, sessionInfoService);

            //// Simulate session if needed
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession(); // optional
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await controller.LoadSupplement("TestAct", 123);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SupplementObjectViewModel>(viewResult.Model);
            Assert.Equal(123, model.GrantID);
            Assert.Equal("TestAct", model.Act);
            Assert.NotEmpty(model.Supplement);
        }


        [Fact]
        public async Task LoadSupplement_NullSessionInfo_ThrowsException_Integration()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase("TestSupplementDb_NullSession")
                .Options;

            using var context = new TestDbContext(options);

            var eGrantsRepository = new TestEGrantsRepository(context);
            var documentRepository = new TestDocumentRepository(context);
            var commonRepository = new TestCommonRepository(context);

            var sessionInfoService = new SessionInfoService(); // returns null if session is empty
            var eGrantsService = new eGrantsService(eGrantsRepository);
            var documentService = new DocumentService(documentRepository, sessionInfoService);
            var commonService = new CommonService(commonRepository);

            var controller = new EgrantsDocController(eGrantsService, commonService, documentService, sessionInfoService);

            var httpContext = new DefaultHttpContext();
            httpContext.Session = null; // Simulate missing session
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() =>
                controller.LoadSupplement("TestAct", 123));
        }

        [Fact]
        public async Task LoadSupplement_EmptySupplements_ReturnsViewWithEmptyList_Integration()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase("TestSupplementDb_Empty")
                .Options;

            using var context = new TestDbContext(options);

            var eGrantsRepository = new TestEGrantsRepository(context);
            var documentRepository = new TestDocumentRepository(context);
            var commonRepository = new TestCommonRepository(context);

            var sessionInfoService = new SessionInfoService();
            var eGrantsService = new eGrantsService(eGrantsRepository);
            var documentService = new DocumentService(documentRepository, sessionInfoService);
            var commonService = new CommonService(commonRepository);

            var controller = new EgrantsDocController(eGrantsService, commonService, documentService, sessionInfoService);

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();
            httpContext.Session.SetString("UserId", "user123");
            httpContext.Session.SetString("Ic", "1");
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await controller.LoadSupplement("TestAct", 123);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SupplementObjectViewModel>(viewResult.Model);
            Assert.Empty(model.Supplement);
            Assert.Empty(model.FormerAppls);
        }


        [Fact]
        public async Task LoadSupplement_DocumentServiceThrowsException_PropagatesError_Integration()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase("TestSupplementDb_Exception")
                .Options;

            using var context = new TestDbContext(options);

            var eGrantsRepository = new TestEGrantsRepository(context);
            var documentRepository = new TestDocumentRepository(context, true);
            var commonRepository = new TestCommonRepository(context);

            var sessionInfoService = new SessionInfoService();
            var eGrantsService = new eGrantsService(eGrantsRepository);
            var documentService = new DocumentService(documentRepository, sessionInfoService);
            var commonService = new CommonService(commonRepository);

            var controller = new EgrantsDocController(eGrantsService, commonService, documentService, sessionInfoService);

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();
            httpContext.Session.SetString("UserId", "user123");
            httpContext.Session.SetString("Ic", "1");
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                controller.LoadSupplement("TestAct", 123));
        }


        [Theory]
        [InlineData("", -1)]
        [InlineData(null, 0)]
        public async Task LoadSupplement_InvalidInputs_ReturnsView_Integration(string act, int grantId)
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase("TestSupplementDb_InvalidInputs")
                .Options;

            using var context = new TestDbContext(options);

            var eGrantsRepository = new TestEGrantsRepository(context);
            var documentRepository = new TestDocumentRepository(context);
            var commonRepository = new TestCommonRepository(context);

            var sessionInfoService = new SessionInfoService();
            var eGrantsService = new eGrantsService(eGrantsRepository);
            var documentService = new DocumentService(documentRepository, sessionInfoService);
            var commonService = new CommonService(commonRepository);

            var controller = new EgrantsDocController(eGrantsService, commonService, documentService, sessionInfoService);

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();
            httpContext.Session.SetString("UserId", "user123");
            httpContext.Session.SetString("Ic", "1");
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await controller.LoadSupplement(act, grantId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SupplementObjectViewModel>(viewResult.Model);
            Assert.Equal(grantId, model.GrantID);
            Assert.Equal(act, model.Act);
        }


        #endregion
    }
}
