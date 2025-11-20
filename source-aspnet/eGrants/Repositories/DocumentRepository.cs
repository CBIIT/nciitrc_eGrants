using System.Data;

using eGrants.DAL;
using eGrants.DTOs;
using eGrants.Models;
using eGrants.Repositories.Interfaces;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using static System.Formats.Asn1.AsnWriter;

namespace eGrants.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly AppDbContext _context;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        // Constructor injects the application's database context
        public DocumentRepository(AppDbContext context, IServiceScopeFactory serviceScopeFactory)
        {
            _context = context;
            _serviceScopeFactory = serviceScopeFactory;
        }

        // Execute the stored procedure 'sp_web_egrants_search_by_appl_id' with the provided parameters.
        // This retrieves document layer records filtered by application ID, search type, category list, IC, and user ID.
        // The results are materialized into a list of 'doclayer' objects.
        public List<doclayer> LoadDocs(int aApplId, string aSearchType, string aCategoryList, string aIc, string aUserId)
        {
            var result = new List<doclayer>();

            using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
            {
                connection.Open();

                using (SqlCommand cmd = new SqlCommand("dbo.sp_web_egrants_search_by_appl_id", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Add parameters
                    cmd.Parameters.AddWithValue("@appl_id", aApplId);
                    cmd.Parameters.AddWithValue("@search_type", aSearchType ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@category_list", aCategoryList ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ic", aIc ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@operator", aUserId ?? (object)DBNull.Value);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var doc = new doclayer
                            {
                                appl_id = reader["appl_id"] as int?,
                                grant_id = reader["grant_id"] as int?,
                                document_id = reader["document_id"] as int?,
                                document_name = reader["document_name"] as string,
                                category_id = reader["category_id"] as int?,
                                category_name = reader["category_name"] as string,
                                sub_category_name = reader["sub_category_name"] as string,
                                created_by = reader["created_by"] as string,
                                modified_by = reader["modified_by"] as string,
                                file_modified_by = reader["file_modified_by"] as string,
                                problem_msg = reader["problem_msg"] as string,
                                problem_reported_by = reader["problem_reported_by"] as string,
                                page_count = reader["page_count"] as int?,
                                fsr_count = reader["fsr_count"] as int?,
                                attachment_count = reader["attachment_count"] as int?,
                                frc_destroyed = reader["frc_destroyed"] as int?,
                                url = reader["url"] as string,
                                can_qc = reader["can_qc"] as string,
                                can_upload = reader["can_upload"] as string,
                                can_modify_index = reader["can_modify_index"] as string,
                                can_delete = reader["can_delete"] as string,
                                can_restore = reader["can_restore"] as string,
                                can_store = reader["can_store"] as string,
                                created_date = reader["created_date"] as string,
                                document_date = reader["document_date"] as string,
                                doc_date = reader["doc_date"] as DateTime?,
                                modified_date = reader["modified_date"] as string,
                                file_modified_date = reader["file_modified_date"] as string,
                                qc_date = reader["qc_date"] as string
                            };

                            result.Add(doc);
                        }
                    }
                }
            }
            return result;
        }

        public virtual async Task<List<former_appls>> loadFormerAppls(int grantId)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                return await context.adminSupplementsWIP
                    .Where(supp => supp.Serial_num == context.Grants
                        .Where(g => g.grant_id == grantId)
                        .Select(g => g.serial_num)
                        .FirstOrDefault())
                    .Select(supp => new former_appls
                    {
                        former_num = supp.Former_num.ToString(),
                        former_appl_id = supp.Former_appl_id.ToString()
                    })
                    .Distinct()
                    .ToListAsync();
            }
        }

        public virtual async Task<List<DocumentInformation>> GetDocInfo(int docId)
        {

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                return await context.egrants
                    .Where(e => e.document_id == docId)
                    .Select(e => new DocumentInformation
                    {
                        admin_phs_org_code = e.admin_phs_org_code ?? "",
                        serial_num = e.serial_num ,
                        appl_id = e.appl_id,
                        category_id = e.category_id,
                        sub_category_name = e.sub_category_name ?? "",
                        full_grant_num = e.full_grant_num ?? "",
                        document_id = e.document_id,
                        document_date = e.document_date,
                        document_name = e.document_name ?? ""
                    })
                    .ToListAsync();

            }
        }

        public virtual async Task<List<CategoriesListDTO>> LoadCategories(string ic)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                
                return await context.VwCategories
                       .Where(c => c.ic == ic && c.can_upload == "yes")
                       .OrderBy(c => c.category_name)
                       .Select(c => new CategoriesListDTO
                       {
                           category_id = Convert.ToInt32(c.category_id),
                           category_name = c.category_name ?? "",
                           package = c.package ?? "",
                           input_type = c.input_type ?? "",
                           input_constraint = c.input_constraint ?? "",
                       })
                       .ToListAsync();

            }
        }

        public virtual async Task<int> GetMaxCategoryId(string ic)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {   
                try
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    return await context.VwCategories
                        .Where(c => c.ic == ic)
                        .MaxAsync(c => Convert.ToInt32(c.category_id));
                } catch (Exception e)
                {
                    return 0; // case where this ic has no categories 
                }
            }    
        }

        public virtual async Task<List<SubCategories>> LoadSubCategoryList()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                return await context.CategoriesSubcatLookup
                    .Select(c => new SubCategories
                    {
                        parent_category_id = Convert.ToInt32(c.parent_category_id),
                        sub_category_name = c.sub_category_name ?? ""
                    })
                    .ToListAsync();         
            }
        }


        public void DocModify(
            string act,
            int applId,
            int categoryId,
            string subCategory,
            string docDate,
            string docidStr,
            string fileType,
            string ic,
            string userId)
        {
            using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
            {
                connection.Open();

                using (SqlCommand cmd = new SqlCommand("sp_web_egrants_doc_modify", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Add parameters
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
                    cmd.Parameters.Add("@appl_id", SqlDbType.Int).Value = applId;
                    cmd.Parameters.Add("@category_id", SqlDbType.Int).Value = categoryId;
                    cmd.Parameters.Add("@sub_category", SqlDbType.VarChar).Value = subCategory;
                    cmd.Parameters.Add("@doc_date", SqlDbType.VarChar).Value = docDate;
                    cmd.Parameters.Add("@docid_str", SqlDbType.VarChar).Value = docidStr;
                    cmd.Parameters.Add("@file_type", SqlDbType.VarChar).Value = fileType;
                    cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
                    cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userId;

                    SqlDataReader reader = cmd.ExecuteReader();
                    reader.Close();
                    connection.Close();
                }


            }
        }

        public async Task<List<DocsUnidentified>> LoadDocsUnidentified(string imageServer, string userId)
        {

            return await _context.egrants
                .Where(e => e.appl_id == null
                            && e.qc_date != null
                            && e.parent_id == null
                            && e.qc_userid == userId)
            .Select(e => new DocsUnidentified
            {
                document_id = e.document_id.ToString(),
                document_date = e.document_date.HasValue ? DateOnly.FromDateTime(e.document_date.Value) : (DateOnly?)null,
                document_name = e.document_name,
                created_by = e.created_by,
                created_date = e.created_date.HasValue ? DateOnly.FromDateTime(e.created_date.Value) : (DateOnly?)null,
                qc_date = e.qc_date.HasValue ? DateOnly.FromDateTime(e.qc_date.Value) : (DateOnly?)null,
                category_id = e.category_id.ToString(),
                url = imageServer + e.url
            })
            .ToListAsync();
        }
    }
}