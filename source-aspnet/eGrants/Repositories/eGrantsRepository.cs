using System.Data;
using System.Security.Principal;

using eGrants.DAL;
using eGrants.DTOs;
using eGrants.Models;
using eGrants.Repositories.Interfaces;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.Extensions.DependencyInjection;

using static System.Formats.Asn1.AsnWriter;

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
        public async Task<List<eGrantsSearchResults>> GetSearchResultsAsync(string aSearchString, int aGrantId, string aPackage, int aApplId, int aCurrentPage, string aBrowser, string aIC, string aOperator)
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
            //try
            //{
            //var test = await _context.Set<eGrantsSearchResults>()
            //    .FromSqlRaw(sql, aSearchString, aGrantId, aPackage, aApplId, aCurrentPage, aBrowser, aIC, aOperator)
            //    .ToListAsync();
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                return await context.Set<eGrantsSearchResults>()
                    .FromSqlRaw(sql, aSearchString, aGrantId, aPackage, aApplId, aCurrentPage, aBrowser, aIC, aOperator)
                    .ToListAsync();
            }
            //catch (Exception ex)
            //{
            //    return new List<eGrantsSearchResults>();
            //}
    //return await _context.eGrantsSearchResults
    //.FromSqlRaw("EXEC dbo.sp_web_egrants @str = {0}, @grant_id = {1}, @package = {2}, @appl_id = {3}, @current_page = {4}, @browser = {5}, @ic = {6}, @operator = {7}", aSearchString, aGrantId, aPackage, aApplId, aCurrentPage, aBrowser, aIC, aOperator)
    //.ToListAsync();
}

        public async Task<List<Pagination>> LoadPaginationAsync(string aSearchString, string aIC, string aOperator, string aPackage = null)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                return await context.LoadPaginationResults
                .FromSqlRaw("EXEC dbo.sp_web_egrants_pagination @str = {0}, @package = {1}, @ic = {2}, @operator = {3}", aSearchString, aPackage, aIC, aOperator)
                .ToListAsync();
            }
        }

        public async Task<List<FilterSearchResult>> FilterSearchQuery(int aFiscalYear, string aMechanism, string aAdminCode, int aSerialnum, int aPageNum, string aBrowser, string aIc, string aOperator)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                return await context.FilterSearchResults
                .FromSqlRaw("EXEC dbo.sp_web_egrants_search_by_filters @fy = {0}, @mechanism = {1}, @adminCode = {2}, @serialnum = {3}, @page_num = {4}, @browser = {5}, @ic = {6}, @operator = {7}", aFiscalYear, aMechanism, aAdminCode, aSerialnum, aPageNum, aBrowser, aIc, aOperator)
                .ToListAsync();
            }
        }

        public async Task<List<GrantDataYears>> GetYearList(string aFiscalYear = null, string aMechanism = null, string aAdminCode = null, string aSerialNumber = null)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                return await context.GrantDataYears
                .FromSqlRaw("EXEC dbo.sp_web_egrants_load_data_years @fy = {0}, @mechanism = {1}, @adminCode = {2}, @serialnum = {3}", aFiscalYear, aMechanism, aAdminCode, aSerialNumber)
                .ToListAsync();
            }
        }

        public async Task<string> GetCategoryNameById(string aCategories)
        {
            var CategoryNameList = string.Empty;

            List<int> categoryList = new List<int>();

            foreach (string item in aCategories.Split(','))
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
                result = await context.Categories.Where(x => categoryList.Contains(x.category_id)).OrderBy(x => x.category_name).Select(x => x.category_name).ToListAsync();
            }

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

            //using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
            //{
            //    var cmd = new SqlCommand(
            //        "select category_name from categories where category_id in (" + categories + ") order by category_name",
            //        conn);

            //    cmd.CommandType = CommandType.Text;
            //    cmd.Parameters.AddWithValue("@categories", categories);

            //    // cmd.Parameters.AddWithValue("@years", years);
            //    conn.Open();
            //    var rdr = cmd.ExecuteReader();

            //    while (rdr.Read())
            //    {
            //        var category = rdr[0] + ", ";
            //        CategoryNameList = CategoryNameList + category;
            //    }

            //    // added by Leon 5/11/2019
            //    // conn.Close();
            //}

            if (CategoryNameList != string.Empty && CategoryNameList.IndexOf(",") > 0)
            {
                CategoryNameList = CategoryNameList.Substring(0, CategoryNameList.Length - 2);
            }

            return CategoryNameList;
        }

        public async Task<int> CheckGrantID(int aGrantId)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                return await context.Grants.Where(x => x.grant_id == aGrantId).CountAsync();
            }
            //return result;

            //using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
            //{
            //    var cmd = new SqlCommand("select count(*) as count_id from grants where grant_id = @grant_id", conn);
            //    cmd.CommandType = CommandType.Text;
            //    cmd.Parameters.Add("@grant_id", SqlDbType.Int).Value = grant_id;

            //    conn.Open();
            //    var exists = 0;
            //    var rdr = cmd.ExecuteReader();

            //    while (rdr.Read())
            //    {
            //        exists = Convert.ToInt32(rdr["count_id"]);
            //    }

            //    //conn.Close();

            //    return exists;
            //}
        }

        public async Task<List<GrantAndStringViewsDto>> GetGrantAndStringViews(int aApplId)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                return await context.VwAppls
                    .Join(_context.VwGrants,
                          a => a.grant_id,
                          g => g.grant_id,
                          (a, g) => new { a, g })
                    .Where(x => x.a.appl_id == aApplId)
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

        //public async Task<Dictionary<string, List<ApplicantDto>>> GetAllMPIInfo(List<string> appl_ids)
        public async Task<List<PersonInvolvement>> GetAllMPIInfo(List<string> appl_ids)
        {
            //var roleTypes = new[] { "PI", "MPI", "CPI" };

            try
            {
                //var applicantList = await _context.PersonInvolvements
                //    .Where(e => roleTypes.Contains(e.RoleTypeCode) && appl_ids.Contains(e.ApplId))
                //    .Join(_context.Persons,
                //        e => e.PersonId,
                //        d => d.PersonId,
                //        (e, d) => new { e, d })
                //    .SelectMany(joined => _context.PersonAddresses
                //        .Where(c => c.PersonId == joined.d.PersonId &&
                //                    c.AddrTypeCode == "HOM" &&
                //                    c.PreferredAddrCode == "Y")
                //        .DefaultIfEmpty(),
                //        (joined, c) => new ApplicantDto
                //        {
                //            ApplId = joined.e.ApplId,
                //            FirstName = joined.d.FirstName,
                //            LastName = joined.d.LastName,
                //            RoleTypeCode = joined.e.RoleTypeCode,
                //            EmailAddr = c != null ? c.EmailAddr : null
                //        })
                //    .ToListAsync();
                var applsParam = string.Join(",", appl_ids.Select(id => $"''{id}''")); // Ensure each ID is quoted
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

        public async Task<List<FilterSearchResult>> GetApplsList(int aGrantId, string aFlagType, string aYears)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                return await context.FilterSearchResults
                .FromSqlRaw("EXEC dbo.sp_web_egrants_load_applid_string @grant_id = {0}, @flag_type = {1}, @years = {2}", aGrantId, aFlagType, aYears)
                .ToListAsync();
            }
        }
    }
}
