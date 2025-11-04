using System.Data;

using eGrants.Models;
using eGrants.ViewModels;

using Microsoft.Data.SqlClient;

namespace eGrants.Services.Interfaces
{
    public interface IeGrantsService
    {
        /// <summary>
        /// Retrieves eGrants search results based on a search string and identifiers.
        /// </summary>
        /// <param name="searchString">The keyword or phrase to search for.</param>
        /// <param name="grantId">The unique identifier of the grant.</param>
        /// <param name="applId">The application ID.</param>
        /// <param name="currentPage">The current page number for pagination.</param>
        /// <param name="sessionInfo">Session context information for the user.</param>
        /// <returns>A view model containing eGrants search results.</returns>
        public Task<eGrantsSearchViewModel> GetEgrantsByStrAsync(string searchString, int grantId, int applId, int currentPage, SessionInfo sessionInfo);

        /// <summary>
        /// Retrieves eGrants search results based on filter criteria such as fiscal year and mechanism.
        /// </summary>
        /// <param name="fiscalYear">The fiscal year to filter by.</param>
        /// <param name="mechanism">The funding mechanism.</param>
        /// <param name="serialNum">The serial number of the grant.</param>
        /// <param name="adminCode">The administrative code.</param>
        /// <param name="grantId">The grant ID.</param>
        /// <param name="applId">The application ID.</param>
        /// <param name="currentPage">The current page number for pagination.</param>
        /// <param name="sessionInfo">Session context information for the user.</param>
        /// <returns>A view model containing filtered eGrants search results.</returns>
        public Task<eGrantsSearchViewModel> GetEgrantsByFilterAsync(int fiscalYear, string mechanism, int serialNum, string adminCode, int grantId, int applId, int currentPage, SessionInfo sessionInfo);

        /// <summary>
        /// Retrieves eGrants search results using detailed grant-related parameters.
        /// </summary>
        /// <param name="searchString">The keyword or phrase to search for.</param>
        /// <param name="grantId">The unique identifier of the grant.</param>
        /// <param name="package">The package type or name.</param>
        /// <param name="applId">The application ID.</param>
        /// <param name="currentPage">The current page number for pagination.</param>
        /// <param name="categories">The category ID(s) as a string.</param>
        /// <param name="applsList">A list of application identifiers.</param>
        /// <param name="years">The year(s) to filter applications.</param>
        /// <param name="mode">The search mode or context.</param>
        /// <param name="sessionInfo">Session context information for the user.</param>
        /// <returns>A view model containing eGrants search results.</returns>
        public Task<eGrantsSearchViewModel> GetEgrantsByGrantAsync(string searchString, int grantId, string package, int applId, int currentPage, string categories, string applsList, string years, string mode, SessionInfo sessionInfo);

        /// <summary>
        /// Loads pagination metadata for search results based on the provided filters.
        /// </summary>
        /// <param name="searchString">The search keyword or phrase.</param>
        /// <param name="ic">Institute or center code.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="package">The package type or name.</param>
        /// <returns>A list of pagination details for the search results.</returns>
        public Task<List<Pagination>> LoadPagination(string searchString, string ic, string userId, string package);

        /// <summary>
        /// Filters search results based on fiscal year, mechanism, admin code, and serial number.
        /// </summary>
        /// <param name="fiscalYear">The fiscal year to filter by.</param>
        /// <param name="mechanism">The funding mechanism.</param>
        /// <param name="adminCode">The administrative code.</param>
        /// <param name="serialnum">The serial number of the grant.</param>
        /// <param name="pageNum">The page number for pagination.</param>
        /// <param name="sessionInfo">Session context information for the user.</param>
        /// <returns>A filtered list of search results.</returns>
        public Task<List<FilterSearchResult>> FilterSearchQuery(int fiscalYear, string mechanism, string adminCode, int serialnum, int pageNum, SessionInfo sessionInfo);

