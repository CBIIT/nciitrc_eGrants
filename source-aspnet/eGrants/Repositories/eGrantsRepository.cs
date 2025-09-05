using Microsoft.EntityFrameworkCore;
using eGrants.DAL;
using eGrants.Models;
using eGrants.Repositories.Interfaces;
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
        public async Task<List<eGrantsSearchResults>> GetEgrantsByStrAsync(string aSearchString, int aGrantId, string aPackage, int aApplId, int aCurrentPage, string aBrowser, string aIC, string aOperator)
        {
            return await _context.eGrantsSearchResults
                .FromSqlRaw("EXEC dbo.sp_web_egrants @str = {0}, @grant_id = {1}, @package = {2}, @appl_id = {3}, @current_page = {4}, @browser = {5}, @ic = {6}, @operator = {7}", aSearchString, aGrantId, aPackage, aApplId, aCurrentPage, aBrowser, aIC, aOperator)
                .ToListAsync();
        }
    }
}
