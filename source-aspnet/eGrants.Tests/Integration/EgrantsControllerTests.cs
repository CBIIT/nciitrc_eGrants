using System.Text;

using eGrants.Controllers.Egrants;
using eGrants.DAL;
using eGrants.Models;
using eGrants.Repositories;
using eGrants.Services;
using eGrants.Tests.Utilities;
using eGrants.ViewModels;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

namespace eGrants.Tests.Integration
{
    public class EgrantsControllerTests
    {
        // Connection string to the development SQL Server instance
        private const string DevConnectionString = @"Data Source=NCIDB-D387-V.nci.nih.gov\\MSSQLEGRANTSQ,52000;Persist Security Info=True;Initial Catalog=EIM;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=45";

        // Creates a DbContext using the dev connection string
        private AppDbContext CreateDevDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(DevConnectionString)
                .Options;

            return new AppDbContext(options);
        }

        // Builds a scoped service provider with the manually created DbContext
        private IServiceScopeFactory CreateScopeFactory()
        {
            var services = new ServiceCollection();
            services.AddScoped(_ => CreateDevDbContext());
            var provider = services.BuildServiceProvider();
            return provider.GetRequiredService<IServiceScopeFactory>();
        }

        // Constructs the EgrantsController with all required services and session context
        private EgrantsController CreateController(AppDbContext context, ISession session = null)
        {
            var serviceScopeFactory = CreateScopeFactory();

            var eGrantsRepository = new eGrantsRepository(context, serviceScopeFactory);
            var documentRepository = new DocumentRepository(context, serviceScopeFactory);
            var commonRepository = new CommonRepository(context, serviceScopeFactory);

            var commonService = new CommonService(commonRepository);
            var eGrantsService = new eGrantsService(eGrantsRepository);
            var sessionInfoService = new SessionInfoService();
            var documentService = new DocumentService(documentRepository, sessionInfoService);

            var controller = new EgrantsController(eGrantsService, commonService, documentService, sessionInfoService);
            var httpContext = new DefaultHttpContext();
            httpContext.Session = session ?? new TestSession();
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            return controller;
        }

        [Fact]
        public void Go_to_default_ReturnsCorrectView()
        {
            // Verifies that Go_to_default returns the expected view path
            using var context = CreateDevDbContext();
            var controller = CreateController(context);

            var result = controller.Go_to_default() as ViewResult;

            Assert.NotNull(result);
            Assert.Equal("~/Views/Shared/Go_to_Default.cshtml", result.ViewName);
        }

        [Fact]
        public async Task Index_ReturnsCorrectViewAndModel()
        {
            // Tests Index action with valid session data
            using var context = CreateDevDbContext();
            var session = new TestSession();
            session.Set("userid", Encoding.UTF8.GetBytes("user123"));
            session.Set("ic", Encoding.UTF8.GetBytes("1"));

            var controller = CreateController(context, session);
            var result = await controller.Index() as ViewResult;

            Assert.NotNull(result);
            Assert.Equal("~/Views/Index.cshtml", result.ViewName);
            Assert.IsType<eGrantsSearchViewModel>(result.Model);
        }

        #region by_str controller method tests

        [Fact]
        public async Task by_str_ReturnsCorrectViewAndModel()
        {
            // Tests by_str with basic search string and session data
            using var context = CreateDevDbContext();
            var session = new TestSession();
            session.Set("ic", Encoding.UTF8.GetBytes("NIC"));
            session.Set("browser", Encoding.UTF8.GetBytes("Chrome"));
            session.Set("userid", Encoding.UTF8.GetBytes("dehuffdc"));

            var controller = CreateController(context, session);
            var result = await controller.by_str("test") as ViewResult;

            Assert.NotNull(result);
            Assert.Equal("~/Views/Index.cshtml", result.ViewName);
            Assert.IsType<eGrantsSearchViewModel>(result.Model);
        }

