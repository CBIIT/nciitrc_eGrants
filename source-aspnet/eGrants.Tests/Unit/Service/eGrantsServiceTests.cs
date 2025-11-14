using System;
using System.Threading.Tasks;

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

namespace eGrants.Tests.Unit.Service
{
    public class eGrantsServiceTests
    {
        private readonly Mock<IeGrantsService> _mockService;
        private readonly Mock<IeGrantsRepository> _mockRepo;
        private readonly Mock<ILogger<IeGrantsService>> _mockLogger;
        private readonly eGrantsService _service;

        public eGrantsServiceTests()
        {
            _mockService = new Mock<IeGrantsService>(); // { CallBase = true };
            _mockRepo = new Mock<IeGrantsRepository>();
            _mockLogger = new Mock<ILogger<IeGrantsService>>();
            _service = new eGrantsService(_mockRepo.Object, _mockLogger.Object);
        }

        #region GetEgrantsByStrAsync Tests

        [Fact]
        public async Task ReturnsMessage_WhenSearchStringIsEmpty()
        {
            var sessionInfo = new SessionInfo { Ic = "IC", UserId = "dcdehuff", Browser = "Chrome" };
            var expectedResult = new eGrantsSearchViewModel
            {
                Message = "No data found for the search",
                grantlayer = null
            };

            _mockService.Setup(s => s.GetEgrantsByStrAsync("", 1, 1, 1, sessionInfo))
                .ReturnsAsync(expectedResult);

            var result = await _mockService.Object.GetEgrantsByStrAsync("", 1, 1, 1, sessionInfo);

            Assert.Equal("No data found for the search", result.Message);
            Assert.Null(result.grantlayer);
        }

        [Fact]
        public async Task ReturnsPagination_WhenGrantLayerPropertyExists()
        {
            var sessionInfo = new SessionInfo { Ic = "IC", UserId = "User" };
            var viewModel = new eGrantsSearchViewModel
            {
                grantlayerproperty = new List<GrantLayer>(),
                Str = "test",
                Pagination = new List<Pagination>()
            };

            _mockService.Setup(s => s.eGrantsSearchResults(
                "test", 1, "", 1, 1, sessionInfo, It.IsAny<eGrantsSearchViewModel>(), true))
                .ReturnsAsync(viewModel);

            _mockService.Setup(s => s.LoadPagination("test", "IC", "User", ""))
                .ReturnsAsync(new List<Pagination>());

            _mockService.Setup(s => s.GetEgrantsByStrAsync("test", 1, 1, 1, sessionInfo))
                .ReturnsAsync(viewModel);

            var result = await _mockService.Object.GetEgrantsByStrAsync("test", 1, 1, 1, sessionInfo);

            Assert.NotNull(result.Pagination);
            Assert.Equal("test", result.Str);
        }


        #endregion

        #region GetEgrantsByFilterAsync Tests

        [Fact]
        public async Task ReturnsMessage_WhenAllFiltersAreEmptyOrInvalidTabOrPackage()
        {
            var sessionInfo = new SessionInfo { Ic = "IC", UserId = "dcdehuff", Browser = "Chrome" };

            var result = await _service.GetEgrantsByFilterAsync(0, "", 0, "", 1, 1, 1, sessionInfo, 0, null);

            Assert.Equal("No data found for the search", result.Message);
            Assert.Null(result.grantlayer);
        }

        [Fact]
        public async Task ReturnsSearchResults_WhenFiltersAreValid_AndGrantLayerExists()
        {
            var sessionInfo = new SessionInfo { Ic = "IC", Browser = "Chrome" };
            var filterQuery = new List<FilterSearchResult>();

            filterQuery.Add(new FilterSearchResult { Value = "SELECT * FROM Grants" });

            var expectedResult = new eGrantsSearchViewModel
            {
                SearchStyle = "by_filters",
                grantlayer = new List<GrantLayer>(),
                appllayer = new List<ApplLayerObject>(),
                ApplCount = 1,
                Pagination = new List<Pagination>()
            };

            _mockRepo.Setup(r => r.FilterSearchQuery(2024, "R01", "ADM", 123, 1, sessionInfo))
                .ReturnsAsync(filterQuery);

            _mockService.Setup(s => s.eGrantsSearchResults("SELECT * FROM Grants", 1, "by_filters", 1, 1, sessionInfo, It.IsAny<eGrantsSearchViewModel>(), true))
                .ReturnsAsync(expectedResult);

            _mockRepo.Setup(r => r.LoadPaginationAsync("SELECT * FROM Grants", "IC", "Chrome", "by_filters"))
                .ReturnsAsync(new List<Pagination>());

            _mockService.Setup(s => s.GetEgrantsByFilterAsync(2024, "R01", 123, "ADM", 1, 1, 1, sessionInfo, 2, "by_filters"))
                .ReturnsAsync(expectedResult);

            var result = await _mockService.Object.GetEgrantsByFilterAsync(2024, "R01", 123, "ADM", 1, 1, 1, sessionInfo, 2, "by_filters");

            Assert.Equal("by_filters", result.SearchStyle);
            Assert.NotNull(result.grantlayer);
            Assert.NotNull(result.appllayer);
            Assert.Equal(1, result.ApplCount);
            Assert.NotNull(result.Pagination);
        }

