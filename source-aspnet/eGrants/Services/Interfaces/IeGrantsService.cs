using eGrants.Models;

namespace eGrants.Services.Interfaces
{
    public interface IeGrantsService
    {
        public Task<List<eGrantsSearchResults>> GetEgrantsByStrAsync(string aSearchString, int aGrantId, string aPackage, int aApplId, int aCurrentPage, string aBrowser, string aIC, string aOperator);
    }
}