        [Fact]
        public async Task by_str_WithDifferentValidParams_ReturnsExpectedModel()
        {
            // Tests by_str with alternate valid parameters
            using var context = CreateDevDbContext();
            var session = new TestSession();
            session.Set("ic", Encoding.UTF8.GetBytes("NIC"));
            session.Set("browser", Encoding.UTF8.GetBytes("Firefox"));
            session.Set("userid", Encoding.UTF8.GetBytes("user123"));

            var controller = CreateController(context, session);
            var result = await controller.by_str("validSearch", "advanced") as ViewResult;

            Assert.NotNull(result);
            Assert.Equal("~/Views/Index.cshtml", result.ViewName);
            Assert.IsType<eGrantsSearchViewModel>(result.Model);
        }

        [Fact]
        public async Task by_str_NullSearchString_ReturnsEmptyModel()
        {
            // Tests by_str with null search string to validate fallback behavior
            using var context = CreateDevDbContext();
            var session = new TestSession();
            session.Set("ic", Encoding.UTF8.GetBytes("NIC"));
            session.Set("browser", Encoding.UTF8.GetBytes("Edge"));
            session.Set("userid", Encoding.UTF8.GetBytes("tester"));

            var controller = CreateController(context, session);

            var result = await controller.by_str(null, "basic") as ViewResult;
            Assert.NotNull(result);
            Assert.Equal("~/Views/Index.cshtml", result.ViewName);
            Assert.IsType<eGrantsSearchViewModel>(result.Model);
        }

        [Fact]
        public async Task by_str_MissingSessionData_ReturnsDefaultModel()
        {
            // Tests by_str with no session data to ensure default behavior
            using var context = CreateDevDbContext();
            var controller = CreateController(context);

            var result = await controller.by_str("125123", "mode") as ViewResult;

            Assert.NotNull(result);
            Assert.Equal("~/Views/Index.cshtml", result.ViewName);
            Assert.IsType<eGrantsSearchViewModel>(result.Model);
        }

        #endregion

        #region by_grant controller tests

        [Fact]
        public async Task by_grant_WithDefaultParameters_ReturnsCorrectViewAndModel()
        {
            // Tests by_grant action with default parameters and valid session
            using var context = CreateDevDbContext();
            var session = new TestSession();
            session.Set("userid", Encoding.UTF8.GetBytes("user123"));
            session.Set("ic", Encoding.UTF8.GetBytes("1"));

            var controller = CreateController(context, session);
            var result = await controller.by_grant() as ViewResult;

            // Verifies the view path and model type
            Assert.NotNull(result);
            Assert.Equal("~/Views/Index.cshtml", result.ViewName);
            Assert.IsType<eGrantsSearchViewModel>(result.Model);
        }

        [Fact]
        public async Task by_grant_WithSpecificGrantIdAndFilters_ReturnsPopulatedModel()
        {
            // Tests by_grant with specific grantId and filter parameters
            using var context = CreateDevDbContext();
            var session = new TestSession();
            session.Set("userid", Encoding.UTF8.GetBytes("user456"));
            session.Set("ic", Encoding.UTF8.GetBytes("2"));

            var controller = CreateController(context, session);
            var result = await controller.by_grant(
                grantId: 12345,
                package: "R01",
                categories: "Cancer",
                applsList: "A1,B2",
                years: "2021",
                mode: "full") as ViewResult;

            // Verifies the view and ensures model is correctly typed and populated
            Assert.NotNull(result);
            Assert.Equal("~/Views/Index.cshtml", result.ViewName);
            var model = Assert.IsType<eGrantsSearchViewModel>(result.Model);
            Assert.NotNull(model.ICList); // Ensures ICList is loaded
        }

