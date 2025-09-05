using eGrants.DTOs;
using eGrants.Models;
using eGrants.ViewModels;

namespace eGrants.Services.Interfaces
{
    public interface IeGrantsService
    {
        public Task<eGrantsSearchByStrViewModel> GetEgrantsByStrAsync(string aSearchString, int aGrantId, string aPackage, int aApplId, int aCurrentPage, string aBrowser, string aIC, string aOperator, string aMode);
    }
}
