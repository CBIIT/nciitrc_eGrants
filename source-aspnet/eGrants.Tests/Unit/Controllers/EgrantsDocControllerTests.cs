using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using eGrants.Controllers.Egrants;
using eGrants.DAL;
using eGrants.Common;
using eGrants.Models;
using eGrants.Services;
using eGrants.Services.Interfaces;
using eGrants.Tests.Utilities;
using eGrants.ViewModels;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Moq;

using Xunit;

namespace eGrants.Tests.Unit.Controllers
{
    /// <summary>
    /// Mock-based unit tests for <see cref="EgrantsDocController"/>.
    ///
    /// These replace the previous database-dependent integration tests for the
    /// null-session scenarios. All collaborators are mocked so the tests are
    /// deterministic and require no SQL Server connectivity.
    /// </summary>
    public class EgrantsDocControllerTests
    {
        private readonly Mock<IeGrantsService> _eGrantsService = new();
        private readonly Mock<ICommonService> _commonService = new();
        private readonly Mock<IDocumentService> _documentService = new();
        private readonly Mock<IApplService> _applService = new();

        /// <summary>
        /// Builds a controller with mocked services and the supplied session.
        /// Uses the real <see cref="SessionInfoService"/> so that session access
        /// behavior (including the null-session path) is exercised faithfully.
        /// </summary>
        private EgrantsDocController CreateController(ISession session, EgrantsCommon egrantsCommon = null)
        {
            var sessionInfoService = new SessionInfoService();

            var controller = new EgrantsDocController(
                _eGrantsService.Object,
                _commonService.Object,
                _documentService.Object,
                sessionInfoService,
                _applService.Object,
                null,
                egrantsCommon);

            var httpContext = new DefaultHttpContext();
            httpContext.Session = session;
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            return controller;
        }

        #region doc_index_update_default (uses the session)

        [Fact]
        public async Task doc_index_update_default_ReturnsViewWithModel_WhenSessionPresent()
        {
            // Arrange
            var expectedModel = new eGrantsDocUpdateViewModel { PreviousUrl = "http://example.com/previous" };
            _documentService
                .Setup(s => s.DocUpdateDefaultAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<SessionInfo>()))
                .ReturnsAsync(expectedModel);

            var controller = CreateController(new TestSession());

            // Act
            var result = await controller.doc_index_update_default(123, "http://example.com/previous");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("~/Views/Egrants/EgrantsDocUpdate.cshtml", viewResult.ViewName);
            var model = Assert.IsType<eGrantsDocUpdateViewModel>(viewResult.Model);
            Assert.Equal("http://example.com/previous", model.PreviousUrl);
        }

        [Fact]
        public async Task doc_index_update_default_Throws_WhenSessionIsNull()
        {
            // Arrange: doc_index_update_default reads sessionInfo, so a null session
            // fails when SessionInfoService attempts to read from it.
            var controller = CreateController(session: null);

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() =>
                controller.doc_index_update_default(999, "test.com"));

