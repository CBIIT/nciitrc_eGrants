using eGrant.Controllers;

using eGrants.Common.Enums;
using eGrants.Controllers;
using eGrants.DAL;
using eGrants.DTOs;
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

        // Constructs the InstitutionalFilesController with dependencies and optional session
        private InstitutionalFilesController CreateController(AppDbContext context, ISession session = null)
        {
            var scopeFactory = CreateScopeFactory();

            var repository = new InstitutionalFilesRepository(context, scopeFactory);
            var service = new InstitutionalFilesService(repository);
            var sessionInfoService = new SessionInfoService();

            var controller = new InstitutionalFilesController(service);
            var httpContext = new DefaultHttpContext();
            httpContext.Session = session;
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            return controller;
        }

        [Fact]
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

            var model = result.Model as InstitutionalFilesPageViewModel;
            Assert.NotNull(model);
            Assert.Equal(InstitutionalFilesPageAction.ShowDocs, model.Action);
            Assert.NotNull(model.SelectedInstitutionalOrg);
            Assert.Equal(testOrgId, model.SelectedInstitutionalOrg.OrgId);
            Assert.NotNull(model.CharacterIndices);
            Assert.NotNull(model.DocFiles);
        }

        [Fact]
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

            var model = Assert.IsType<InstitutionalFilesPageViewModel>(result.Model);
            Assert.Equal(InstitutionalFilesPageAction.ShowDocs, model.Action);
            Assert.NotNull(model.SelectedInstitutionalOrg);
            Assert.Equal(orgId, model.SelectedInstitutionalOrg.OrgId);
            Assert.NotNull(model.CharacterIndices);
            Assert.NotNull(model.DocFiles);
        }

        [Fact]
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
            var model = Assert.IsType<InstitutionalFilesPageViewModel>(result.Model);
            Assert.NotNull(model.DocFiles);
            Assert.Empty(model.DocFiles);
        }

        [Fact]
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
            var model = Assert.IsType<InstitutionalFilesPageViewModel>(result.Model);
            Assert.NotNull(model.CharacterIndices);
            Assert.False(model.CharacterIndices.Count == 0 || model.CharacterIndices.All(i => i == null));
        }

        [Fact]
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
    }
}
