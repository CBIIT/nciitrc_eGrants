using System.Collections.Generic;
using System.Data;
using System.Text;

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
            var results = new List<eGrantsSearchResults>();

            using (var conn = new SqlConnection(_context.Database.GetConnectionString()))
            using (var cmd = new SqlCommand("dbo.sp_web_egrants", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 60;

                cmd.Parameters.AddWithValue("@str", searchString ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@grant_id", grantId);
                cmd.Parameters.AddWithValue("@package", package);
                cmd.Parameters.AddWithValue("@appl_id", applId);
                cmd.Parameters.AddWithValue("@current_page", currentPage);
                cmd.Parameters.AddWithValue("@browser", sessionInfo.Browser ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ic", sessionInfo.Ic ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@operator", sessionInfo.UserId);

                try
                {
                    await conn.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var item = new eGrantsSearchResults
                            {
                                tag = reader["tag"] as int?,
                                parent = reader["parent"] as int?,
                                grant_id = reader["grant_id"] as int?,
                                label = reader["label"] as string,
                                serial_num = reader["serial_num"] as string,
                                admin_phs_org_code = reader["admin_phs_org_code"] as string,
                                former_grant_num = reader["former_grant_num"] as string,
                                latest_full_grant_num = reader["latest_full_grant_num"] as string,
                                all_activity_code = reader["all_activity_code"] as string,
                                project_title = reader["project_title"] as string,
                                org_id = reader["org_id"] as int?,
                                org_name = reader["org_name"] as string,
                                pi_name = reader["pi_name"] as string,
                                current_pi_name = reader["current_pi_name"] as string,
                                current_pi_email_address = reader["current_pi_email_address"] as string,
                                current_pd_name = reader["current_pd_name"] as string,
                                current_pd_email_address = reader["current_pd_email_address"] as string,
                                current_spec_name = reader["current_spec_name"] as string,
                                current_spec_email_address = reader["current_spec_email_address"] as string,
                                current_bo_email_address = reader["current_bo_email_address"] as string,
                                prog_class_code = reader["prog_class_code"] as string,
                                sv_url = reader["sv_url"] as string,
                                arra_flag = reader["arra_flag"] as string,
                                fda_flag = reader["fda_flag"] as string,
                                stop_flag = reader["stop_flag"] as string,
                                ms_flag = reader["ms_flag"] as string,
                                od_flag = reader["od_flag"] as string,
                                ds_flag = reader["ds_flag"] as string,
                                adm_supp = reader["adm_supp"] as int?,
                                institutional_flag1 = reader["institutional_flag1"] as int?,
                                institutional_flag2 = reader["institutional_flag2"] as int?,
                                inst_flag1_url = reader["inst_flag1_url"] as string,
                                appl_id = reader["appl_id"] as int?,
                                full_grant_num = reader["full_grant_num"] as string,
                                appl_type_code = reader["appl_type_code"] as string,
                                deleted_by_impac = reader["deleted_by_impac"] as string,
                                doc_count = reader["doc_count"] as int?,
                                closeout_notcount = reader["closeout_notcount"] as int?,
                                competing = reader["competing"] as string,
                                fsr_count = reader["fsr_count"] as int?,
                                frc_destroyed = reader["frc_destroyed"] as int?,
                                appl_fda_flag = reader["appl_fda_flag"] as string,
                                appl_ms_flag = reader["appl_ms_flag"] as string,
                                appl_od_flag = reader["appl_od_flag"] as string,
                                appl_ds_flag = reader["appl_ds_flag"] as string,
                                closeout_flag = reader["closeout_flag"] as string,
                                irppr_id = reader["irppr_id"] as int?,
                                can_add_doc = reader["can_add_doc"] as string,
                                can_add_funding = reader["can_add_funding"] as string,
                                docs_count = reader["docs_count"] as int?,
                                is_current_pi = reader["is_current_pi"] as int?,
                                specific_year_pi_name = reader["specific_year_pi_name"] as string,
                                specific_year_pi_email_address = reader["specific_year_pi_email_address"] as string,
                                specific_year_project_name = reader["specific_year_project_name"] as string,
                                specific_year_org_name = reader["specific_year_org_name"] as string,
                                specific_year_full_grant_num = reader["specific_year_full_grant_num"] as string,
                                specific_year_institution1 = reader["specific_year_institution1"] as int?,
                                specific_year_institution2 = reader["specific_year_institution2"] as int?,
                                support_year = reader["support_year"] as string
                            };

                            results.Add(item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    return new List<eGrantsSearchResults>();
                }
            }

            return results;
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

        public async Task<string> GetCategoryNameById(string categories = "")
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
                // Format application IDs for use in the OPENQUERY SQL string so that not more than 8000 characters worth of Ids are used.
                var sb = new StringBuilder();
                int maxLength = 8000;
                bool first = true;

                foreach (var id in applIds)
                {
                    var formatted = $"''{id}''";
                    if (sb.Length + formatted.Length + (first ? 0 : 1) > maxLength)
                        break;

                    if (!first)
                        sb.Append(",");
                    sb.Append(formatted);
                    first = false;
                }

                var applsParam = sb.ToString();

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
                    context.Database.SetCommandTimeout(60);

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
                    .FirstOrDefaultAsync();

            }
        }

        public async Task<List<string>> GetCategoryList(int grantId, string years)
        {
            var categoryList = new List<string>();

            using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("dbo.sp_web_egrants_load_category_list", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@grant_id", grantId);
                    command.Parameters.AddWithValue("@years", years);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            // Force cast to int regardless of underlying SQL type
                            int categoryId = Convert.ToInt32(reader["category_id"]);
                            string categoryName = reader["category_name"].ToString();

                            categoryList.Add($"{categoryId}:{categoryName}");
                        }
                    }
                }
            }

            return categoryList;
        }

        public async Task<List<VwApplDTO>> LoadApplsByApplid(int? applId)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var GrantYearList = await context.VwAppls
                .Where(vw => vw.grant_id == context.VwAppls
                    .Where(a => a.appl_id == applId)
                    .Select(a => a.grant_id)
                    .FirstOrDefault())
                    .OrderByDescending(vw => vw.support_year)
                    .Select(vw => new VwApplDTO
                    {
                        appl_id = vw.appl_id,
                        support_year = Convert.ToInt32(vw.support_year),
                        full_grant_num = vw.full_grant_num ?? ""
                    })
                    .ToListAsync();
                return GrantYearList;
            }

        }

        public async Task<List<string>> LoadDataAutocomplete(string sqlQuery, string term, string mechanism, string fy, string adminCode, string serialNum)
        {
            var dataList = new List<string>();

            await using var connection = new SqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync().ConfigureAwait(false);

            await using var command = new SqlCommand(sqlQuery, connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 60
            };

            command.Parameters.Add("@term", SqlDbType.VarChar).Value = term;
            command.Parameters.Add("@fy", SqlDbType.VarChar).Value = fy;
            command.Parameters.Add("@mechanism", SqlDbType.VarChar).Value = mechanism;
            command.Parameters.Add("@admincode", SqlDbType.VarChar).Value = adminCode;
            command.Parameters.Add("@serialnum", SqlDbType.VarChar).Value = serialNum;

            await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                dataList.Add(reader[0].ToString());
            }

            return dataList;
        }
    }
}