        [Fact]
        public async Task ReturnsMessage_WhenGrantLayerPropertyIsNull()
        {
            // Arrange
            var sessionInfo = new SessionInfo { Ic = "IC", UserId = "dcdehuff", Browser = "Chrome" };
            var filterQuery = new List<FilterSearchResult> { new FilterSearchResult { Value = "SELECT * FROM Grants" } };

            var intermediateViewModel = new eGrantsSearchViewModel
            {
                grantlayerproperty = null,
                appllayerproperty = null
            };

            _mockRepo.Setup(r => r.FilterSearchQuery(2024, "R01", "ADM", 123, 1, sessionInfo))
                .ReturnsAsync(filterQuery);

            _mockService.Setup(s => s.eGrantsSearchResults(
                    "SELECT * FROM Grants",
                    1,
                    "by_filters",
                    1,
                    1,
                    sessionInfo,
                    It.IsAny<eGrantsSearchViewModel>(),
                    true))
                .ReturnsAsync(intermediateViewModel);

            // Act
            var result = await _service.GetEgrantsByFilterAsync(
                fiscalYear: 2024,
                mechanism: "R01",
                serialNum: 123,
                adminCode: "ADM",
                grantId: 1,
                applId: 1,
                currentPage: 1,
                sessionInfo: sessionInfo,
                tabNum: 1,
                package: "by_filters");

            // Assert
            Assert.Equal("No data found for the search", result.Message);
            Assert.Null(result.grantlayer);
        }

        [Fact]
        public async Task SetsFilterFieldsCorrectly_WhenFiltersAreProvided()
        {
            var sessionInfo = new SessionInfo { Ic = "IC", UserId = "dcdehuff", Browser = "Chrome" };
            var filterQuery = new List<FilterSearchResult>();
            var viewModel = new eGrantsSearchViewModel
            {
                grantlayerproperty = new List<GrantLayer>(),
                appllayerproperty = new List<ApplLayerObject> { new ApplLayerObject() }
            };

            var expectedResult = new eGrantsSearchViewModel
            {
                FilterFY = 2024,
                FilterSerialNumber = 123,
                FilterMechanism = "R01",
                FilterAdminCode = "ADM"
            };

            filterQuery.Add(new FilterSearchResult { Value = "SELECT * FROM Grants" });

            _mockRepo.Setup(r => r.FilterSearchQuery(2024, "R01", "ADM", 123, 1, sessionInfo))
                .ReturnsAsync(filterQuery);

            _mockService.Setup(s => s.eGrantsSearchResults(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), sessionInfo, It.IsAny<eGrantsSearchViewModel>(), true))
                .ReturnsAsync(viewModel);

            _mockService.Setup(s => s.GetEgrantsByFilterAsync(2024, "R01", 123, "ADM", 1, 1, 1, sessionInfo, 2, "by_filters"))
                .ReturnsAsync(expectedResult);

            //var result = await _mockService.Object.GetEgrantsByFilterAsync(2024, "R01", 123, "ADM", 1, 1, 1, sessionInfo);
            var result = await _mockService.Object.GetEgrantsByFilterAsync(2024, "R01", 123, "ADM", 1, 1, 1, sessionInfo, 2, "by_filters");

            Assert.Equal(2024, result.FilterFY);
            Assert.Equal(123, result.FilterSerialNumber);
            Assert.Equal("R01", result.FilterMechanism);
            Assert.Equal("ADM", result.FilterAdminCode);
        }

        [Fact]
        public async Task ReturnsErrorMessage_WhenServiceThrowsException()
        {
            var sessionInfo = new SessionInfo { Ic = "IC", UserId = "dcdehuff", Browser = "Chrome" };

            // Simulate the service throwing an exception
            _mockRepo.Setup(r => r.FilterSearchQuery(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), sessionInfo))
                .ThrowsAsync(new Exception("Database unreachable"));

            // Act
            eGrantsSearchViewModel result = null;
            string errorMessage = null;

            try
            {
                result = await _service.GetEgrantsByFilterAsync(2024, "R01", 123, "ADM", 1, 1, 1, sessionInfo, 1, "by_filters");
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            // Assert
            Assert.Null(result);
            Assert.Equal("Database unreachable", errorMessage);
        }

