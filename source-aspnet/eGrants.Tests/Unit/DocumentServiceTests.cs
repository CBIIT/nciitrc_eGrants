using System;
using System.Threading.Tasks;

using eGrants.DTOs;
using eGrants.Models;
using eGrants.Repositories;
using eGrants.Repositories.Interfaces;
using eGrants.Services;
using eGrants.Services.Interfaces;
using eGrants.ViewModels;

using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

namespace eGrants.Tests.Unit
{
    public class eGrantsServiceTests
    {
        private readonly Mock<IeGrantsService> _mockService;
        private readonly Mock<IDocumentService> _mockDocumentService;
        private readonly Mock<IeGrantsRepository> _mockRepo;
        private readonly Mock<IDocumentRepository> _mockDocumentRepo;
        private readonly Mock<ICommonRepository> _mockCommonRepo;
        private readonly Mock<ILogger<IeGrantsService>> _mockLogger;
        private readonly eGrantsService _service;

        public eGrantsServiceTests()
        {
            _mockService = new Mock<IeGrantsService>(); // { CallBase = true };
            _mockRepo = new Mock<IeGrantsRepository>();
            _mockLogger = new Mock<ILogger<IeGrantsService>>();
            _service = new eGrantsService(_mockRepo.Object, _mockLogger.Object);
            _mockDocumentRepo = new Mock<IDocumentRepository>();
            _mockDocumentService = new Mock<IDocumentService>();
            _mockCommonRepo = new Mock<ICommonRepository>();
        }

        #region DocUploadDefaultAsync tests
        [Fact]
        public async Task DocUploadDefaultAsync_WithValidDocId_ReturnsViewModelWithDocumentData()
        {
            // Arrange
            var date = DateTime.Now;
            var documentInfo = new List<DocumentInformation>
            {
                new DocumentInformation
                {
                    document_id = 123,
                    appl_id = 123456,
                    document_name = "Test Document",
                    document_date = date,
                    full_grant_num = "1R01CA123456-01"
                }
            };

            _mockDocumentRepo
                .Setup(repo => repo.GetDocInfo(123))
                .ReturnsAsync(documentInfo);

            var documentService = new DocumentService(
                _mockDocumentRepo.Object, null, null, null);

            // Act
            var result = await documentService.DocUploadDefaultAsync(123);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(123, result.DocId);
            Assert.Equal(123456, result.ApplId);
            Assert.Equal("Test Document", result.DocName);
            Assert.Equal(date, result.DocDate);
            Assert.Equal("1R01CA123456-01", result.FullGrantNum);
        }

        [Fact]
        public async Task DocUploadDefaultAsync_WithEmptyDocumentList_ReturnsEmptyViewModel()
        {
            // Arrange
            var emptyDocumentInfo = new List<DocumentInformation>();

            _mockDocumentRepo
                .Setup(repo => repo.GetDocInfo(456))
                .ReturnsAsync(emptyDocumentInfo);

            var documentService = new DocumentService(
                _mockDocumentRepo.Object, null, null, null);

            // Act
            var result = await documentService.DocUploadDefaultAsync(456);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.DocId);
            Assert.Null(result.ApplId);
            Assert.Null(result.DocName);
            Assert.Null(result.FullGrantNum);
        }

        [Fact]
        public async Task DocUploadDefaultAsync_CallsRepositoryWithCorrectDocId()
        {
            // Arrange
            var date = DateTime.Now;
            var documentInfo = new List<DocumentInformation>
            {
                new DocumentInformation
                {
                    document_id = 789,
                    appl_id = 987654,
                    document_name = "Repository Test Document",
                    document_date = date,
                    full_grant_num = "3R01CA987654-03"
                }
            };

            _mockDocumentRepo
                .Setup(repo => repo.GetDocInfo(789))
                .ReturnsAsync(documentInfo);

            var documentService = new DocumentService(
                _mockDocumentRepo.Object, null, null, null);

            // Act
            var result = await documentService.DocUploadDefaultAsync(789);

            // Assert
            _mockDocumentRepo.Verify(repo => repo.GetDocInfo(789), Times.Once);
            Assert.Equal(789, result.DocId);
            Assert.Equal(987654, result.ApplId);
            Assert.Equal("Repository Test Document", result.DocName);
        }
        #endregion