            // The document service should never be reached because session access fails first.
            _documentService.Verify(
                s => s.DocUpdateDefaultAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<SessionInfo>()),
                Times.Never);
        }

        #endregion

        #region doc_upload_default (does NOT use the session)

        [Fact]
        public async Task doc_upload_default_ReturnsViewWithModel()
        {
            // Arrange
            var expectedModel = new eGrantsDocUploadViewModel { DocId = 123 };
            _documentService
                .Setup(s => s.DocUploadDefaultAsync(123))
                .ReturnsAsync(expectedModel);

            var controller = CreateController(new TestSession());

            // Act
            var result = await controller.doc_upload_default(123);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("~/Views/Egrants/EgrantsDocUpload.cshtml", viewResult.ViewName);
            var model = Assert.IsType<eGrantsDocUploadViewModel>(viewResult.Model);
            Assert.Equal(123, model.DocId);
        }

        [Fact]
        public async Task doc_upload_default_CallsDocumentServiceWithCorrectDocId()
        {
            // Arrange
            _documentService
                .Setup(s => s.DocUploadDefaultAsync(It.IsAny<int>()))
                .ReturnsAsync(new eGrantsDocUploadViewModel());

            var controller = CreateController(new TestSession());

            // Act
            await controller.doc_upload_default(555);

            // Assert
            _documentService.Verify(s => s.DocUploadDefaultAsync(555), Times.Once);
        }

        #endregion

        #region doc_modify

        [Fact]
        public void doc_modify_WithNonRestoreAction_DelegatesSingleCallToService()
        {
            var session = new TestSession();
            session.SetString("ic", "NCI");
            session.SetString("userid", "tester");
            session.SetInt32("position_id", 2);

            var controller = CreateController(session);

            controller.doc_modify("to delete", "101,102");

            _documentService.Verify(s => s.DocModify(
                "to delete",
                0,
                0,
                string.Empty,
                string.Empty,
                "101,102",
                string.Empty,
                "NCI",
                "tester"), Times.Once);

            _documentService.Verify(s => s.UpdateDocumentFileType(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void doc_modify_ToRestore_WithNoResolvableFileType_StillCallsDocModifyPerDocument()
        {
            var session = new TestSession();
            session.SetString("ic", "NCI");
            session.SetString("userid", "tester");
            session.SetInt32("position_id", 2);

            var dbOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=egrants_unit_tests;Trusted_Connection=True;TrustServerCertificate=True;")
                .Options;
            using var dbContext = new AppDbContext(dbOptions);
            var egrantsCommon = new EgrantsCommon(dbContext);

            var controller = CreateController(session, egrantsCommon);

            controller.doc_modify("to restore", "missing_doc_a,missing_doc_b");

            _documentService.Verify(s => s.UpdateDocumentFileType(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            _documentService.Verify(s => s.DocModify(
                "to restore",
                0,
                0,
                string.Empty,
                string.Empty,
                "missing_doc_a",
                It.Is<string>(fileType => string.IsNullOrEmpty(fileType)),
                "NCI",
                "tester"), Times.Once);
            _documentService.Verify(s => s.DocModify(
                "to restore",
                0,
                0,
                string.Empty,
                string.Empty,
                "missing_doc_b",
                It.Is<string>(fileType => string.IsNullOrEmpty(fileType)),
                "NCI",
                "tester"), Times.Once);
        }

        [Fact]
        public void doc_modify_ReadOnlyCrossIcUser_ReturnsForbid_AndDoesNotCallService()
        {
            var session = new TestSession();
            session.SetString("ic", "CSR");
            session.SetString("authority_ic", "nci");
            session.SetString("userid", "foiauser");
            session.SetString("effective_role", "ProgramCrossIcReadOnly");
            session.SetInt32("position_id", 1);

            var controller = CreateController(session);

            var result = controller.doc_modify("to delete", "101");

            Assert.IsType<ForbidResult>(result);
            _documentService.Verify(s => s.DocModify(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void doc_modify_SpecialistCrossIc_UsesAuthorityIc()
        {
            var session = new TestSession();
            session.SetString("ic", "CSR");
            session.SetString("authority_ic", "nci");
            session.SetString("userid", "specialistx");
            session.SetString("effective_role", "SpecialistCrossIc");
            session.SetInt32("position_id", 2);

            var controller = CreateController(session);

            controller.doc_modify("to delete", "101");

            _documentService.Verify(s => s.DocModify(
                "to delete",
                0,
                0,
                string.Empty,
                string.Empty,
                "101",
                string.Empty,
                "nci",
                "specialistx"), Times.Once);
        }

        #endregion

        #region Cross-IC write guards

        [Fact]
        public async Task doc_create_by_file_ReadOnlyCrossIcUser_ReturnsForbid()
        {
            var session = new TestSession();
            session.SetString("ic", "CSR");
            session.SetString("authority_ic", "nci");
            session.SetString("userid", "foiauser");
            session.SetString("effective_role", "ProgramCrossIcReadOnly");
            session.SetInt32("position_id", 1);

            var controller = CreateController(session);

            var stream = new MemoryStream(new byte[] { 1, 2, 3 });
            IFormFile file = new FormFile(stream, 0, stream.Length, "file", "test.txt");

            var result = await controller.doc_create_by_file(file, 1, 1, "sub", DateTime.Today, "NCI", 123);

            Assert.IsType<ForbidResult>(result);
            _documentService.Verify(s => s.DocCreateByFileAsync(
                It.IsAny<IFormFile>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<SessionInfo>()), Times.Never);
        }

        [Fact]
        public async Task doc_upload_by_file_ReadOnlyCrossIcUser_ReturnsForbid()
        {
            var session = new TestSession();
            session.SetString("ic", "CSR");
            session.SetString("authority_ic", "nci");
            session.SetString("userid", "foiauser");
            session.SetString("effective_role", "ProgramCrossIcReadOnly");
            session.SetInt32("position_id", 1);

            var controller = CreateController(session);

            var stream = new MemoryStream(new byte[] { 1, 2, 3 });
            IFormFile file = new FormFile(stream, 0, stream.Length, "file", "test.txt");

            var result = await controller.doc_upload_by_file(file, 101);

            Assert.IsType<ForbidResult>(result);
            _documentService.Verify(s => s.DocUploadByFileAsync(
                It.IsAny<IFormFile>(),
                It.IsAny<int>(),
                It.IsAny<SessionInfo>()), Times.Never);
        }

        [Fact]
        public async Task doc_index_modify_ReadOnlyCrossIcUser_ReturnsForbid()
        {
            var session = new TestSession();
            session.SetString("ic", "CSR");
            session.SetString("authority_ic", "nci");
            session.SetString("userid", "foiauser");
            session.SetString("effective_role", "ProgramCrossIcReadOnly");
            session.SetInt32("position_id", 1);

            var controller = CreateController(session);

            var result = await controller.doc_index_modify(
                act: "to_update",
                appl_id: 1,
                document_id: 2,
                category_id: 3,
                sub_category: "sub",
                document_date: "01/01/2026",
                previous_url: "/Egrants/Index");

            Assert.IsType<ForbidResult>(result);
            _documentService.Verify(s => s.DocIndexModifyAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<SessionInfo>()), Times.Never);
        }

        [Fact]
        public void doc_modify_SpecialistCrossIc_FallsBackToIcWhenAuthorityMissing()
        {
            var session = new TestSession();
            session.SetString("ic", "NCI");
            session.SetString("userid", "specialistx");
            session.SetString("effective_role", "SpecialistCrossIc");
            session.SetInt32("position_id", 2);

            var controller = CreateController(session);

            controller.doc_modify("to delete", "101");

            _documentService.Verify(s => s.DocModify(
                "to delete",
                0,
                0,
                string.Empty,
                string.Empty,
                "101",
                string.Empty,
                "NCI",
                "specialistx"), Times.Once);
        }

        #endregion
    }
}