        #endregion

        #region GetSupplements Tests

        [Fact]
        public async Task GetSupplements_ValidInputs_ReturnsSupplements()
        {
            // Arrange
            var expected = new List<supplement> { new supplement(), new supplement() };
            _mockService.Setup(r => r.GetSupplements("ACT", 1, 2025, "A", "DOC123", 100, "IC1", "user1"))
                        .ReturnsAsync(expected);

            // Act
            var result = await _mockService.Object.GetSupplements("ACT", 1, 2025, "A", "DOC123", 100, "IC1", "user1");

            // Assert
            Assert.Equal(expected.Count, result.Count);
        }

        [Fact]
        public async Task GetSupplements_NoResults_ReturnsEmptyList()
        {
            _mockService.Setup(r => r.GetSupplements(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
                                                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(),
                                                    It.IsAny<string>(), It.IsAny<string>()))
                        .ReturnsAsync(new List<supplement>());

            var result = await _mockService.Object.GetSupplements("ACT", 1, 2025, "A", "DOC123", 100, "IC1", "user1");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetSupplements_NullStrings_CallsRepository()
        {
            _mockService.Setup(r => r.GetSupplements(null, 0, 0, null, null, 0, null, null))
                        .ReturnsAsync(new List<supplement>());

            var result = await _mockService.Object.GetSupplements(null, 0, 0, null, null, 0, null, null);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetSupplements_RepositoryThrows_ThrowsException()
        {
            _mockService.Setup(r => r.GetSupplements(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
                                                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(),
                                                    It.IsAny<string>(), It.IsAny<string>()))
                        .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() =>
                _mockService.Object.GetSupplements("ACT", 1, 2025, "A", "DOC123", 100, "IC1", "user1"));
        }

        [Fact]
        public async Task GetSupplements_VerifyRepositoryCalledOnce()
        {
            _mockService.Setup(r => r.GetSupplements(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
                                                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(),
                                                    It.IsAny<string>(), It.IsAny<string>()))
                        .ReturnsAsync(new List<supplement>());

            await _mockService.Object.GetSupplements("ACT", 1, 2025, "A", "DOC123", 100, "IC1", "user1");

            _mockService.Verify(r => r.GetSupplements("ACT", 1, 2025, "A", "DOC123", 100, "IC1", "user1"), Times.Once);
        }

        #endregion

        #region GetCategoryList Tests

        [Fact]
        public async Task GetCategoryList_ReturnsExpectedList_WhenRepositorySucceeds()
        {
            // Arrange
            int grantId = 42;
            string years = "All";
            var expected = new List<string> { "Education", "Health" };

            _mockRepo.Setup(r => r.GetCategoryList(grantId, years))
                     .ReturnsAsync(expected);

            // Act
            var result = await _service.GetCategoryList(grantId, years);

            // Assert
            Assert.Equal(expected, result);
            _mockRepo.Verify(r => r.GetCategoryList(grantId, years), Times.Once);
        }

        [Fact]
        public async Task GetCategoryList_LogsAndThrows_WhenRepositoryThrows()
        {
            // Arrange
            int grantId = 99;
            string years = "All";
            var exception = new Exception("Database error");

            _mockRepo.Setup(r => r.GetCategoryList(grantId, years))
                     .ThrowsAsync(exception);

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.GetCategoryList(grantId, years));

            // Assert
            Assert.Equal("Database error", ex.Message);

            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains("Error retrieving category list for GrantId")),
                    It.Is<Exception>(ex => ex.Message == "Database error"),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #endregion

        #region GetEgrantsByApplAsync Tests
        [Fact]
        public async Task ReturnsMessage_WhenApplIdIsZeroOrDoesNotExist()
        {
            int applId = 0;
            string mode = "testMode";
            string str = "testString";
            var sessionInfo = new SessionInfo { Ic = "IC", UserId = "testUser", Browser = "Chrome" };
            var expectedResult = new eGrantsSearchViewModel
            {
                Message = "No data found for the search",
                grantlayer = null
            };

            _mockService.Setup(s => s.GetEgrantsByApplAsync(0, mode, str, sessionInfo))
                        .ReturnsAsync(expectedResult);

            var result = await _mockService.Object.GetEgrantsByApplAsync(applId, mode, str, sessionInfo);

            Assert.Equal("No data found for the search", result.Message);
            Assert.Null(result.grantlayer);
        }

