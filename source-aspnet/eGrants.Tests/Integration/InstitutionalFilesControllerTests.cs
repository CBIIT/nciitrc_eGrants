using eGrant.Controllers;

using eGrants.Common.Enums;
using eGrants.Controllers;
using eGrants.DAL;
using eGrants.DTOs;
using eGrants.Models;
using eGrants.Repositories;
using eGrants.Services;
using eGrants.Services.Interfaces;
using eGrants.Tests.Utilities;
using eGrants.ViewModels;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Moq;

namespace eGrants.Tests.Integration
{
    public class InstitutionalFilesControllerTests
    {

        // Creates a DbContext using the dev connection string
        private AppDbContext CreateDevDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(TestDatabase.ConnectionString)
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

        // Constructs the InstitutionalFilesController with dependencies and optional session
        private InstitutionalFilesController CreateController(AppDbContext context, ISession session = null)
        {
            var scopeFactory = CreateScopeFactory();

            var repository = new InstitutionalFilesRepository(context, scopeFactory);
            var service = new InstitutionalFilesService(repository, context);
            var sessionInfoService = new SessionInfoService();

            var controller = new InstitutionalFilesController(service, sessionInfoService);
            var httpContext = new DefaultHttpContext();
            httpContext.Session = session;
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            return controller;
        }

        [DbFact]
        public async Task Show_Docs_WithValidOrgId_ReturnsCorrectViewAndModel()
        {
            // Arrange
            using var context = CreateDevDbContext();
            var controller = CreateController(context);

            // Use a known orgId and orgName that exist in the dev database
            int testOrgId = 1;
            string testOrgName = "Test Organization";

            // Act
            var result = await controller.Show_Docs(testOrgId, testOrgName) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("~/Views/eGrants/InstitutionalFilesIndex.cshtml", result.ViewName);

            var model = result.Model as InstitutionalFilesPage;
            Assert.NotNull(model);
            Assert.Equal(InstitutionalFilesPageAction.ShowDocs, model.Action);
            Assert.NotNull(model.SelectedInstitutionalOrg);
            Assert.Equal(testOrgId, model.SelectedInstitutionalOrg.OrgId);
            Assert.NotNull(model.CharacterIndices);
            Assert.NotNull(model.DocFiles);
        }

        [DbFact]
        public async Task Show_Docs_WithValidOrgIdAndName_ReturnsExpectedViewAndModel()
        {
            // Arrange
            using var context = CreateDevDbContext();
            var controller = CreateController(context);
            int orgId = 1;
            string orgName = "Test Organization";

            // Act
            var result = await controller.Show_Docs(orgId, orgName) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("~/Views/eGrants/InstitutionalFilesIndex.cshtml", result.ViewName);

            var model = Assert.IsType<InstitutionalFilesPage>(result.Model);
            Assert.Equal(InstitutionalFilesPageAction.ShowDocs, model.Action);
            Assert.NotNull(model.SelectedInstitutionalOrg);
            Assert.Equal(orgId, model.SelectedInstitutionalOrg.OrgId);
            Assert.NotNull(model.CharacterIndices);
            Assert.NotNull(model.DocFiles);
        }

        [DbFact]
        public async Task Show_Docs_WithOrgHavingNoDocs_ReturnsEmptyDocList()
        {
            // Arrange
            using var context = CreateDevDbContext();
            var controller = CreateController(context);
            int orgId = 999; // Use a known orgId with no docs
            string orgName = "Empty Org";

            // Act
            var result = await controller.Show_Docs(orgId, orgName) as ViewResult;

            // Assert
            var model = Assert.IsType<InstitutionalFilesPage>(result.Model);
            Assert.NotNull(model.DocFiles);
            Assert.Empty(model.DocFiles);
        }

        [DbFact]
        public async Task Show_Docs_WithMissingCharacterIndices_ReturnsEmptyIndices()
        {
            // Arrange
            using var context = CreateDevDbContext();
            var controller = CreateController(context);
            int orgId = 1;
            string orgName = "Test Org";

            // Act
            var result = await controller.Show_Docs(orgId, orgName) as ViewResult;

            // Assert
            var model = Assert.IsType<InstitutionalFilesPage>(result.Model);
            Assert.NotNull(model.CharacterIndices);
            Assert.False(model.CharacterIndices.Count == 0 || model.CharacterIndices.All(i => i == null));
        }

        [DbFact]
        public async Task Show_Docs_ReturnsExpectedViewPath()
        {
            // Arrange
            using var context = CreateDevDbContext();
            var controller = CreateController(context);
            int orgId = 1;
            string orgName = "Test Org";

            // Act
            var result = await controller.Show_Docs(orgId, orgName) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("~/Views/eGrants/InstitutionalFilesIndex.cshtml", result.ViewName);
        }

        // Builds the controller with a mocked service and seeded session; no database needed.
        private static InstitutionalFilesController CreateControllerWithMockedService(
            IInstitutionalFilesService service, string ic = "NCI", string userId = "testuser")
        {
            var session = new TestSession();
            session.Set("ic", System.Text.Encoding.UTF8.GetBytes(ic));
            session.Set("userid", System.Text.Encoding.UTF8.GetBytes(userId));

            var controller = new InstitutionalFilesController(service, new SessionInfoService());
            var httpContext = new DefaultHttpContext { Session = session };
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            return controller;
        }

        [Fact]
        public async Task Update_Doc_WithValidCategory_CallsServiceWithProvidedValues()
        {
            var service = new Mock<IInstitutionalFilesService>();
            service
                .Setup(s => s.UpdateDocument(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("done");

            var controller = CreateControllerWithMockedService(service.Object);

            await controller.Update_Doc(
                category_id: 5,
                start_date: "2024-01-01",
                end_date: "2024-12-31",
                comments: "some notes",
                doc_id: 10);

            service.Verify(s => s.UpdateDocument(
                10, 5, "2024-01-01", "2024-12-31", "NCI", "testuser", "some notes"), Times.Once);
        }

        [Fact]
        public async Task Update_Doc_WithNullOptionalStrings_PassesEmptyStringsToService()
        {
            var service = new Mock<IInstitutionalFilesService>();
            service
                .Setup(s => s.UpdateDocument(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("done");

            var controller = CreateControllerWithMockedService(service.Object);

            await controller.Update_Doc(
                category_id: 7,
                start_date: null,
                end_date: null,
                comments: null,
                doc_id: 42);

            // The controller's ?? "" guards must convert nulls to empty strings before the
            // values reach the service (and ultimately ADO.NET).
            service.Verify(s => s.UpdateDocument(
                42, 7, "", "", "NCI", "testuser", ""), Times.Once);
        }

        [Fact]
        public async Task Update_Doc_WithZeroCategory_DoesNotCallServiceAndSetsMessage()
        {
            var service = new Mock<IInstitutionalFilesService>();

            var controller = CreateControllerWithMockedService(service.Object);

            await controller.Update_Doc(
                category_id: 0,
                start_date: "2024-01-01",
                end_date: "2024-12-31",
                comments: "ignored",
                doc_id: 10);

            service.Verify(s => s.UpdateDocument(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);

            Assert.Equal("You have not specified information correctly.", controller.ViewData["Message"]);
        }
    }
}
