using System.Data;

using eGrants.DAL;
using eGrants.Models;
using eGrants.Services.Interfaces;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using Serilog;

namespace eGrants.Services
{
    public class ApplDestructedService : IApplDestructedService
    {
        private readonly ISessionInfoService _sessionInfoService;
        private readonly AppDbContext _context;

        public ApplDestructedService(ISessionInfoService sessionInfoService, AppDbContext context)
        {
            _sessionInfoService = sessionInfoService;
            _context = context;
        }

        public List<DestructionYears> LoadYears()
        {
            try
            {
                using (var conn = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    var cmd = new SqlCommand(
                    "SELECT distinct year(EGRANTS_CREATED_DATE) as [year] FROM dbo.IMPAC_DESTRUCTED_APPL order by [year] desc",
                    conn);

                    conn.Open();

                    var years = new List<DestructionYears>();
                    var rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                        years.Add(new DestructionYears { year = rdr["year"]?.ToString() });

                    rdr.Close();
                    conn.Close();

                    return years;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "LoadYears: Error occurred while loading destruction years");
                throw;
            }
        }

        public List<DescripCodes> LoadDescripCodes()
        {
            try
            {
                using (var conn = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    var cmd = new SqlCommand(
                        "SELECT distinct APPL_STATUS_GRP_DESCRIP as descrip_code FROM dbo.IMPAC_DESTRUCTED_APPL "
                      + "WHERE APPL_STATUS_GRP_DESCRIP is not null ORDER BY APPL_STATUS_GRP_DESCRIP",
                        conn);

                    conn.Open();

                    var codes = new List<DescripCodes>();
                    var rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                        codes.Add(new DescripCodes { descrip_code = rdr["descrip_code"]?.ToString() });

                    rdr.Close();
                    conn.Close();

                    return codes;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "LoadDescripCodes: Error occurred while loading description codes");
                throw;
            }
        }

        public List<ExceptionCodes> LoadExceptionCodes()
        {
            try
            {
                using (var conn = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    var cmd = new SqlCommand(
                                    "SELECT id, code as exception_code, detail, convert(varchar,created_date,101) as created_date, dbo.fn_get_person_name(created_by_person_id) as created_by "
                                  + " FROM dbo.IMPAC_DESTRUCT_OGA_EXCEPTION WHERE disable_date is null ORDER BY exception_code",
                                    conn);

                    conn.Open();

                    var codes = new List<ExceptionCodes>();
                    var rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                        codes.Add(
                            new ExceptionCodes
                            {
                                id = rdr["id"]?.ToString(),
                                exception_code = rdr["exception_code"]?.ToString(),
                                detail = rdr["detail"]?.ToString(),
                                created_date = rdr["created_date"]?.ToString(),
                                created_by = rdr["created_by"]?.ToString()
                            });

                    rdr.Close();
                    conn.Close();

                    return codes;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "LoadExceptionCodes: Error occurred while loading exception codes");
                throw;
            }
        }

