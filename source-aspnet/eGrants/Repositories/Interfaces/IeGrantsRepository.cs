using System.Data;

using eGrants.DTOs;
using eGrants.Models;
using eGrants.ViewModels;

using Microsoft.Data.SqlClient;

namespace eGrants.Repositories.Interfaces
{
    public interface IeGrantsRepository
    {
        Task<List<eGrantsSearchResults>> GetSearchResultsAsync(string searchString, int grantId, string package, int applId, int currentPage, SessionInfo sessionInfo);

        Task<List<Pagination>> LoadPaginationAsync(string searchString, string ic, string userId, string package);

        Task<List<FilterSearchResult>> FilterSearchQuery(int fiscalYear, string mechanism, string adminCode, int serialnum, int pageNum, SessionInfo sessionInfo);

        Task<List<GrantDataYears>> GetYearList(string fiscalYear, string mechanism, string adminCode, string serialNumber);

        Task<int> CheckGrantID(int grantId);

        Task<string> GetCategoryNameById(string categories);

        Task<List<GrantAndStringViewsDto>> GetGrantAndStringViews(int applId);

        //Task<Dictionary<string, List<ApplicantDto>>> GetAllMPIInfo(List<string> applIds);

        Task<List<PersonInvolvement>> GetAllMPIInfo(List<string> applIds);

        Task<List<FilterSearchResult>> GetApplsList(int grantId, string flagType, string years);

        Task<List<supplement>> GetSupplements(string act, int grantId, int supportYear, string suffixCode, string docidStr, int formerApplId, string ic, string userId);
        Task<List<string>> GetCategoryList(int grantId, string years);
    }
}