        #region DocUpdateDefaultAsync tests
        [Fact]
        public async Task DocUpdateDefaultAsync_WithValidDocId_ReturnsViewModelWithDocumentData()
        {
            // Arrange
            var date = DateTime.Now;
            var sessionInfo = new SessionInfo { UserId = "testuser", Ic = "1" };
            var documentInfo = new List<DocumentInformation>
            {
                new DocumentInformation
                {
                    document_id = 123,
                    appl_id = 123456,
                    admin_phs_org_code = "NCI",
                    serial_num = 789,
                    category_id = 5,
                    sub_category_name = "Research Grant",
                    document_date = date
                }
            };

            var adminCodeList = new List<AdminCodes> { new AdminCodes { admin_phs_org_code = "NCI" } };
            var categoryList = new List<CategoriesListDTO> { new CategoriesListDTO { category_id = 5 } };
            var subCategoryList = new List<SubCategories> { new SubCategories { sub_category_name = "Research Grant" } };
            var grantYearList = new List<VwApplDTO> { new VwApplDTO { appl_id = 123456 } };

            _mockDocumentRepo.Setup(repo => repo.GetDocInfo(123)).ReturnsAsync(documentInfo);
            _mockCommonRepo.Setup(repo => repo.LoadAdminCodes()).ReturnsAsync(adminCodeList);
            _mockDocumentRepo.Setup(repo => repo.LoadCategories("1")).ReturnsAsync(categoryList);
            _mockDocumentRepo.Setup(repo => repo.GetMaxCategoryId("1")).ReturnsAsync(10);
            _mockDocumentRepo.Setup(repo => repo.LoadSubCategoryList()).ReturnsAsync(subCategoryList);
            _mockService.Setup(service => service.LoadApplsByApplid(123456)).ReturnsAsync(grantYearList);

            var mockSessionInfoService = new Mock<ISessionInfoService>();
            var mockCommonRepo = new Mock<ICommonRepository>();
            var mockEGrantsService = new Mock<IeGrantsService>();

            var documentService = new DocumentService(
                _mockDocumentRepo.Object,
                mockSessionInfoService.Object,
                _mockCommonRepo.Object,
                _mockService.Object);

            // Act
            var result = await documentService.DocUpdateDefaultAsync(123, "http://previous.url", sessionInfo);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Update", result.Act);
            Assert.Equal("NCI", result.AdminCode);
            Assert.Equal(789, result.SerialNum);
            Assert.Equal(123456, result.ApplId);
            Assert.Equal(123, result.DocId);
            Assert.Equal("Research Grant", result.SubCategory);
            Assert.Equal(date, result.DocDate);
            Assert.Equal("http://previous.url", result.PreviousUrl);
            Assert.Equal("default", result.Status);
        }

        [Fact]
        public async Task DocUpdateDefaultAsync_LoadsAllRequiredLists()
        {
            // Arrange
            var date = DateTime.Now;
            var sessionInfo = new SessionInfo { UserId = "user123", Ic = "2" };
            var documentInfo = new List<DocumentInformation>
            {
                new DocumentInformation
                {
                    document_id = 456,
                    appl_id = 654321,
                    admin_phs_org_code = "NHLBI",
                    serial_num = 111,
                    category_id = 3,
                    sub_category_name = "Training Grant",
                    document_date = date
                }
            };

            var adminCodeList = new List<AdminCodes> { new AdminCodes { admin_phs_org_code = "NHLBI" } };
            var categoryList = new List<CategoriesListDTO> { new CategoriesListDTO { category_id = 3 } };
            var subCategoryList = new List<SubCategories> { new SubCategories { sub_category_name = "Training Grant" } };
            var grantYearList = new List<VwApplDTO> { new VwApplDTO { appl_id = 654321 } };

            _mockDocumentRepo.Setup(repo => repo.GetDocInfo(456)).ReturnsAsync(documentInfo);
            _mockCommonRepo.Setup(repo => repo.LoadAdminCodes()).ReturnsAsync(adminCodeList);
            _mockDocumentRepo.Setup(repo => repo.LoadCategories("2")).ReturnsAsync(categoryList);
            _mockDocumentRepo.Setup(repo => repo.GetMaxCategoryId("2")).ReturnsAsync(15);
            _mockDocumentRepo.Setup(repo => repo.LoadSubCategoryList()).ReturnsAsync(subCategoryList);
            _mockService.Setup(service => service.LoadApplsByApplid(654321)).ReturnsAsync(grantYearList);

            var mockSessionInfoService = new Mock<ISessionInfoService>();

            var documentService = new DocumentService(
                _mockDocumentRepo.Object,
                mockSessionInfoService.Object,
                _mockCommonRepo.Object,
                _mockService.Object);

            // Act
            var result = await documentService.DocUpdateDefaultAsync(456, "http://test.url", sessionInfo);

            // Assert
            Assert.NotNull(result.AdminCodeList);
            Assert.NotNull(result.CategoryList);
            Assert.NotNull(result.SubCategoryList);
            Assert.NotNull(result.GrantYearList);
            Assert.Equal(15, result.MaxCategoryId);
            Assert.Single(result.AdminCodeList);
            Assert.Single(result.CategoryList);
            Assert.Single(result.SubCategoryList);
            Assert.Single(result.GrantYearList);
        }

