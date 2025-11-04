using System.Collections.Generic;
using System.Data;

using eGrants.DAL;
using eGrants.DTOs;
using eGrants.Models;
using eGrants.Repositories.Interfaces;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace eGrants.Repositories
{

    // Concrete implementation of IEgrantRepository using Entity Framework Core
    public class eGrantsRepository : IeGrantsRepository
    {
        private readonly AppDbContext _context;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        // Constructor injects the application's database context
        public eGrantsRepository(AppDbContext context, IServiceScopeFactory serviceScopeFactory)
        {
            _context = context;
            _serviceScopeFactory = serviceScopeFactory;
        }

        // Retrieves specific grants from the database asynchronously
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

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Execute the stored procedure and return the results as a list of eGrantsSearchResults.
                return await context.Set<eGrantsSearchResults>()
                    .FromSqlRaw(sql, searchString, grantId, package, applId, currentPage, sessionInfo.Browser, sessionInfo.Ic, sessionInfo.UserId)
                    .ToListAsync();
            }
        }

        public async Task<List<Pagination>> LoadPaginationAsync(string searchString, string ic, string userId, string package = null)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Execute the pagination stored procedure and return the results.
                return await context.LoadPaginationResults
                .FromSqlRaw("EXEC dbo.sp_web_egrants_pagination @str = {0}, @package = {1}, @ic = {2}, @operator = {3}", searchString, package, ic, userId)
                .ToListAsync();
            }
        }

        public async Task<List<FilterSearchResult>> FilterSearchQuery(int fiscalYear, string mechanism, string adminCode, int serialnum, int pageNum, SessionInfo sessionInfo)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Execute the filter search stored procedure and return the results.
                return await context.FilterSearchResults
                .FromSqlRaw("EXEC dbo.sp_web_egrants_search_by_filters @fy = {0}, @mechanism = {1}, @adminCode = {2}, @serialnum = {3}, @page_num = {4}, @browser = {5}, @ic = {6}, @operator = {7}", fiscalYear, mechanism, adminCode, serialnum, pageNum, sessionInfo.Browser, sessionInfo.Ic, sessionInfo.UserId)
                .ToListAsync();
            }
        }

        public async Task<List<GrantDataYears>> GetYearList(string fiscalYear = null, string mechanism = null, string adminCode = null, string serialNumber = null)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Execute the stored procedure to load available data years and return the results.
                return await context.GrantDataYears
                .FromSqlRaw("EXEC dbo.sp_web_egrants_load_data_years @fy = {0}, @mechanism = {1}, @adminCode = {2}, @serialnum = {3}", 
                    fiscalYear, mechanism, adminCode, serialNumber)
                .ToListAsync();
            }
        }

        public async Task<string> GetCategoryNameById(string categories)
        {
            var CategoryNameList = string.Empty;

            List<int> categoryList = new List<int>();

            // Parse the comma-separated category IDs into a list of integers.
            foreach (string item in categories.Split(','))
            {
                if (int.TryParse(item, out int value))
                {
                    categoryList.Add(value);
                }
            }

            List<string> result = new List<string>();
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Query the Categories table for matching IDs and order by name.
                result = await context.Categories
                    .Where(x => categoryList.Contains(x.category_id))
                    .OrderBy(x => x.category_name)
                    .Select(x => x.category_name)
                    .ToListAsync();
            }

            // Concatenate category names into a comma-separated string.
            foreach (var cat in result)
            {
                if (cat != "")
                {
                    if (CategoryNameList == string.Empty)
                    {
                        CategoryNameList = cat.ToString();
                    }
                    else
                    {
                        CategoryNameList = CategoryNameList + "," + cat.ToString();
                    }
                }
            }

            // Trim trailing comma if present.
            if (CategoryNameList != string.Empty && CategoryNameList.IndexOf(",") > 0)
            {
                CategoryNameList = CategoryNameList.Substring(0, CategoryNameList.Length - 2);
            }

            return CategoryNameList;
        }

        public async Task<int> CheckGrantID(int grantId)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                return await context.Grants.Where(x => x.grant_id == grantId).CountAsync();
            }
        }

        public async Task<List<GrantAndStringViewsDto>> GetGrantAndStringViews(int applId)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Join VwAppls and VwTests on Test_id, filter by applId, and project into DTO.
                return await context.VwAppls
                    .Join(_context.VwGrants,
                          a => a.grant_id,
                          g => g.grant_id,
                          (a, g) => new { a, g })
                    .Where(x => x.a.appl_id == applId)
                    .Select(x => new GrantAndStringViewsDto
                    {
                        project_title = x.a.project_title,
                        first_name = x.a.first_name,
                        last_name = x.a.last_name,
                        org_name = x.a.org_name,
                        current_pi_email_address = x.g.current_pi_email_address
                    }).ToListAsync();
            }
        }

        public async Task<List<PersonInvolvement>> GetAllMPIInfo(List<string> applIds)
        {
            try
            {
                // Format application IDs for use in the OPENQUERY SQL string.
                var applsParam = string.Join(",", applIds.Select(id => $"''{id}''")); // Ensure each ID is quoted

                // Construct the OPENQUERY SQL to retrieve MPI information from IRDB.
                var openQuery = $@"
                    SELECT APPL_ID, First_Name, Last_name, Role_Type_Code
                    FROM OPENQUERY(IRDB, '
                        SELECT e.appl_id, d.person_id, d.first_name, d.last_name, d.mi_name src_mi_name,
                               c.email_addr, e.role_type_code, c.addr_type_code
                        FROM person_involvements_mv e
                        JOIN persons_secure d ON d.person_id = e.person_id
                        LEFT OUTER JOIN person_addresses_mv c ON d.person_id = c.person_id
                            AND c.addr_type_code IN (''HOM'') AND c.preferred_addr_code = ''Y''
                        WHERE e.role_type_code IN (''PI'', ''MPI'', ''CPI'')
                            AND appl_id IN ({applsParam})
                            AND d.person_id = e.person_id
                    ')
                ";

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    // Execute the OPENQUERY and return the results.
                    return await context.PersonInvolvements
                        .FromSqlRaw(openQuery)
                        .ToListAsync();
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            return null; // applicantDict;
        }

        public async Task<List<FilterSearchResult>> GetApplsList(int grantId, string flagType, string years)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Execute the stored procedure to load application IDs and return the results.
                return await context.FilterSearchResults
                .FromSqlRaw("EXEC dbo.sp_web_egrants_load_applid_string @grant_id = {0}, @flag_type = {1}, @years = {2}", grantId, flagType, years)
                .ToListAsync();
            }
        }

        public virtual async Task<List<supplement>> GetSupplements(string act, int grantId, int supportYear, string suffixCode, string docidStr, int formerApplId, string ic, string userId)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Execute the stored procedure to load application IDs and return the results.
                return await context.supplements
                .FromSqlRaw("EXEC dbo.sp_web_egrants_supplement @act = {0}, @grant_id = {1}, @support_year = {2}, @suffix_code = {3}, @docid_str = {4}, @former_applid = {5}, @ic = {6}, @Operator = {7}", act, grantId, (byte)supportYear, suffixCode, docidStr, formerApplId, ic, userId)
                .ToListAsync();
            }
        }

        public async Task<int> CheckApplID(int applId)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                return await context.VwAppls.Where(x => x.appl_id == applId).CountAsync();
            }
        }

        public async Task<int?> GetGrantID(int applId)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                return await context.VwAppls
                    .Where(x => x.appl_id == applId)
                    .Select(x => x.grant_id)
    .               FirstOrDefaultAsync();

            }
        }
    }
}
