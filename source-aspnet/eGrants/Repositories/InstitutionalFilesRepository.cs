using System.Data;
using System.Security.Cryptography;

using eGrants.DAL;
using eGrants.DTOs;
using eGrants.Models;
using eGrants.Repositories.Interfaces;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using static NPOI.HSSF.Util.HSSFColor;

namespace eGrants.Repositories
{
    public class InstitutionalFilesRepository : IInstitutionalFilesRepository
    {
        private readonly AppDbContext _context;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        // Constructor injects the application's database context
        public InstitutionalFilesRepository(AppDbContext context, IServiceScopeFactory serviceScopeFactory)
        {
            _context = context;
            _serviceScopeFactory = serviceScopeFactory;
        }
        public async Task<InstitutionalOrg> FindOrg(int orgId, string orgName = "")
        {
            await using var conn = new SqlConnection(_context.Database.GetConnectionString());
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("sp_web_egrants_institutional_file_find_org", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@org_id", SqlDbType.Int).Value = orgId;

            // sanitize orgName to avoid SQL injection issues
            var sanitizedOrgName = orgName.Replace("'", "''");
            cmd.Parameters.Add("@org_name", SqlDbType.VarChar).Value = sanitizedOrgName;

            InstitutionalOrg org = null;

            await using var rdr = await cmd.ExecuteReaderAsync();

            while (await rdr.ReadAsync())
            {
                org = new InstitutionalOrg
                {
                    OrgId = rdr.GetInt32(rdr.GetOrdinal("org_id")),
                    OrgName = rdr["org_name"] as string,
                    SVCreatedBy = rdr["created_by"] as string,
                    SVCreatedDate = rdr["created_date"] as string,
                    SVEndDate = rdr["end_date"] as string,
                    SvUrl = rdr["sv_url"] as string
                };
            }

            return org;
        }


        public async Task<List<InsitutionalOrgNameIndex>> LoadOrgNameCharacterIndices()
        {
            var results = new List<InsitutionalOrgNameIndex>();

            await using var conn = new SqlConnection(_context.Database.GetConnectionString());
            await conn.OpenAsync();

            await using var cmd = new SqlCommand(
                "SELECT index_id, character_index, index_seq FROM dbo.character_index ORDER BY index_seq", conn)
            {
                CommandType = CommandType.Text
            };

            await using var rdr = await cmd.ExecuteReaderAsync();

            while (await rdr.ReadAsync())
            {
                results.Add(new InsitutionalOrgNameIndex
                {
                    IndexId = rdr.GetInt32(rdr.GetOrdinal("index_id")),
                    CharacterIndex = rdr["character_index"] as string,
                    IndexSeq = rdr.GetInt32(rdr.GetOrdinal("index_seq"))
                });
            }

            return results;
        }
        public async Task<List<InstitutionalDocFiles>> LoadOrgDocList(int orgId)
        {
            var results = new List<InstitutionalDocFiles>();

            await using var conn = new SqlConnection(_context.Database.GetConnectionString());
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("sp_web_egrants_inst_files_show_docs", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@org_id", SqlDbType.Int).Value = orgId;

            await using var rdr = await cmd.ExecuteReaderAsync();

            while (await rdr.ReadAsync())
            {
                results.Add(new InstitutionalDocFiles
                {
                    org_id = rdr["org_id"]?.ToString(),
                    org_name = rdr["org_name"]?.ToString(),
                    DocumentId = rdr.GetInt32(rdr.GetOrdinal("document_id")),
                    category_name = rdr["category_name"] as string,
                    url = rdr["url"] as string,
                    start_date = rdr["start_date"] as string,
                    end_date = rdr["end_date"] as string,
                    created_date = rdr["created_date"] as string,
                    comments = rdr["comments"] as string
                });
            }

            return results;
        }


        public async Task<List<InstitutionalOrg>> LoadOrgList(int indexId)
        {
            var orgList = new List<InstitutionalOrg>();

            await using var conn = new SqlConnection(_context.Database.GetConnectionString());
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("sp_web_egrants_inst_files_show_orgs", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@index_id", SqlDbType.Int).Value = indexId;

            await using var rdr = await cmd.ExecuteReaderAsync();

            while (await rdr.ReadAsync())
            {
                orgList.Add(new InstitutionalOrg
                {
                    OrgId = rdr.GetInt32(rdr.GetOrdinal("org_id")),
                    OrgName = rdr["org_name"] as string,
                    SVCreatedBy = rdr["svcreated_by"] as string,
                    SVCreatedDate = rdr["svcreated_date"] as string,
                    SVEndDate = rdr["svend_date"] as string,
                    SvUrl = rdr["sv_url"] as string,
                    FUCreatedBy = rdr["fucreated_by"] as string,
                    FUCreatedDate = rdr["fucreated_date"] as string,
                    FUEndDate = rdr["fuend_date"] as string,
                    FUUrl = rdr["fu_url"] as string,
                    AnyOrgDoc = rdr.GetBoolean(rdr.GetOrdinal("anyorgdoc"))
                });
            }

            return orgList;
        }

        public async Task<List<InstitutionalOrgCategory>> LoadOrgCategory(bool activeOnly)
        {
            var results = new List<InstitutionalOrgCategory>();

            await using var conn = new SqlConnection(_context.Database.GetConnectionString());
            await conn.OpenAsync();

            var where = activeOnly ? "WHERE active = 1" : string.Empty;

            await using var cmd = new SqlCommand(
                $@"SELECT 
              doctype_id AS category_id, 
              doctype_name AS category_name, 
              tobe_flagged AS tobe_flag, 
              Flag_period, 
              ISNULL(comments_required,0) AS comments_required, 
              active 
          FROM dbo.Org_Categories {where} 
          ORDER BY category_name", conn)
            {
                CommandType = CommandType.Text
            };

            await using var rdr = await cmd.ExecuteReaderAsync();

            while (await rdr.ReadAsync())
            {
                results.Add(new InstitutionalOrgCategory
                {
                    category_id = rdr["category_id"]?.ToString(),
                    category_name = rdr["category_name"]?.ToString(),
                    tobe_flag = rdr["tobe_flag"]?.ToString(),
                    flag_period = rdr["Flag_period"]?.ToString(),
                    flag_data = $"{rdr["tobe_flag"]}_{rdr["Flag_period"]}",
                    require_comments = rdr.GetBoolean(rdr.GetOrdinal("comments_required")),
                    active = rdr.GetBoolean(rdr.GetOrdinal("active"))
                });
            }

            return results;
        }

        public async Task<string> UpdateDocument(
            int docId,
            int categoryId,
            string startDate,
            string endDate,
            string ic,
            string userId,
            string comments)
        {
            await using var conn = new SqlConnection(_context.Database.GetConnectionString());
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("sp_web_egrants_institutional_file_update", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@category_id", SqlDbType.Int).Value = categoryId;
            cmd.Parameters.Add("@start_date", SqlDbType.VarChar).Value = startDate;
            cmd.Parameters.Add("@end_date", SqlDbType.VarChar).Value = endDate;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userId;
            cmd.Parameters.Add("@document_id", SqlDbType.Int).Value = docId;
            cmd.Parameters.Add("@comments", SqlDbType.VarChar).Value = comments;

            await cmd.ExecuteNonQueryAsync();

            // Retrieve the updated document_id parameter value
            var documentId = Convert.ToString(cmd.Parameters["@document_id"].Value);

            return documentId;
        }

        public async Task DisableDoc(int docId, string userId)
        {
            await using var conn = new SqlConnection(_context.Database.GetConnectionString());
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("sp_web_egrants_inst_files_disable_doc", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = docId;
            cmd.Parameters.Add("@user_id", SqlDbType.VarChar).Value = userId;

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<string> GetDocID(
            int orgId,
            int categoryId,
            string fileType,
            string startDate,
            string endDate,
            string ic,
            string userId,
            string comments)
        {
            await using var conn = new SqlConnection(_context.Database.GetConnectionString());
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("sp_web_egrants_institutional_file_create", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@org_id", SqlDbType.Int).Value = orgId;
            cmd.Parameters.Add("@category_id", SqlDbType.Int).Value = categoryId;
            cmd.Parameters.Add("@file_type", SqlDbType.VarChar).Value = fileType;
            cmd.Parameters.Add("@start_date", SqlDbType.VarChar).Value = startDate;
            cmd.Parameters.Add("@end_date", SqlDbType.VarChar).Value = endDate;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userId;
            cmd.Parameters.Add("@comments", SqlDbType.VarChar).Value = comments;

            // Output parameter
            var outputParam = cmd.Parameters.Add("@document_id", SqlDbType.VarChar, 100);
            outputParam.Direction = ParameterDirection.Output;

            // Execute the stored procedure asynchronously
            await cmd.ExecuteNonQueryAsync();

            // Retrieve the output parameter value
            var documentId = Convert.ToString(outputParam.Value);

            return documentId;
        }

        public async Task<List<InstitutionalOrg>> SearchOrgList(string searchStr)
        {
            var results = new List<InstitutionalOrg>();

            await using var conn = new SqlConnection(_context.Database.GetConnectionString());
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("sp_web_egrants_inst_files_search_orgs", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@str", SqlDbType.VarChar).Value = searchStr;

            await using var rdr = await cmd.ExecuteReaderAsync();

            while (await rdr.ReadAsync())
            {
                results.Add(new InstitutionalOrg
                {
                    OrgId = rdr.GetInt32(rdr.GetOrdinal("org_id")),
                    OrgName = rdr["org_name"] as string,
                    SVCreatedBy = rdr["svcreated_by"] as string,
                    SVCreatedDate = rdr["svcreated_date"] as string,
                    SVEndDate = rdr["svend_date"] as string,
                    SvUrl = rdr["sv_url"] as string,
                    FUCreatedBy = rdr["fucreated_by"] as string,
                    FUCreatedDate = rdr["fucreated_date"] as string,
                    FUEndDate = rdr["fuend_date"] as string,
                    FUUrl = rdr["fu_url"] as string,
                    AnyOrgDoc = rdr.GetBoolean(rdr.GetOrdinal("anyorgdoc"))
                });
            }

            return results;
        }
    }
}
