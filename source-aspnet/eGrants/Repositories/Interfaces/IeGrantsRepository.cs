using eGrants.Models;
using eGrants.ViewModels;

namespace eGrants.Repositories.Interfaces
{
    public interface IeGrantsRepository
    {
        Task<List<eGrantsSearchResults>> GetSearchResultsAsync(string aSearchString, int aGrantId, string aPackage, int aApplId, int aCurrentPage, string aBrowser, string aIC, string aOperator);

        Task<List<Pagination>> LoadPaginationAsync(string aSearchString, string aIC, string aOperator, string aPackage);

        Task<List<FilterSearchResult>> FilterSearchQuery(int aFiscalYear, string aMechanism, string aAdminCode, int aSerialnum, int aPageNum, string aBrowser, string aIc, string aUserId);

        Task<List<GrantDataYears>> GetYearList(string aFiscalYear, string aMechanism, string aAdminCode, string aSerialNumber);
    }
}
