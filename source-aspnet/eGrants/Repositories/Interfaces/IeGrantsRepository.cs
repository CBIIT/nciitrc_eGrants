using eGrants.Models;

namespace eGrants.Repositories.Interfaces
{
    public interface IeGrantsRepository
    {
        Task<List<eGrantsSearchResults>> GetEgrantsByStrAsync(string aSearchString, int aGrantId, string aPackage, int aApplId, int aCurrentPage, string aBrowser, string aIC, string aOperator);
    }
}
