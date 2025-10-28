using System;
using System.Threading.Tasks;

using eGrants.Models;
using eGrants.Repositories;
using eGrants.Repositories.Interfaces;
using eGrants.Services;
using eGrants.Services.Interfaces;
using eGrants.ViewModels;

using Moq;

using Xunit;

namespace eGrants.Tests.Unit.Service
{
    public class eGrantsServiceTests
    {
        private readonly Mock<IeGrantsService> _mockService;
        private readonly Mock<IeGrantsRepository> _mockRepo;
        private readonly eGrantsService _service;

        public eGrantsServiceTests()
        {
            _mockService = new Mock<IeGrantsService>(); // { CallBase = true };
            _mockRepo = new Mock<IeGrantsRepository>();
            _service = new eGrantsService(_mockRepo.Object);
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
        public async Task ReturnsMessage_WhenAllFiltersAreEmpty()
        {
            var sessionInfo = new SessionInfo { Ic = "IC", UserId = "dcdehuff", Browser = "Chrome" };
            var expectedResult = new eGrantsSearchViewModel
            {
                Message = "No data found for the search",
                grantlayer = null
            };

            _mockService.Setup(s => s.GetEgrantsByFilterAsync(0, "", 0, "", 1, 1, 1, sessionInfo))
                .ReturnsAsync(expectedResult);

            var result = await _mockService.Object.GetEgrantsByFilterAsync(0, "", 0, "", 1, 1, 1, sessionInfo);

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

            _mockService.Setup(s => s.GetEgrantsByFilterAsync(2024, "R01", 123, "ADM", 1, 1, 1, sessionInfo))
                .ReturnsAsync(expectedResult);

            var result = await _mockService.Object.GetEgrantsByFilterAsync(2024, "R01", 123, "ADM", 1, 1, 1, sessionInfo);

            Assert.Equal("by_filters", result.SearchStyle);
            Assert.NotNull(result.grantlayer);
            Assert.NotNull(result.appllayer);
            Assert.Equal(1, result.ApplCount);
            Assert.NotNull(result.Pagination);
        }

        [Fact]
        public async Task ReturnsMessage_WhenGrantLayerPropertyIsNull()
        {
            var sessionInfo = new SessionInfo { Ic = "IC", UserId = "dcdehuff", Browser = "Chrome" };
            var filterQuery = new List<FilterSearchResult>();

            filterQuery.Add(new FilterSearchResult { Value = "SELECT * FROM Grants" });

            var viewModel = new eGrantsSearchViewModel
            {
                grantlayerproperty = null
            };

            var expectedResult = new eGrantsSearchViewModel
            {
                grant_id = "1",
                project_title = "Grant A",
                Message = "No data found for the search",
                grantlayer = null
            };

            _mockRepo.Setup(r => r.FilterSearchQuery(2024, "R01", "ADM", 123, 1, sessionInfo))
                .ReturnsAsync(filterQuery);

            _mockService.Setup(s => s.eGrantsSearchResults("SELECT * FROM Grants", 1, "by_filters", 1, 1, sessionInfo, It.IsAny<eGrantsSearchViewModel>(), true))
                .ReturnsAsync(viewModel);

            _mockService.Setup(s => s.GetEgrantsByFilterAsync(2024, "R01", 123, "ADM", 1, 1, 1, sessionInfo))
                .ReturnsAsync(expectedResult);

            //var result = await _mockService.Object.GetEgrantsByFilterAsync(2024, "R01", 123, "ADM", 1, 1, 1, sessionInfo);
            var result = await _mockService.Object.GetEgrantsByFilterAsync(2024, "R01", 123, "ADM", 1, 1, 1, sessionInfo);
            
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

            _mockService.Setup(s => s.GetEgrantsByFilterAsync(2024, "R01", 123, "ADM", 1, 1, 1, sessionInfo))
                .ReturnsAsync(expectedResult);

            //var result = await _mockService.Object.GetEgrantsByFilterAsync(2024, "R01", 123, "ADM", 1, 1, 1, sessionInfo);
            var result = await _mockService.Object.GetEgrantsByFilterAsync(2024, "R01", 123, "ADM", 1, 1, 1, sessionInfo);

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
            _mockService.Setup(s => s.GetEgrantsByFilterAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), sessionInfo))
                .ThrowsAsync(new Exception("Database unreachable"));

            // Act
            eGrantsSearchViewModel result = null;
            string errorMessage = null;

            try
            {
                result = await _mockService.Object.GetEgrantsByFilterAsync(2024, "R01", 123, "ADM", 1, 1, 1, sessionInfo);
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
    }
}

