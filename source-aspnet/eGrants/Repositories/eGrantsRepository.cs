using System.Security.Principal;

using eGrants.DAL;
using eGrants.Models;
using eGrants.Repositories.Interfaces;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;

namespace eGrants.Repositories
{
    // Concrete implementation of IEgrantRepository using Entity Framework Core
    public class eGrantsRepository : IeGrantsRepository
    {
        private readonly AppDbContext _context;

        // Constructor injects the application's database context
        public eGrantsRepository(AppDbContext context)
        {
            _context = context;
        }

        // Retrieves specific grants from the database asynchronously
        public async Task<List<eGrantsSearchResults>> GetSearchResultsAsync(string aSearchString, int aGrantId, string aPackage, int aApplId, int aCurrentPage, string aBrowser, string aIC, string aOperator)
        {
            return await _context.eGrantsSearchResults
                .FromSqlRaw("EXEC dbo.sp_web_egrants @str = {0}, @grant_id = {1}, @package = {2}, @appl_id = {3}, @current_page = {4}, @browser = {5}, @ic = {6}, @operator = {7}", aSearchString, aGrantId, aPackage, aApplId, aCurrentPage, aBrowser, aIC, aOperator)
                .ToListAsync();
        }

        public async Task<List<Pagination>> LoadPaginationAsync(string aSearchString, string aIC, string aOperator, string aPackage = null)
        {
            return await _context.LoadPaginationResults
                .FromSqlRaw("EXEC dbo.sp_web_egrants_pagination @str = {0}, @package = {1}, @ic = {2}, @operator = {3}", aSearchString, aPackage, aIC, aOperator)
                .ToListAsync();
        }

        public async Task<List<FilterSearchResult>> FilterSearchQuery(int aFiscalYear, string aMechanism, string aAdminCode, int aSerialnum, int aPageNum, string aBrowser, string aIc, string aOperator)
        {
            return await _context.FilterSearchResults
                .FromSqlRaw("EXEC dbo.sp_web_egrants_search_by_filters @fy = {0}, @mechanism = {1}, @adminCode = {2}, @serialnum = {3}, @page_num = {4}, @browser = {5}, @ic = {6}, @operator = {7}", aFiscalYear, aMechanism, aAdminCode, aSerialnum, aPageNum, aBrowser, aIc, aOperator)
                .ToListAsync();
        }

        public async Task<List<GrantDataYears>> GetYearList(string aFiscalYear = null, string aMechanism = null, string aAdminCode = null, string aSerialNumber = null)
        {
            return await _context.GrantDataYears
                .FromSqlRaw("EXEC dbo.sp_web_egrants_load_data_years @fy = {0}, @mechanism = {1}, @adminCode = {2}, @serialnum = {3}", aFiscalYear, aMechanism, aAdminCode, aSerialNumber)
                .ToListAsync();
        }
    }
}