        /// <summary>
        /// Retrieves a list of grant data years based on fiscal year and other identifiers.
        /// </summary>
        /// <param name="fiscalYear">The fiscal year to query.</param>
        /// <param name="mechanism">The funding mechanism.</param>
        /// <param name="adminCode">The administrative code.</param>
        /// <param name="serialNumber">The serial number of the grant.</param>
        /// <returns>A list of grant data years.</returns>
        public Task<List<GrantDataYears>> GetYearList(string fiscalYear, string mechanism, string adminCode, string serialNumber);

        /// <summary>
        /// Checks whether a given grant ID exists in the system.
        /// </summary>
        /// <param name="grantId">The grant ID to validate.</param>
        /// <returns>An integer indicating the existence or status of the grant ID.</returns>
        public Task<int> CheckGrantID(int grantId);

        /// <summary>
        /// Retrieves the category name associated with the given category ID(s).
        /// </summary>
        /// <param name="categories">The category ID(s) as a string.</param>
        /// <returns>The name of the category.</returns>
        public Task<string> GetCategoryNameById(string categories);

        /// <summary>
        /// Retrieves a list of applications associated with a specific grant ID, flag type, and year(s).
        /// </summary>
        /// <param name="grantId">The grant ID to filter applications.</param>
        /// <param name="flagType">The type of flag to apply (optional).</param>
        /// <param name="years">The year(s) to filter applications (optional).</param>
        /// <returns>A list of filtered application search results.</returns>
        public Task<List<FilterSearchResult>> GetApplsList(int grantId, string flagType = null, string years = null);

        /// <summary>
        /// Retrieves eGrants search results based on search parameters and optionally loads pagination.
        /// </summary>
        /// <param name="searchString">The keyword or phrase to search for.</param>
        /// <param name="grantId">The unique identifier of the grant.</param>
        /// <param name="package">The package type or name.</param>
        /// <param name="applId">The application ID.</param>
        /// <param name="currentPage">The current page number for pagination.</param>
        /// <param name="sessionInfo">Session context information for the user.</param>
        /// <param name="searchByStrViewModel">The view model containing search results by string.</param>
        /// <param name="loadPagination">Flag indicating whether to load pagination data.</param>
        /// <returns>A view model containing eGrants search results.</returns>
        public Task<eGrantsSearchViewModel> eGrantsSearchResults(string searchString, int grantId, string package, int applId, int currentPage, SessionInfo sessionInfo, eGrantsSearchViewModel searchByStrViewModel, Boolean loadPagination);

        /// <summary>
        /// Retrieves a list of supplement records associated with a specific grant application and context.
        /// </summary>
        /// <param name="act">The activity code representing the type of grant or funding mechanism.</param>
        /// <param name="grantId">The unique identifier for the grant.</param>
        /// <param name="supportYear">The support year of the grant (e.g., year of funding).</param>
        /// <param name="suffixCode">An optional suffix code used to distinguish grant components or segments.</param>
        /// <param name="docidStr">A string representing the document ID related to the grant application.</param>
        /// <param name="formerApplId">The identifier for a former application, used for historical reference or linkage.</param>
        /// <param name="ic">The institute or center code associated with the grant.</param>
        /// <param name="userId">The identifier of the user making the request, used for authorization or auditing.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a list of <see cref="supplement"/> objects
        /// matching the specified criteria.
        /// </returns>
        public Task<List<supplement>> GetSupplements(string act, int grantId, int supportYear, string suffixCode, string docidStr, int formerApplId, string ic, string userId);

        /// <summary>
        /// Retrieves a list of category names associated with a specific grant and year range.
        /// </summary>
        /// <param name="grantId">The unique identifier of the grant for which categories are requested.</param>
        /// <param name="years">
        /// A comma-separated string representing one or more years (e.g., "2022,2023") used to filter categories.
        /// </param>
        /// <returns>
        /// A task that resolves to a list of category names matching the specified grant and year criteria.
        /// </returns>
        public Task<List<string>> GetCategoryList(int grantId, string years);
    }
}