        [Fact]
        public async Task DocUpdateDefaultAsync_CallsRepositoryMethodsWithCorrectParameters()
        {
            // Arrange
            var date = DateTime.Now;
            var sessionInfo = new SessionInfo { UserId = "user987", Ic = "3" };
            var documentInfo = new List<DocumentInformation>
                {
                    new DocumentInformation
                    {
                        document_id = 789,
                        appl_id = 987654,
                        admin_phs_org_code = "NIAID",
                        serial_num = 222,
                        category_id = 7,
                        sub_category_name = "Fellowship",
                        document_date = date
                    }
                };

            _mockDocumentRepo.Setup(repo => repo.GetDocInfo(789)).ReturnsAsync(documentInfo);
            _mockCommonRepo.Setup(repo => repo.LoadAdminCodes()).ReturnsAsync(new List<AdminCodes>());
            _mockDocumentRepo.Setup(repo => repo.LoadCategories("3")).ReturnsAsync(new List<CategoriesListDTO>());
            _mockDocumentRepo.Setup(repo => repo.GetMaxCategoryId("3")).ReturnsAsync(20);
            _mockDocumentRepo.Setup(repo => repo.LoadSubCategoryList()).ReturnsAsync(new List<SubCategories>());
            _mockService.Setup(service => service.LoadApplsByApplid(987654)).ReturnsAsync(new List<VwApplDTO>());

            var mockSessionInfoService = new Mock<ISessionInfoService>();

            var documentService = new DocumentService(
                _mockDocumentRepo.Object,
                mockSessionInfoService.Object,
                _mockCommonRepo.Object,
                _mockService.Object);

            // Act
            await documentService.DocUpdateDefaultAsync(789, "http://another.url", sessionInfo);

            // Assert
            _mockDocumentRepo.Verify(repo => repo.GetDocInfo(789), Times.Once);
            _mockCommonRepo.Verify(repo => repo.LoadAdminCodes(), Times.Once);
            _mockDocumentRepo.Verify(repo => repo.LoadCategories("3"), Times.Once);
            _mockDocumentRepo.Verify(repo => repo.GetMaxCategoryId("3"), Times.Once);
            _mockDocumentRepo.Verify(repo => repo.LoadSubCategoryList(), Times.Once);
            _mockService.Verify(service => service.LoadApplsByApplid(987654), Times.Once);
        }
        #endregion

