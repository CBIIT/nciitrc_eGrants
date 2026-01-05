using eGrants.DAL;
using eGrants.Models;
using eGrants.Services.Interfaces;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using System.Data;

using Serilog;

namespace eGrants.Services
{
    public class ApplService : IApplService
    {
        private readonly AppDbContext _context;

        public ApplService(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Load all application types
        /// </summary>
        public async Task<List<ApplType>> LoadApplTypeAsync()
        {
            var conn = new SqlConnection(_context.Database.GetConnectionString());

            var cmd = new SqlCommand(
                "SELECT DISTINCT appl_type_code FROM appls WHERE appl_id > 0",
                conn);

            cmd.CommandType = CommandType.Text;

            var applTypeList = new List<ApplType>();

            await conn.OpenAsync();
            var rdr = await cmd.ExecuteReaderAsync();

            while (await rdr.ReadAsync())
            {
                applTypeList.Add(new ApplType
                {
                    appl_type_code = rdr["appl_type_code"]?.ToString()
                });
            }

            await rdr.CloseAsync();
            await conn.CloseAsync();

            return applTypeList;
        }

        /// <summary>
        /// Load activity codes by admin code
        /// </summary>
        public async Task<List<ActivityCode>> LoadActivityCodeAsync(string adminCode)
        {
            var conn = new SqlConnection(_context.Database.GetConnectionString());

            var cmd = new SqlCommand(
                "SELECT DISTINCT activity_code FROM vw_appls " +
                "WHERE appl_id > 0 AND admin_phs_org_code = @admin_code " +
                "ORDER BY activity_code",
                conn);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@admin_code", SqlDbType.VarChar).Value = adminCode;

            await conn.OpenAsync();

            var activityCodeList = new List<ActivityCode>();
            var rdr = await cmd.ExecuteReaderAsync();

            while (await rdr.ReadAsync())
            {
                activityCodeList.Add(new ActivityCode
                {
                    activity_code = rdr["activity_code"]?.ToString()
                });
            }

            await rdr.CloseAsync();
            await conn.CloseAsync();

            return activityCodeList;
        }

        /// <summary>
        /// Load applications by admin code and serial number
        /// </summary>
        public async Task<List<Appls>> LoadApplsBySerialNumAsync(string adminCode, int serialNum)
        {
            var conn = new SqlConnection(_context.Database.GetConnectionString());

            var cmd = new SqlCommand(
                "SELECT appl_id, full_grant_num FROM vw_appls " +
                "WHERE admin_phs_org_code = @admincode AND serial_num = @serialnum " +
                "ORDER BY support_year DESC",
                conn);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@admincode", SqlDbType.VarChar).Value = adminCode;
            cmd.Parameters.Add("@serialnum", SqlDbType.Int).Value = serialNum;

            await conn.OpenAsync();

            var grantYearList = new List<Appls>();
            var rdr = await cmd.ExecuteReaderAsync();

            while (await rdr.ReadAsync())
            {
                grantYearList.Add(new Appls
                {
                    appl_id = rdr["appl_id"]?.ToString(),
                    full_grant_num = rdr["full_grant_num"]?.ToString()
                });
            }

            await rdr.CloseAsync();
            await conn.CloseAsync();

            return grantYearList;
        }

        public async Task<string> CreateNewAppl(
            string admin_code,
            int serial_num,
            int appl_type,
            string activity_code,
            int support_year,
            string suffix_code,
            string ic,
            string userid)
        {
            try
            {
                await using var conn = new SqlConnection(_context.Database.GetConnectionString());
                await using var cmd = new SqlCommand("dbo.sp_web_egrants_create_appl", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add("@admin_code", SqlDbType.VarChar).Value = admin_code;
                cmd.Parameters.Add("@serial_num", SqlDbType.Int).Value = serial_num;
                cmd.Parameters.Add("@appl_type_code", SqlDbType.Int).Value = appl_type;
                cmd.Parameters.Add("@activity_code", SqlDbType.VarChar).Value = activity_code;
                cmd.Parameters.Add("@support_year", SqlDbType.Int).Value = support_year;
                cmd.Parameters.Add("@suffix_code", SqlDbType.VarChar).Value = suffix_code;
                cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
                cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;

                var output = cmd.Parameters.Add("@return_notice", SqlDbType.VarChar, 200);
                output.Direction = ParameterDirection.Output;

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                return output.Value?.ToString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                Log.Error(ex,
                    "Error creating new application. AdminCode={AdminCode}, Serial={Serial}, ApplType={ApplType}, Activity={Activity}, Year={Year}, Suffix={Suffix}, IC={IC}, User={User}",
                    admin_code, serial_num, appl_type, activity_code, support_year, suffix_code, ic, userid);

                throw; // rethrow so upstream can handle it
            }
        }
    }
}