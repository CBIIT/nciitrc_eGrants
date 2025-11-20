using System.Text;

using eGrants.Controllers.Egrants;
using eGrants.DAL;
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
    public class EgrantsDocsControllerTests
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

        private EgrantsDocController CreateController(AppDbContext context, ISession session = null, IDocumentService mockDocumentService = null)
        {
            var scopeFactory = CreateScopeFactory();

            var eGrantsRepository = new eGrantsRepository(context, scopeFactory);
            var documentRepository = new DocumentRepository(context, scopeFactory);
            var commonRepository = new CommonRepository(context, scopeFactory);

            var commonService = new CommonService(commonRepository);
            var eGrantsService = new eGrantsService(eGrantsRepository);
            var sessionInfoService = new SessionInfoService();
            var documentService = mockDocumentService ?? new DocumentService(documentRepository, sessionInfoService, commonRepository, eGrantsService);

            var controller = new EgrantsDocController(eGrantsService, commonService, documentService, sessionInfoService);
            var httpContext = new DefaultHttpContext();
            httpContext.Session = session;
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            return controller;
        }

        #region LoadSupplement Tests

        [Fact]
        public async Task LoadSupplement_ReturnsViewWithCorrectModel()
        {
            using var context = CreateDevDbContext();
            var session = new TestSession();
            session.SetString("UserId", "user123");
            session.SetString("Ic", "1");

            var controller = CreateController(context, session);
            var result = await controller.LoadSupplement("TestAct", 123);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SupplementObjectViewModel>(viewResult.Model);
            Assert.Equal(123, model.GrantID);
            Assert.Equal("TestAct", model.Act);
            Assert.NotEmpty(model.Supplement);
        }

        [Fact]
        public async Task LoadSupplement_NullSessionInfo_ThrowsException()
        {
            using var context = CreateDevDbContext();
            var controller = CreateController(context, session: null);

            await Assert.ThrowsAsync<NullReferenceException>(() =>
                controller.LoadSupplement("ajskkljfsa", 123));
        }

        [Fact]
        public async Task LoadSupplement_EmptySupplements_ReturnsViewWithEmptyList()
        {
            using var context = CreateDevDbContext();
            var session = new TestSession();
            session.SetString("userId", "user123");
            session.SetString("ic", "1");

            var controller = CreateController(context, session);
            var result = await controller.LoadSupplement("EmptyAct", -434242343); // Use a GrantID unlikely to have data

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SupplementObjectViewModel>(viewResult.Model);
            Assert.Empty(model.Supplement);
            Assert.Empty(model.FormerAppls);
        }

        [Fact]
        public async Task LoadSupplement_DocumentServiceThrowsException_PropagatesError()
        {
            using var context = CreateDevDbContext();
            var session = new TestSession();
            session.SetString("userId", "user123");
            session.SetString("ic", "1");

            var mockDocService = new Mock<IDocumentService>();
            mockDocService.Setup(s => s.loadFormerAppls(It.IsAny<int>()))
                          .ThrowsAsync(new Exception("Simulated failure"));

            var controller = CreateController(context, session, mockDocService.Object);

            // Use a GrantID or Act that triggers a known failure in DocumentService
            await Assert.ThrowsAsync<Exception>(() =>
                controller.LoadSupplement("TriggerErrorAct", 999998));
        }

        [Theory]
        [InlineData("", -1)]
        [InlineData(null, 0)]
        public async Task LoadSupplement_InvalidInputs_ReturnsView(string act, int grantId)
        {
            using var context = CreateDevDbContext();
            var session = new TestSession();
            session.SetString("UserId", "user123");
            session.SetString("Ic", "1");

            var controller = CreateController(context, session);
            var result = await controller.LoadSupplement(act, grantId);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SupplementObjectViewModel>(viewResult.Model);
            Assert.Equal(grantId, model.GrantID);
            Assert.Equal(act, model.Act);
        }

        #endregion

        #region doc_index_update_default tests
        [Fact]
        public async Task doc_index_update_default_ReturnsViewWithViewModel()
        {
            using var context = CreateDevDbContext();
            var session = new TestSession();
            session.SetString("UserId", "user123");
            session.SetString("Ic", "1");

            var controller = CreateController(context, session);

            int documentId = 123;
            string previousUrl = "http://example.com/previous";

            var result = await controller.doc_index_update_default(documentId, previousUrl);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("~/Views/Egrants/EgrantsDocUpdate.cshtml", viewResult.ViewName);
            var model = Assert.IsType<eGrantsDocUpdateViewModel>(viewResult.Model);
            Assert.NotNull(model);
        }

        [Fact]
        public async Task doc_index_update_default_NullSessionInfo_ThrowsException()
        {
            using var context = CreateDevDbContext();
            var controller = CreateController(context, session: null);

            await Assert.ThrowsAsync<NullReferenceException>(() =>
                controller.doc_index_update_default(999, "test.com"));
        }

        [Fact]
        public async Task doc_index_update_default_SetsCorrectPreviousUrlInViewModel()
        {
            using var context = CreateDevDbContext();
            var session = new TestSession();
            session.SetString("UserId", "user456");
            session.SetString("Ic", "2");

            var controller = CreateController(context, session);

            int documentId = 123;
            string previousUrl = "http://example.com/previous";

            var result = await controller.doc_index_update_default(documentId, previousUrl);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<eGrantsDocUpdateViewModel>(viewResult.Model);
            Assert.Equal(previousUrl, model.PreviousUrl);
        }
        #endregion

        #region doc_upload_default tests
        [Fact]
        public async Task doc_upload_default_ReturnsViewWithViewModel()
        {
            using var context = CreateDevDbContext();
            var session = new TestSession();
            session.SetString("UserId", "user678");
            session.SetString("Ic", "2");

            var controller = CreateController(context, session);

            int docId = 123;

            var result = await controller.doc_upload_default(docId);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("~/Views/Egrants/EgrantsDocUpload.cshtml", viewResult.ViewName);
            var model = Assert.IsType<eGrantsDocUploadViewModel>(viewResult.Model);
            Assert.NotNull(model);
        }

        [Fact]
        public async Task doc_upload_default_NullSessionInfo_ThrowsException()
        {
            using var context = CreateDevDbContext();
            var controller = CreateController(context, session: null);

            await Assert.ThrowsAsync<NullReferenceException>(() =>
                controller.doc_upload_default(999));
        }

        [Fact]
        public async Task doc_upload_default_CallsDocumentServiceWithCorrectDocId()
        {
            using var context = CreateDevDbContext();
            var session = new TestSession();
            session.SetString("UserId", "user678");
            session.SetString("Ic", "2");

            var mockDocumentService = new Mock<IDocumentService>();
            var expectedViewModel = new eGrantsDocUploadViewModel { DocId = 456 };

            mockDocumentService
                .Setup(d => d.DocUploadDefaultAsync(456))
                .ReturnsAsync(expectedViewModel);

            var controller = CreateController(context, session, mockDocumentService.Object);

            int docId = 456;

            var result = await controller.doc_upload_default(docId);

            mockDocumentService.Verify(d => d.DocUploadDefaultAsync(docId), Times.Once);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<eGrantsDocUploadViewModel>(viewResult.Model);
            Assert.Equal(docId, model.DocId);
        }
        #endregion

        #region doc_create_without_applid tests
        [Fact]
        public async Task doc_create_without_applid_ReturnsViewWithViewModel()
        {
            using var context = CreateDevDbContext();
            var session = new TestSession();
            session.SetString("UserId", "user678");
            session.SetString("Ic", "2");

            var controller = CreateController(context, session);

            string previousUrl = "test.com";

            var result = await controller.doc_create_without_applid(previousUrl);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("~/Views/Egrants/EgrantsDocCreate.cshtml", viewResult.ViewName);
            var model = Assert.IsType<eGrantsDocCreateViewModel>(viewResult.Model);
            Assert.NotNull(model);
        }

        [Fact]
        public async Task doc_create_without_applid_NullSessionInfo_ThrowsException()
        {
            using var context = CreateDevDbContext();
            var controller = CreateController(context, session: null);

            await Assert.ThrowsAsync<NullReferenceException>(() =>
                controller.doc_upload_default(999));
        }

        [Fact]
        public async Task doc_create_without_applid_SetsPreviousUrlInViewModel()
        {
            using var context = CreateDevDbContext();
            var session = new TestSession();
            session.SetString("UserId", "user321");
            session.SetString("Ic", "1");

            var controller = CreateController(context, session);

            string previousUrl = "test.com/previous";

            var result = await controller.doc_create_without_applid(previousUrl);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<eGrantsDocCreateViewModel>(viewResult.Model);
            Assert.Equal(previousUrl, model.PreviousUrl);
            Assert.NotNull(model);
        }
        #endregion

        #region doc_upload_by_ddrop tests
        [Fact]
        public async Task doc_upload_by_ddrop_ValidFile_ReturnsSuccessJson()
        {
            using var context = CreateDevDbContext();
            var session = new TestSession();
            session.SetString("UserId", "user123");
            session.SetString("Ic", "NCI");
            session.SetString("WebGrantUrl", "testserver");
            session.SetString("ImageServerUrl", "http://testserver/");
            session.SetString("EgrantsDocModifyRelativePath", "docs/modify/");

            var mockDocumentService = new Mock<IDocumentService>();
            var expectedResult = new DocumentCreateOrUploadResult
            {
                Success = true,
                Url = "http://testserver/docs/modify/123.pdf",
                Message = "Done! New document has been created"
            };

            mockDocumentService
                .Setup(d => d.DocUploadByDdropAsync(It.IsAny<IFormFile>(), 123, It.IsAny<SessionInfo>()))
                .ReturnsAsync(expectedResult);

            var controller = CreateController(context, session, mockDocumentService.Object);

            // Create a mock file
            var content = "Test file content";
            var fileName = "test.pdf";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            var formFile = new FormFile(stream, 0, stream.Length, "dropedfile", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/pdf"
            };

            var result = await controller.doc_upload_by_ddrop(formFile, 123);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonValue = jsonResult.Value;
            Assert.NotNull(jsonValue);

            var url = jsonValue.GetType().GetProperty("url")?.GetValue(jsonValue, null) as string;
            var message = jsonValue.GetType().GetProperty("message")?.GetValue(jsonValue, null) as string;

            Assert.Equal(expectedResult.Url, url);
            Assert.Equal(expectedResult.Message, message);
        }

        [Fact]
        public async Task doc_upload_by_ddrop_NullFile_ReturnsErrorJson()
        {
            using var context = CreateDevDbContext();
            var session = new TestSession();
            session.SetString("UserId", "user123");
            session.SetString("Ic", "NCI");

            var mockDocumentService = new Mock<IDocumentService>();
            var expectedResult = new DocumentCreateOrUploadResult
            {
                Success = false,
                Url = null,
                Message = "Error while uploading the files."
            };

            mockDocumentService
                .Setup(d => d.DocUploadByDdropAsync(null, 123, It.IsAny<SessionInfo>()))
                .ReturnsAsync(expectedResult);

            var controller = CreateController(context, session, mockDocumentService.Object);

            var result = await controller.doc_upload_by_ddrop(null, 123);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonValue = jsonResult.Value;
            Assert.NotNull(jsonValue);

            var url = jsonValue.GetType().GetProperty("url")?.GetValue(jsonValue, null);
            var message = jsonValue.GetType().GetProperty("message")?.GetValue(jsonValue, null) as string;

            Assert.Null(url);
            Assert.Equal(expectedResult.Message, message);
        }

        #endregion
    }
}
