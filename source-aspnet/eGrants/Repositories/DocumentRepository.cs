using System.Data;

using eGrants.DAL;
using eGrants.Models;
using eGrants.Repositories.Interfaces;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
                var list = await context.egrants
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
                return list;

            }
        }
    }
}