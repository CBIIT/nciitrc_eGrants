using eGrants.Models;
using eGrants.ViewModels;

namespace eGrants.Services.Interfaces
{
    public interface IeGrantsService
    {
        public Task<eGrantsSearchViewModel> GetEgrantsByStrAsync(string aSearchString, int aGrantId, int aApplId, int aCurrentPage, string aBrowser, string aIC, string aOperator);

        public Task<eGrantsSearchViewModel> GetEgrantsByFilterAsync(int aFiscalYear, string aMechanism, int aSerialNum, string aAdminCode, int aGrantId, int aApplId, int aCurrentPage, string aBrowser, string aIC, string aOperator);


        public Task<List<Pagination>> LoadPagination(string aSearchString, string aIC, string aUserId, string aPackage);

        public Task<List<FilterSearchResult>> FilterSearchQuery(int aFiscalYear, string aMechanism, string aAdminCode, int aSerialnum, int aPageNum, string aBrowser, string aIc, string aUserId);

        public Task<List<GrantDataYears>> GetYearList(string aFiscalYear, string aMechanism, string aAdminCode, string aSerialNumber);
    }
}