        #region DocCreateWithoutApplId tests
        [Fact]
        public async Task DocCreateWithoutApplIdAsync_ReturnsViewModelWithCorrectData()
        {
            // Arrange
            var sessionInfo = new SessionInfo { UserId = "user456", Ic = "1" };
            var previousUrl = "http://previous.url";

            var adminCodeList = new List<AdminCodes> { new AdminCodes { admin_phs_org_code = "NCI" } };
            var categoryList = new List<CategoriesListDTO> { new CategoriesListDTO { category_id = 5 } };
            var subCategoryList = new List<SubCategories> { new SubCategories { sub_category_name = "Research Grant" } };

            _mockCommonRepo.Setup(repo => repo.LoadAdminCodes()).ReturnsAsync(adminCodeList);
            _mockDocumentRepo.Setup(repo => repo.LoadCategories("1")).ReturnsAsync(categoryList);
            _mockDocumentRepo.Setup(repo => repo.GetMaxCategoryId("1")).ReturnsAsync(10);
            _mockDocumentRepo.Setup(repo => repo.LoadSubCategoryList()).ReturnsAsync(subCategoryList);

            var mockSessionInfoService = new Mock<ISessionInfoService>();

            var documentService = new DocumentService(
                _mockDocumentRepo.Object,
                mockSessionInfoService.Object,
                _mockCommonRepo.Object,
                _mockService.Object);

            // Act
            var result = await documentService.DocCreateWithoutApplIdAsync(previousUrl, sessionInfo);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Add", result.Act);
            Assert.Equal(previousUrl, result.PreviousUrl);
            Assert.Equal(10, result.MaxCategoryId);
            Assert.NotNull(result.AdminCodeList);
            Assert.NotNull(result.CategoryList);
            Assert.NotNull(result.SubCategoryList);
            Assert.Single(result.AdminCodeList);
            Assert.Single(result.CategoryList);
            Assert.Single(result.SubCategoryList);
        }

        [Fact]
        public async Task DocCreateWithoutApplIdAsync_CallsRepositoryMethodsWithCorrectParameters()
        {
            // Arrange
            var sessionInfo = new SessionInfo { UserId = "testuser", Ic = "2" };
            var previousUrl = "http://test.url";

            _mockCommonRepo.Setup(repo => repo.LoadAdminCodes()).ReturnsAsync(new List<AdminCodes>());
            _mockDocumentRepo.Setup(repo => repo.LoadCategories("2")).ReturnsAsync(new List<CategoriesListDTO>());
            _mockDocumentRepo.Setup(repo => repo.GetMaxCategoryId("2")).ReturnsAsync(15);
            _mockDocumentRepo.Setup(repo => repo.LoadSubCategoryList()).ReturnsAsync(new List<SubCategories>());

            var mockSessionInfoService = new Mock<ISessionInfoService>();

            var documentService = new DocumentService(
                _mockDocumentRepo.Object,
                mockSessionInfoService.Object,
                _mockCommonRepo.Object,
                _mockService.Object);

            // Act
            await documentService.DocCreateWithoutApplIdAsync(previousUrl, sessionInfo);

            // Assert
            _mockCommonRepo.Verify(repo => repo.LoadAdminCodes(), Times.Once);
            _mockDocumentRepo.Verify(repo => repo.LoadCategories("2"), Times.Once);
            _mockDocumentRepo.Verify(repo => repo.GetMaxCategoryId("2"), Times.Once);
            _mockDocumentRepo.Verify(repo => repo.LoadSubCategoryList(), Times.Once);
        }

        [Fact]
        public async Task DocCreateWithoutApplIdAsync_WithNullPreviousUrl_ReturnsViewModelWithNullPreviousUrl()
        {
            // Arrange
            var sessionInfo = new SessionInfo { UserId = "aalyaanferoz", Ic = "3" };
            string previousUrl = null;

            var adminCodeList = new List<AdminCodes> { new AdminCodes { admin_phs_org_code = "NHLBI" } };
            var categoryList = new List<CategoriesListDTO> { new CategoriesListDTO { category_id = 3 } };
            var subCategoryList = new List<SubCategories> { new SubCategories { sub_category_name = "Training Grant" } };

            _mockCommonRepo.Setup(repo => repo.LoadAdminCodes()).ReturnsAsync(adminCodeList);
            _mockDocumentRepo.Setup(repo => repo.LoadCategories("3")).ReturnsAsync(categoryList);
            _mockDocumentRepo.Setup(repo => repo.GetMaxCategoryId("3")).ReturnsAsync(20);
            _mockDocumentRepo.Setup(repo => repo.LoadSubCategoryList()).ReturnsAsync(subCategoryList);

            var mockSessionInfoService = new Mock<ISessionInfoService>();

            var documentService = new DocumentService(
                _mockDocumentRepo.Object,
                mockSessionInfoService.Object,
                _mockCommonRepo.Object,
                _mockService.Object);

            // Act
            var result = await documentService.DocCreateWithoutApplIdAsync(previousUrl, sessionInfo);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Add", result.Act);
            Assert.Null(result.PreviousUrl);
            Assert.Equal(20, result.MaxCategoryId);
        }
        #endregion
    }
}