        [Fact]
        public async Task by_grant_WithEmptySession_StillReturnsView()
        {
            // Tests by_grant behavior when session is empty or missing expected keys
            using var context = CreateDevDbContext();
            var session = new TestSession(); // No userid or ic set

            var controller = CreateController(context, session);
            var result = await controller.by_grant(grantId: 99999, categories: "All") as ViewResult;

            // Verifies fallback behavior still returns view and model
            Assert.NotNull(result);
            Assert.Equal("~/Views/Index.cshtml", result.ViewName);
            Assert.IsType<eGrantsSearchViewModel>(result.Model);
        }

        #endregion
        #region by_filter controller tests

        [Fact]
        public async Task by_filters_WithDefaultParameters_ReturnsCorrectViewAndModel()
        {
            // Tests by_filters with default parameters and valid session
            using var context = CreateDevDbContext();
            var session = new TestSession();
            session.Set("userid", Encoding.UTF8.GetBytes("user123"));
            session.Set("ic", Encoding.UTF8.GetBytes("1"));

            var controller = CreateController(context, session);
            var result = await controller.by_filters() as ViewResult;

            // Verifies the view path and model type
            Assert.NotNull(result);
            Assert.Equal("~/Views/Index.cshtml", result.ViewName);
            Assert.IsType<eGrantsSearchViewModel>(result.Model);
        }

        [Fact]
        public async Task by_filters_WithSpecificFilters_ReturnsPopulatedModel()
        {
            // Tests by_filters with specific fiscal year, mechanism, admin code, and serial number
            using var context = CreateDevDbContext();
            var session = new TestSession();
            session.Set("userid", Encoding.UTF8.GetBytes("user456"));
            session.Set("ic", Encoding.UTF8.GetBytes("2"));

            var controller = CreateController(context, session);
            var result = await controller.by_filters(
                fiscalYear: 2022,
                mechanism: "R01",
                adminCode: "CA",
                serialNum: 123456) as ViewResult;

            // Verifies the view and ensures model is correctly typed and populated
            Assert.NotNull(result);
            Assert.Equal("~/Views/Index.cshtml", result.ViewName);
            var model = Assert.IsType<eGrantsSearchViewModel>(result.Model);
            Assert.NotNull(model.ICList); // Ensures ICList is loaded
        }

        [Fact]
        public async Task by_filters_WithEmptySession_StillReturnsView()
        {
            // Tests by_filters behavior when session is empty or missing expected keys
            using var context = CreateDevDbContext();
            var session = new TestSession(); // No userid or ic set

            var controller = CreateController(context, session);
            var result = await controller.by_filters(fiscalYear: 2023) as ViewResult;

            // Verifies fallback behavior still returns view and model
            Assert.NotNull(result);
            Assert.Equal("~/Views/Index.cshtml", result.ViewName);
            Assert.IsType<eGrantsSearchViewModel>(result.Model);
        }

        #endregion
        #region LoadCategories controller tests

        [Fact]

        public async Task LoadCategories_ReturnsSerializedCategoryList()
        {
            // Arrange: create test DB context and session
            using var context = CreateDevDbContext();
            var session = new TestSession();
            session.Set("ic", Encoding.UTF8.GetBytes("NIC"));
            session.Set("browser", Encoding.UTF8.GetBytes("Chrome"));
            session.Set("userid", Encoding.UTF8.GetBytes("dehuffdc"));

            var controller = CreateController(context, session);

            // Seed test data if needed
            int testGrantId = 687129;
            string testYears = "All";

            // Act: call LoadCategories
            var resultJson = await controller.LoadCategories(testGrantId, testYears);

            // Assert: deserialize and validate result
            Assert.False(string.IsNullOrWhiteSpace(resultJson));

            var categoryStrings = JsonConvert.DeserializeObject<List<string>>(resultJson);
            Assert.NotNull(categoryStrings);
            Assert.All(categoryStrings, entry =>
            {
                Assert.Matches(@"^\d+:.+$", entry); // e.g., "101:Education"
            });

            // Optional: check specific known values if seeded
            Assert.Contains(categoryStrings, s => s.Contains("Greensheet"));
        }
        #endregion
    }
}
