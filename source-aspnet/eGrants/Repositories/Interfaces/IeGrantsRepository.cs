using System.Data;

using eGrants.DTOs;
using eGrants.Models;
using eGrants.ViewModels;

using Microsoft.Data.SqlClient;

namespace eGrants.Repositories.Interfaces
{
    public interface IeGrantsRepository
    {
        Task<List<eGrantsSearchResults>> GetSearchResultsAsync(string aSearchString, int aGrantId, string aPackage, int aApplId, int aCurrentPage, string aBrowser, string aIC, string aOperator);

        Task<List<Pagination>> LoadPaginationAsync(string aSearchString, string aIC, string aOperator, string aPackage);

        Task<List<FilterSearchResult>> FilterSearchQuery(int aFiscalYear, string aMechanism, string aAdminCode, int aSerialnum, int aPageNum, string aBrowser, string aIc, string aUserId);

        Task<List<GrantDataYears>> GetYearList(string aFiscalYear, string aMechanism, string aAdminCode, string aSerialNumber);

        Task<int> CheckGrantID(int aGrantId);

        Task<string> GetCategoryNameById(string aCategories);

        Task<List<GrantAndStringViewsDto>> GetGrantAndStringViews(int aApplId);

        //Task<Dictionary<string, List<ApplicantDto>>> GetAllMPIInfo(List<string> appl_ids);

        Task<List<PersonInvolvement>> GetAllMPIInfo(List<string> appl_ids);

        Task<List<FilterSearchResult>> GetApplsList(int aGrantId, string aFlagType, string aYears);
    }
}