        [Fact]
        public async Task GetEgrantsByApplAsync_ReturnsViewModel_WithCorrectData_WhenApplIdExists()
        {
            // Arrange
            int applId = 123;
            string mode = "testMode";
            string str = "testString";
            var sessionInfo = new SessionInfo { Ic = "IC", UserId = "testUser", Browser = "Chrome" };


            _mockService.Setup(s => s.CheckApplID(applId))
                .ReturnsAsync(1);

            _mockService.Setup(s => s.GetGrantID(applId))
                .ReturnsAsync(456);

            _mockService.Setup(s => s.GetEgrantsByApplAsync(applId, mode, str, sessionInfo))
                .ReturnsAsync((int aId, string m, string st, SessionInfo si) =>
                {
                    var ViewModel = new eGrantsSearchViewModel
                    {
                        ApplID = 123,
                        GrantID = 456,
                        SearchStyle = "by_appl",
                        appllayer = new List<ApplLayerObject>
                        {
                            new ApplLayerObject { appl_id = "123", label = "TestLabel" }
                        }
                    };
                    return ViewModel;
                });


            // Act
            var result = await _mockService.Object.GetEgrantsByApplAsync(applId, mode, str, sessionInfo);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(applId, result.ApplID);
            Assert.Equal(456, result.GrantID);
            Assert.Equal("by_appl", result.SearchStyle);
            Assert.NotNull(result.appllayer);
            Assert.Single(result.appllayer);

        }

        #endregion

        #region CheckApplID Tests
        [Fact]
        public async Task CheckApplID_ReturnsExpectedValue()
        {
            int grantId = 123;
            int expectedResult = 1; 

            _mockRepo
                .Setup(repo => repo.CheckApplID(grantId))
                .ReturnsAsync(expectedResult);

            var result = await _service.CheckApplID(grantId);

            Assert.Equal(expectedResult, result);
            _mockRepo.Verify(repo => repo.CheckApplID(grantId), Times.Once);
        }

        [Fact]
        public async Task CheckApplID_ReturnsZero_WhenApplIDDoesNotExist()
        {
            int grantId = 456;
            int expectedResult = 0;

            _mockRepo
                .Setup(repo => repo.CheckApplID(grantId))
                .ReturnsAsync(expectedResult);

            var result = await _service.CheckApplID(grantId);

            Assert.Equal(expectedResult, result);
            _mockRepo.Verify(repo => repo.CheckApplID(grantId), Times.Once);
        }
        #endregion

        #region GetGrantID Tests
        [Fact]
        public async Task GetGrantID_ReturnsExpectedValue()
        {
            int applId = 1234567;
            int? expectedResult = 123;

            _mockRepo
                .Setup(repo => repo.GetGrantID(applId))
                .ReturnsAsync(expectedResult);

            var result = await _service.GetGrantID(applId);

            Assert.Equal(expectedResult, result);
            _mockRepo.Verify(repo => repo.GetGrantID(applId), Times.Once);
        }

        [Fact]
        public async Task GetGrantID_ReturnsZero_WhenGrantIDDoesNotExist()
        {
            int applId = 7654321;
            int? expectedResult = 321;

            _mockRepo
                .Setup(repo => repo.GetGrantID(applId))
                .ReturnsAsync(expectedResult);

            var result = await _service.GetGrantID(applId);

            Assert.Equal(expectedResult, result);
            _mockRepo.Verify(repo => repo.GetGrantID(applId), Times.Once);
        }
        #endregion

        #region LoadDataAutocomplete Tests

        [Fact]
        public async Task LoadDataAutocomplete_ReturnsExpectedList_WhenRepositorySucceeds()
        {
            // Arrange
            string type = "mechanism";
            string term = "abc";
            string sqlQuery = "sp_web_egrants_load_data_autocomplete_mechanism";
            var expected = new List<string> { "M1", "M2" };

            _mockRepo.Setup(r => r.LoadDataAutocomplete(sqlQuery, term, null, null, null, null))
                     .ReturnsAsync(expected);

            // Act
            var result = await _service.LoadDataAutocomplete(type, term);

            // Assert
            Assert.Equal(expected, result);
            _mockRepo.Verify(r => r.LoadDataAutocomplete(sqlQuery, term, null, null, null, null), Times.Once);
        }

        [Fact]
        public async Task LoadDataAutocomplete_LogsAndThrows_WhenRepositoryThrows()
        {
            // Arrange
            string type = "fy";
            string term = "2025";
            string sqlQuery = "sp_web_egrants_load_data_autocomplete_fy";
            var exception = new Exception("Stored procedure failed");

            _mockRepo.Setup(r => r.LoadDataAutocomplete(sqlQuery, term, null, null, null, null))
                     .ThrowsAsync(exception);

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.LoadDataAutocomplete(type, term));

            // Assert
            Assert.Equal("Stored procedure failed", ex.Message);

            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains("Error executing LoadDataAutocomplete")),
                    It.Is<Exception>(ex => ex.Message == "Stored procedure failed"),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #endregion
    }
}

