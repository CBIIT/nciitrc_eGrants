using System.Data;

using eGrants.DTOs;
using eGrants.Models;
using eGrants.ViewModels;

using Microsoft.Data.SqlClient;

namespace eGrants.Repositories.Interfaces
{
    public interface IeGrantsRepository
    {
        /// <summary>
        /// Asynchronously retrieves search results for eGrants based on the specified criteria.
        /// </summary>
        /// <param name="searchString">The search keyword or phrase.</param>
        /// <param name="grantId">The unique identifier of the grant.</param>
        /// <param name="package">The package type or name.</param>
        /// <param name="applId">The application ID to filter results.</param>
        /// <param name="currentPage">The current page number for pagination.</param>
        /// <param name="sessionInfo">Session-related information for the current user.</param>
        /// <returns>A task that returns a list of <see cref="eGrantsSearchResults"/> objects.</returns>
        Task<List<eGrantsSearchResults>> GetSearchResultsAsync(string searchString, int grantId, string package, int applId, int currentPage, SessionInfo sessionInfo);

        /// <summary>
        /// Asynchronously loads pagination details for a given search query and user context.
        /// </summary>
        /// <param name="searchString">The search keyword or phrase.</param>
        /// <param name="ic">The institute code or identifier.</param>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="package">The package type or name.</param>
        /// <returns>A task that returns a list of <see cref="Pagination"/> objects.</returns>
        Task<List<Pagination>> LoadPaginationAsync(string searchString, string ic, string userId, string package);

        /// <summary>
        /// Filters search results based on fiscal year, mechanism, administrative code, serial number, and page number.
        /// </summary>
        /// <param name="fiscalYear">The fiscal year to filter by.</param>
        /// <param name="mechanism">The funding mechanism.</param>
        /// <param name="adminCode">The administrative code.</param>
        /// <param name="serialnum">The serial number of the application.</param>
        /// <param name="pageNum">The page number for pagination.</param>
        /// <param name="sessionInfo">Session-related information for the current user.</param>
        /// <returns>A task that returns a list of <see cref="FilterSearchResult"/> objects.</returns>
        Task<List<FilterSearchResult>> FilterSearchQuery(int fiscalYear, string mechanism, string adminCode, int serialnum, int pageNum, SessionInfo sessionInfo);

        /// <summary>
        /// Retrieves a list of grant data years based on the specified fiscal year and grant identifiers.
        /// </summary>
        /// <param name="fiscalYear">The fiscal year to filter by.</param>
        /// <param name="mechanism">The funding mechanism.</param>
        /// <param name="adminCode">The administrative code.</param>
        /// <param name="serialNumber">The serial number of the grant.</param>
        /// <returns>A task that returns a list of <see cref="GrantDataYears"/> objects.</returns>
        Task<List<GrantDataYears>> GetYearList(string fiscalYear, string mechanism, string adminCode, string serialNumber);

        /// <summary>
        /// Checks whether a given grant ID exists or is valid.
        /// </summary>
        /// <param name="grantId">The grant ID to validate.</param>
        /// <returns>A task that returns an integer indicating the result of the check.</returns>
        Task<int> CheckGrantID(int grantId);

        /// <summary>
        /// Retrieves the category name associated with the given category identifier.
        /// </summary>
        /// <param name="categories">The category identifier(s).</param>
        /// <returns>A task that returns the category name as a string.</returns>
        Task<string> GetCategoryNameById(string categories);

        /// <summary>
        /// Retrieves grant and string view details for a specific application ID.
        /// </summary>
        /// <param name="applId">The application ID.</param>
        /// <returns>A task that returns a list of <see cref="GrantAndStringViewsDto"/> objects.</returns>
        Task<List<GrantAndStringViewsDto>> GetGrantAndStringViews(int applId);

        /// <summary>
        /// Retrieves information about all MPI (Multiple Principal Investigators) for the specified application IDs.
        /// </summary>
        /// <param name="applIds">A list of application IDs.</param>
        /// <returns>A task that returns a list of <see cref="PersonInvolvement"/> objects.</returns>
        Task<List<PersonInvolvement>> GetAllMPIInfo(List<string> applIds);

        /// <summary>
        /// Retrieves a filtered list of applications based on grant ID, flag type, and year.
        /// </summary>
        /// <param name="grantId">The grant ID to filter by.</param>
        /// <param name="flagType">The type of flag used for filtering.</param>
        /// <param name="years">The year(s) to filter by.</param>
        /// <returns>A task that returns a list of <see cref="FilterSearchResult"/> objects.</returns>
        Task<List<FilterSearchResult>> GetApplsList(int grantId, string flagType, string years);

        /// <summary>
        /// Retrieves supplement information for a given grant and support year.
        /// </summary>
        /// <param name="act">The activity code.</param>
        /// <param name="grantId">The grant ID.</param>
        /// <param name="supportYear">The support year.</param>
        /// <param name="suffixCode">The suffix code.</param>
        /// <param name="docidStr">The document ID string.</param>
        /// <param name="formerApplId">The former application ID.</param>
        /// <param name="ic">The institute code.</param>
        /// <param name="userId">The user ID.</param>
        /// <returns>A task that returns a list of <see cref="supplement"/> objects.</returns>
        Task<List<supplement>> GetSupplements(string act, int grantId, int supportYear, string suffixCode, string docidStr, int formerApplId, string ic, string userId);

        /// <summary>
        /// Retrieves the grant ID if it exists.
        /// </summary>
        /// <param name="grantId">The grant ID to retrieve.</param>
        /// <returns>A task that returns the grant ID as a nullable integer.</returns>
        Task<int?> GetGrantID(int grantId);

        /// <summary>
        /// Checks whether a given application ID exists or is valid.
        /// </summary>
        /// <param name="applId">The application ID to validate.</param>
        /// <returns>A task that returns an integer indicating the result of the check.</returns>
        Task<int> CheckApplID(int applId);

        /// <summary>
        /// Retrieves a list of category names associated with a specific grant and year.
        /// </summary>
        /// <param name="grantId">The grant ID.</param>
        /// <param name="years">The year(s) to filter by.</param>
        /// <returns>A task that returns a list of category names as strings.</returns>
        Task<List<string>> GetCategoryList(int grantId, string years);
        Task<List<VwApplDTO>> LoadApplsByApplid(int? applId);

        Task<List<string>> LoadDataAutocomplete(string sql_query, string term, string mechanism, string fy, string adminCode, string serialNum);
    }
}