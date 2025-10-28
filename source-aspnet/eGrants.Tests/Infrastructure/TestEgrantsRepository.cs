using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using eGrants.DAL;
using eGrants.DTOs;
using eGrants.Models;
using eGrants.Repositories.Interfaces;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace eGrants.Tests.Infrastructure
{
    public class TestEGrantsRepository : IeGrantsRepository
    {
        private readonly TestDbContext _context;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public TestEGrantsRepository(TestDbContext context)
        {
            _context = context;
        }

        public async Task<List<supplement>> GetSupplements(string act, int grantId, int supportYear, string suffixCode, string docidStr, int formerApplId, string ic, string userId)
        {
            return await _context.Supplements
                .Where(s => s.grant_id == grantId)
                .ToListAsync();
        }

        //public Task<List<eGrantsSearchResults>> GetSearchResultsAsync(string searchString, int grantId, string package, int applId, int currentPage, SessionInfo sessionInfo) => throw new NotImplementedException();


        public async Task<List<eGrantsSearchResults>> GetSearchResultsAsync(string searchString, int grantId, string package, int applId, int currentPage, SessionInfo sessionInfo)
        {
            var sql = @"
                EXEC dbo.sp_web_egrants 
                    @str = {0}, 
                    @grant_id = {1}, 
                    @package = {2}, 
                    @appl_id = {3}, 
                    @current_page = {4}, 
                    @browser = {5}, 
                    @ic = {6}, 
                    @operator = {7}";

            //using (var scope = _serviceScopeFactory.CreateScope())
            //{
            //    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Execute the stored procedure and return the results as a list of eGrantsSearchResults.
                return await _context.Set<eGrantsSearchResults>()
                    .FromSqlRaw(sql, searchString, grantId, package, applId, currentPage, sessionInfo.Browser, sessionInfo.Ic, sessionInfo.UserId)
                    .ToListAsync();
            //}
        }


        public Task<List<Pagination>> LoadPaginationAsync(string searchString, string ic, string userId, string package) => throw new NotImplementedException();

        public Task<List<FilterSearchResult>> FilterSearchQuery(int fiscalYear, string mechanism, string adminCode, int serialnum, int pageNum, SessionInfo sessionInfo) => throw new NotImplementedException();

        public Task<List<GrantDataYears>> GetYearList(string fiscalYear, string mechanism, string adminCode, string serialNumber) => throw new NotImplementedException();

        public Task<int> CheckGrantID(int grantId) => throw new NotImplementedException();

        public Task<string> GetCategoryNameById(string categories) => throw new NotImplementedException();

        public Task<List<GrantAndStringViewsDto>> GetGrantAndStringViews(int applId) => throw new NotImplementedException();

        //Task<Dictionary<string, List<ApplicantDto>>> GetAllMPIInfo(List<string> applIds) => throw new NotImplementedException();

        public Task<List<PersonInvolvement>> GetAllMPIInfo(List<string> applIds) => throw new NotImplementedException();

        public Task<List<FilterSearchResult>> GetApplsList(int grantId, string flagType, string years) => throw new NotImplementedException();
    }

}
