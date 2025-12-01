using System.Data;
using System.Runtime.CompilerServices;

using eGrants.DAL;
using eGrants.Models;
using eGrants.Services.Interfaces;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using Serilog;

namespace eGrants.Services
{
    public class ManagementService : IManagementService
    {
        private readonly AppDbContext _context;

        public ManagementService(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<QCReasons>> LoadQCReasons(string ic)
        {
            var qcReasons = new List<QCReasons>();

            try
            {
                await using var conn = new SqlConnection(_context.Database.GetConnectionString());
                await conn.OpenAsync();

                await using var cmd = new SqlCommand(
                    "SELECT DISTINCT qc_reason FROM vw_quality_control WHERE profile = @ic ORDER BY qc_reason", conn);
                cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;

                await using var rdr = await cmd.ExecuteReaderAsync();
                while (await rdr.ReadAsync())
                {
                    qcReasons.Add(new QCReasons { qc_reason = rdr["qc_reason"].ToString() });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading QCReasons for IC: {IC}", ic);
            }

            return qcReasons;
        }

        public async Task<List<EgrantsUsers>> LoadSpecialists(string ic)
        {
            var list = new List<EgrantsUsers>();

            try
            {
                await using var conn = new SqlConnection(_context.Database.GetConnectionString());
                await conn.OpenAsync();

                await using var cmd = new SqlCommand(
                    @"SELECT person_name, person_id 
                      FROM vw_people 
                      WHERE ic = @IC 
                        AND application_type = 'egrants' 
                        AND position_id > 1 
                        AND PATINDEX('%,%', person_name) > 0 
                      ORDER BY person_name", conn);

                cmd.Parameters.Add("@IC", SqlDbType.VarChar).Value = ic;

                await using var rdr = await cmd.ExecuteReaderAsync();
                while (await rdr.ReadAsync())
                {
                    list.Add(new EgrantsUsers
                    {
                        PersonId = rdr["person_id"].ToString(),
                        person_name = rdr["person_name"].ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading specialists for IC: {IC}", ic);
            }

            return list;
        }

        public async Task<List<QCPersons>> LoadQCPersons(string ic)
        {
            var qcPersons = new List<QCPersons>();

            try
            {
                await using var conn = new SqlConnection(_context.Database.GetConnectionString());
                await conn.OpenAsync();

                await using var cmd = new SqlCommand(
                    "SELECT qc_reason, userid, person_id, person_name FROM vw_quality_control WHERE profile=@ic ORDER BY qc_reason",
                    conn);

                cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;

                await using var rdr = await cmd.ExecuteReaderAsync();
                while (await rdr.ReadAsync())
                {
                    qcPersons.Add(new QCPersons
                    {
                        qc_reason = rdr["qc_reason"].ToString(),
                        userid = rdr["userid"].ToString(),
                        person_id = rdr["person_id"].ToString(),
                        person_name = rdr["person_name"].ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading QCPersons for IC: {IC}", ic);
            }

            return qcPersons;
        }

        public async Task<List<QCReports>> LoadQCReport(string ic)
        {
            var qcReports = new List<QCReports>();

            try
            {
                await using var conn = new SqlConnection(_context.Database.GetConnectionString());
                await conn.OpenAsync();

                await using var cmd = new SqlCommand(
                    @"WITH qc AS (
                        SELECT COUNT(*) AS files_to_qc, 
                               AVG(DATEDIFF(D, qc_date, GETDATE())) AS qc_days,
                               qc_person_id 
                        FROM egrants 
                        WHERE qc_date IS NOT NULL
                          AND qc_person_id IS NOT NULL
                          AND qc_reason IS NOT NULL
                          AND disabled_date IS NULL
                          AND ic = @ic
                          AND parent_id IS NULL
                          AND grant_id IS NOT NULL
                        GROUP BY qc_person_id
                    )
                    SELECT qc.files_to_qc, qc.qc_days,
                           qc.qc_person_id, 
                           COALESCE(vp.person_name, CAST(qc.qc_person_id AS VARCHAR(10))) AS qc_person_name
                    FROM qc 
                    INNER JOIN vw_people vp ON qc.qc_person_id = vp.person_id", conn);

                cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;

                await using var rdr = await cmd.ExecuteReaderAsync();
                while (await rdr.ReadAsync())
                {
                    qcReports.Add(new QCReports
                    {
                        files_to_qc = rdr["files_to_qc"].ToString(),
                        qc_person_id = rdr["qc_person_id"].ToString(),
                        qc_person_name = rdr["qc_person_name"].ToString(),
                        qc_days = rdr["qc_days"].ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading QCReport for IC: {IC}", ic);
            }

            return qcReports;
        }

        public async Task run_db(string act, int qcPersonId, string qcReason, int percent, int personId, string ic, string userId)
        {
            try
            {
                await using var conn = new SqlConnection(_context.Database.GetConnectionString());
                await conn.OpenAsync();

                await using var cmd = new SqlCommand("sp_web_management_qc_assign", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
                cmd.Parameters.Add("@person_id", SqlDbType.Int).Value = personId;
                cmd.Parameters.Add("@qc_person_id", SqlDbType.Int).Value = qcPersonId;
                cmd.Parameters.Add("@qc_reason", SqlDbType.VarChar).Value = qcReason;
                cmd.Parameters.Add("@percent", SqlDbType.Int).Value = percent;
                cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
                cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userId;

                await using var rdr = await cmd.ExecuteReaderAsync();
                // No need to process results, just ensure execution
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error executing RunDbAsync with act: {Act}, qcPersonId: {QcPersonId}, ic: {IC}, userId: {UserId}", act, qcPersonId, ic, userId);
                throw; // rethrow if you want upstream handling
            }
        }

        public async Task<List<DocTransactionHistory>> LoadDocTransactionHistory(
            string transactionType,
            int personId,
            string startDate,
            string endDate,
            string dateRange,
            string ic,
            string userId)
        {
            var results = new List<DocTransactionHistory>();

            try
            {
                await using var conn = new SqlConnection(_context.Database.GetConnectionString());
                await using var cmd = new SqlCommand("sp_web_management_doc_transaction_report", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@transaction_type", transactionType ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@startdate", startDate ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@enddate", endDate ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@date_range", dateRange ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@person_id", personId);
                cmd.Parameters.AddWithValue("@ic", ic ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@operator", userId ?? (object)DBNull.Value);

                await conn.OpenAsync();

                await using var rdr = await cmd.ExecuteReaderAsync();
                while (await rdr.ReadAsync())
                {
                    results.Add(new DocTransactionHistory
                    {
                        transaction_type = rdr["transaction_type"]?.ToString(),
                        document_id = rdr["document_id"]?.ToString(),
                        full_grant_num = rdr["full_grant_num"]?.ToString(),
                        category_name = rdr["category_name"]?.ToString(),
                        person_name = rdr["person_name"]?.ToString(),
                        url = rdr["url"]?.ToString(),
                        transaction_date = rdr["transaction_date"]?.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading document transaction history for user {UserId}", userId);
            }

            return results;
        }

        public async Task<List<EgrantAccessions>> LoadAccessions(string ic)
        {
            var accessions = new List<EgrantAccessions>();

            try
            {
                await using var conn = new SqlConnection(_context.Database.GetConnectionString());
                await conn.OpenAsync();

                const string query = @"
                    SELECT accession_id, accession_number
                    FROM eim.dbo.accessions
                    WHERE contract = 0 
                      AND profile_id = (SELECT profile_id FROM profiles WHERE profile = @ic)
                    ORDER BY accession_id DESC";

                await using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;

                await using var rdr = await cmd.ExecuteReaderAsync();
                while (await rdr.ReadAsync())
                {
                    accessions.Add(new EgrantAccessions
                    {
                        accession_id = rdr["accession_id"].ToString(),
                        accession_number = rdr["accession_number"].ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading accessions for IC: {IC}", ic);
            }

            return accessions;
        }

        public async Task<List<EgrantFolders>> LoadFolders(string act, int searchNumber, string ic, string userId)
        {
            var egrantFolders = new List<EgrantFolders>();

            try
            {
                await using var conn = new SqlConnection(_context.Database.GetConnectionString());
                await using var cmd = new SqlCommand("sp_web_management_system_report", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
                cmd.Parameters.Add("@search_number", SqlDbType.Int).Value = searchNumber;
                cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
                cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userId;

                await conn.OpenAsync();

                await using var rdr = await cmd.ExecuteReaderAsync();
                while (await rdr.ReadAsync())
                {
                    egrantFolders.Add(new EgrantFolders
                    {
                        folder_id = rdr["folder_id"] as string,
                        bar_code = rdr["bar_code"] as string,
                        grant_num = rdr["grant_num"] as string,
                        former_grant_num = rdr["former_grant_num"] as string,
                        id_string = rdr["id_string"] as string,
                        latest_move_date = rdr["latest_move_date"] as string,
                        current_status = rdr["current_status"] as string,
                        closed_out = rdr["closed_out"] as string,
                        accession_destroyed_date = rdr["accession_destroyed_date"] as string
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading folders for Act: {Act}, SearchNumber: {SearchNumber}, IC: {IC}, UserId: {UserId}",
                    act, searchNumber, ic, userId);
            }

            return egrantFolders;
        }
    }
}
