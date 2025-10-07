using System.Data;

using eGrants.Models;
using eGrants.ViewModels;

using Microsoft.Data.SqlClient;

namespace eGrants.Services.Interfaces
{
    public interface IeGrantsService
    {
        public Task<eGrantsSearchViewModel> GetEgrantsByStrAsync(string searchString, int grantId, int applId, int currentPage, SessionInfo sessionInfo);

        public Task<eGrantsSearchViewModel> GetEgrantsByFilterAsync(int fiscalYear, string mechanism, int serialNum, string adminCode, int grantId, int applId, int currentPage, SessionInfo sessionInfo);

        public Task<eGrantsSearchViewModel> GetEgrantsByGrantAsync(string searchString, int grantId, string package, int applId, int currentPage, string categories, string applsList, string years, string mode, SessionInfo sessionInfo);

        public Task<List<Pagination>> LoadPagination(string searchString, string ic, string userId, string package);

        public Task<List<FilterSearchResult>> FilterSearchQuery(int fiscalYear, string mechanism, string adminCode, int serialnum, int pageNum, SessionInfo sessionInfo);

        public Task<List<GrantDataYears>> GetYearList(string fiscalYear, string mechanism, string adminCode, string serialNumber);

        public Task<int> CheckGrantID(int grantId);

        public Task<string> GetCategoryNameById(string categories);

        public Task<List<FilterSearchResult>> GetApplsList(int grantId, string flagType = null, string years = null);

        public Task<eGrantsSearchViewModel> eGrantsSearchResults(string searchString, int grantId, string package, int applId, int currentPage, SessionInfo sessionInfo, eGrantsSearchViewModel searchByStrViewModel, Boolean loadPagination);
    }
}