        public List<DestructedsAppls> LoadAppls(
            string act,
            int year,
            string status_code,
            string exception_code,
            string str,
            string id_string,
            string exception_type,
            string ic,
            string userid)
        {
            try
            {
                using (var conn = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    var cmd = new SqlCommand("sp_web_admin_appl_destructed", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
                    cmd.Parameters.Add("@year", SqlDbType.Int).Value = year;
                    cmd.Parameters.Add("@status_code", SqlDbType.VarChar).Value = status_code;
                    cmd.Parameters.Add("@exception_code", SqlDbType.VarChar).Value = exception_code;
                    cmd.Parameters.Add("@str", SqlDbType.VarChar).Value = str;
                    cmd.Parameters.Add("@id_string", SqlDbType.VarChar).Value = id_string;
                    cmd.Parameters.Add("@exception_type", SqlDbType.VarChar).Value = exception_type;
                    cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
                    cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;
                    conn.Open();

                    var Appls = new List<DestructedsAppls>();
                    var rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                        Appls.Add(
                            new DestructedsAppls
                            {
                                appl_id = rdr["appl_id"]?.ToString(),
                                full_grant_num = rdr["full_grant_num"]?.ToString(),
                                serial_num = rdr["serial_num"]?.ToString(),
                                exception_code = rdr["exception_code"]?.ToString(),
                                status_code = rdr["status_code"]?.ToString(),
                                step_code = rdr["step_code"]?.ToString(),
                                appl_editable = rdr["appl_editable"]?.ToString()
                            });

                    rdr.Close();
                    conn.Close();

                    return Appls;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex,
                    "LoadAppls: Error occurred while loading applications - Act: {Act}, Year: {Year}, StatusCode: {StatusCode}, ExceptionCode: {ExceptionCode}, UserId: {UserId}",
                    act, year, status_code, exception_code, userid);
                throw;
            }
        }

        public List<SearchInfo> LoadSearchInfo(int year, string status_code, string exception_code, string str)
        {
            try
            {
                using (var conn = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    var cmd = new SqlCommand("sp_web_admin_appl_destructed_index", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@year", SqlDbType.Int).Value = year;
                    cmd.Parameters.Add("@status_code", SqlDbType.VarChar).Value = status_code;
                    cmd.Parameters.Add("@exception_code", SqlDbType.VarChar).Value = exception_code;
                    cmd.Parameters.Add("@str", SqlDbType.VarChar).Value = str;

                    conn.Open();

                    var SearchInfo = new List<SearchInfo>();
                    var rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                        SearchInfo.Add(
                            new SearchInfo
                            {
                                total_appls = rdr["total_appls"]?.ToString(),
                                total_pages = rdr["total_pages"]?.ToString(),
                                per_page = rdr["per_page"]?.ToString()
                            });

                    rdr.Close();
                    conn.Close();

                    return SearchInfo;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex,
                    "LoadSearchInfo: Error occurred while loading search info - Year: {Year}, StatusCode: {StatusCode}, ExceptionCode: {ExceptionCode}",
                    year, status_code, exception_code);
                throw;
            }
        }

        /// <summary>
        /// The check permission.
        /// </summary>
        /// <param name="year">
        /// The year.
        /// </param>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string CheckPermission(int year, string userid)
        {
            try
            {
                using (var conn = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    var cmd = new SqlCommand(
                        "select dbo.fn_is_Archival_admin(@year,(select person_id from people where userid=@userid)) as permission",
                        conn);

                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@year", SqlDbType.Int).Value = year;
                    cmd.Parameters.Add("@userid", SqlDbType.VarChar).Value = userid;
                    conn.Open();
                    var Processable = (string)cmd.ExecuteScalar();
                    conn.Close();

                    return Processable;
                }
            }
            catch (SqlException ex)
            {
                Log.Error(ex,
                    "CheckPermission: Error occurred while checking permission - Year: {Year}, UserId: {UserId}",
                    year, userid);
                return "Error - Cannot open year";
            }
        }

        public void EditExceptionCode(string act, int id, string detail, string code, string ic, string userid)
        {
            try
            {
                using (var conn = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    var cmd = new SqlCommand("sp_web_admin_appl_destructed_edit", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    cmd.Parameters.Add("@detail", SqlDbType.VarChar).Value = detail ?? "";
                    cmd.Parameters.Add("@code", SqlDbType.VarChar).Value = code ?? "";
                    cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
                    cmd.Parameters.Add("@Operator", SqlDbType.VarChar).Value = userid;
                    conn.Open();
                    var rdr = cmd.ExecuteReader();
                    rdr.Close();
                    conn.Close();
                }
            }
            catch (SqlException ex)
            {
                Log.Error(ex,
                    "EditExceptionCode: Error occurred while editing exception code - Act: {Act}, Id: {Id}, Code: {Code}, UserId: {UserId}",
                    act, id, code, userid);
                throw;
            }
        }
    }
}