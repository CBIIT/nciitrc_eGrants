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

public class EGrantsServiceTests
{
    private readonly Mock<IeGrantsService> _mockService;
    private readonly Mock<IeGrantsRepository> _mockRepo;

    public EGrantsServiceTests()
    {
        _mockService = new Mock<IeGrantsService>(); // { CallBase = true };
        _mockRepo = new Mock<IeGrantsRepository>();
    }

    [Fact]
    public async Task ReturnsMessage_WhenSearchStringIsEmpty()
    {
        var result = await _mockService.Object.GetEgrantsByStrAsync("", 1, 1, 1, new SessionInfo());

        Assert.Equal("No data found for the search", result?.Message);
        Assert.Null(result?.grantlayer);
    }

    [Fact]
    public async Task ReturnsPagination_WhenGrantLayerPropertyExists()
    {
        var sessionInfo = new SessionInfo { Ic = "IC", UserId = "User" };
        var viewModel = new eGrantsSearchViewModel
        {
            grantlayerproperty = new List<GrantLayer>(),
            Str = "test"
        };

        _mockService.Setup(s => s.eGrantsSearchResults(
            "test", 1, "", 1, 1, sessionInfo, It.IsAny<eGrantsSearchViewModel>(), true))
            .ReturnsAsync(viewModel);

        _mockService.Setup(s => s.LoadPagination("test", "IC", "User", ""))
            .ReturnsAsync(new List<Pagination>());

        var result = await _mockService.Object.GetEgrantsByStrAsync("test", 1, 1, 1, sessionInfo);

        Assert.NotNull(result.Pagination);
        Assert.Equal("test", result.Str);
    }

    [Fact]
    public async Task ThrowsException_AfterMaxRetries()
    {
        _mockService.Setup(s => s.eGrantsSearchResults(
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
            It.IsAny<SessionInfo>(), It.IsAny<eGrantsSearchViewModel>(), true))
            .ThrowsAsync(new Exception("Search failed"));

        await Assert.ThrowsAsync<Exception>(() =>
            _mockService.Object.GetEgrantsByStrAsync("test", 1, 1, 1, new SessionInfo()));
    }

    [Fact]
    public async Task SetsMessage_WhenGrantLayerPropertyIsNull()
    {
        var viewModel = new eGrantsSearchViewModel
        {
            grantlayerproperty = null
        };

        _mockService.Setup(s => s.eGrantsSearchResults(
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
            It.IsAny<SessionInfo>(), It.IsAny<eGrantsSearchViewModel>(), true))
            .ReturnsAsync(viewModel);

        var result = await _mockService.Object.GetEgrantsByStrAsync("test", 1, 1, 1, new SessionInfo());

        Assert.Equal("No data found for the search", result.Message);
        Assert.Null(result.grantlayer);
    }

    [Fact]
    public async Task ReturnsMessage_WhenAllFiltersAreEmpty()
    {
        var result = await _mockService.Object.GetEgrantsByFilterAsync(0, "", 0, "", 1, 1, 1, new SessionInfo());

        Assert.Equal("No data found for the search", result?.Message);
        Assert.Null(result?.grantlayer);
    }

    [Fact]
    public async Task ReturnsSearchResults_WhenFiltersAreValid_AndGrantLayerExists()
    {
        var sessionInfo = new SessionInfo { Ic = "IC", Browser = "Chrome" };
        var filterQuery = new List<FilterSearchResult>();

        filterQuery.Add(new FilterSearchResult { Value = "SELECT * FROM Grants" });

        var viewModel = new eGrantsSearchViewModel
        {
            grantlayerproperty = new List<GrantLayer>(),
            appllayerproperty = new List<ApplLayerObject> { new ApplLayerObject() }
        };

        _mockRepo.Setup(r => r.FilterSearchQuery(2024, "R01", "ADM", 123, 1, sessionInfo))
            .ReturnsAsync(filterQuery);

        _mockService.Setup(s => s.eGrantsSearchResults("SELECT * FROM Grants", 1, "by_filters", 1, 1, sessionInfo, It.IsAny<eGrantsSearchViewModel>(), true))
            .ReturnsAsync(viewModel);

        _mockRepo.Setup(r => r.LoadPaginationAsync("SELECT * FROM Grants", "IC", "Chrome", "by_filters"))
            .ReturnsAsync(new List<Pagination>());

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
        var sessionInfo = new SessionInfo();
        //var filterQuery = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("key", "SELECT * FROM Grants") };
        var filterQuery = new List<FilterSearchResult>();

        filterQuery.Add(new FilterSearchResult { Value = "SELECT * FROM Grants" });

        var viewModel = new eGrantsSearchViewModel
        {
            grantlayerproperty = null
        };

        _mockRepo.Setup(r => r.FilterSearchQuery(2024, "R01", "ADM", 123, 1, sessionInfo))
            .ReturnsAsync(filterQuery);

        _mockService.Setup(s => s.eGrantsSearchResults("SELECT * FROM Grants", 1, "by_filters", 1, 1, sessionInfo, It.IsAny<eGrantsSearchViewModel>(), true))
            .ReturnsAsync(viewModel);

        var result = await _mockService.Object.GetEgrantsByFilterAsync(2024, "R01", 123, "ADM", 1, 1, 1, sessionInfo);

        Assert.Equal("No data found for the search", result?.Message);
        Assert.Null(result?.grantlayer);
    }

    [Fact]
    public async Task SetsFilterFieldsCorrectly_WhenFiltersAreProvided()
    {
        var sessionInfo = new SessionInfo();
        var filterQuery = new List<FilterSearchResult>();
        var viewModel = new eGrantsSearchViewModel
        {
            grantlayerproperty = new List<GrantLayer>(),
            appllayerproperty = new List<ApplLayerObject> { new ApplLayerObject() }
        };

        filterQuery.Add(new FilterSearchResult { Value = "SELECT * FROM Grants" });

        _mockRepo.Setup(r => r.FilterSearchQuery(2024, "R01", "ADM", 123, 1, sessionInfo))
            .ReturnsAsync(filterQuery);

        _mockService.Setup(s => s.eGrantsSearchResults(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), sessionInfo, It.IsAny<eGrantsSearchViewModel>(), true))
            .ReturnsAsync(viewModel);

        var result = await _mockService.Object.GetEgrantsByFilterAsync(2024, "R01", 123, "ADM", 1, 1, 1, sessionInfo);

        Assert.Equal(2024, result.FilterFY);
        Assert.Equal(123, result.FilterSerialNumber);
        Assert.Equal("R01", result.FilterMechanism);
        Assert.Equal("ADM", result.FilterAdminCode);
    }
}

