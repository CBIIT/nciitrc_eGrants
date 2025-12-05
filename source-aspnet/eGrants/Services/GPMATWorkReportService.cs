using System.Data;

using eGrants.DAL;
using eGrants.Models;
using eGrants.Services.Interfaces;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using Serilog;

namespace eGrants.Services
{
    public class GPMATWorkReportService : IGPMATWorkReportService
    {
        private readonly ISessionInfoService _sessionInfoService;
        private readonly AppDbContext _context;

        public GPMATWorkReportService(ISessionInfoService sessionInfoService, AppDbContext context)
        {
            _sessionInfoService = sessionInfoService;
            _context = context;
        }

        /// <summary>
        /// The load reports.
        /// </summary>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<PMATWorkReports> LoadReports(string ic, string userid)
        {
            try
            {
                using (var conn = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    var cmd = new SqlCommand("sp_web_admin_gpmat_workload", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
                    cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;
                    conn.Open();

                    var Reports = new List<PMATWorkReports>();
                    var rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                        Reports.Add(
                            new PMATWorkReports
                            {
                                specialist_name = rdr["specialist_name"]?.ToString(),
                                specialist_code = rdr["specialist_code"]?.ToString(),
                                branch = rdr["branch"]?.ToString(),
                                team = rdr["team"]?.ToString(),
                                fy = rdr["fy"]?.ToString(),
                                OCT_CNT = rdr["OCT_CNT"]?.ToString(),
                                OCT_REL = rdr["OCT_REL"]?.ToString(),
                                OCT_WRKLD = rdr["OCT_WRKLD"]?.ToString(),
                                NOV_CNT = rdr["NOV_CNT"]?.ToString(),
                                NOV_REL = rdr["NOV_REL"]?.ToString(),
                                NOV_WRKLD = rdr["NOV_WRKLD"]?.ToString(),
                                DEC_CNT = rdr["DEC_CNT"]?.ToString(),
                                DEC_REL = rdr["DEC_REL"]?.ToString(),
                                DEC_WRKLD = rdr["DEC_WRKLD"]?.ToString(),
                                JAN_CNT = rdr["JAN_CNT"]?.ToString(),
                                JAN_REL = rdr["JAN_REL"]?.ToString(),
                                JAN_WRKLD = rdr["JAN_WRKLD"]?.ToString(),
                                FEB_CNT = rdr["FEB_CNT"]?.ToString(),
                                FEB_REL = rdr["FEB_REL"]?.ToString(),
                                FEB_WRKLD = rdr["FEB_WRKLD"]?.ToString(),
                                MAR_CNT = rdr["MAR_CNT"]?.ToString(),
                                MAR_REL = rdr["MAR_REL"]?.ToString(),
                                MAR_WRKLD = rdr["MAR_WRKLD"]?.ToString(),
                                APR_CNT = rdr["APR_CNT"]?.ToString(),
                                APR_REL = rdr["APR_REL"]?.ToString(),
                                APR_WRKLD = rdr["APR_WRKLD"]?.ToString(),
                                MAY_CNT = rdr["MAY_CNT"]?.ToString(),
                                MAY_REL = rdr["MAY_REL"]?.ToString(),
                                MAY_WRKLD = rdr["MAY_WRKLD"]?.ToString(),
                                JUN_CNT = rdr["JUN_CNT"]?.ToString(),
                                JUN_REL = rdr["JUN_REL"]?.ToString(),
                                JUN_WRKLD = rdr["JUN_WRKLD"]?.ToString(),
                                JUL_CNT = rdr["JUL_CNT"]?.ToString(),
                                JUL_REL = rdr["JUL_REL"]?.ToString(),
                                JUL_WRKLD = rdr["JUL_WRKLD"]?.ToString(),
                                AUG_CNT = rdr["AUG_CNT"]?.ToString(),
                                AUG_REL = rdr["AUG_REL"]?.ToString(),
                                AUG_WRKLD = rdr["AUG_WRKLD"]?.ToString(),
                                SEP_CNT = rdr["SEP_CNT"]?.ToString(),
                                SEP_REL = rdr["SEP_REL"]?.ToString(),
                                SEP_WRKLD = rdr["SEP_WRKLD"]?.ToString(),
                                TOTAL_CNT = rdr["TOTAL_CNT"]?.ToString(),
                                TOTAL_REL = rdr["TOTAL_REL"]?.ToString(),
                                TOTAL_WRKLD = rdr["TOTL_WRKLD"]?.ToString()
                            });

                    rdr.Close();
                    conn.Close();

                    return Reports;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "LoadReports: Error occurred while loading GPMAT workload reports - IC: {IC}, UserId: {UserId}", ic, userid);
                throw;
            }
        }
    }
}