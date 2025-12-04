using System.Data;

using eGrants.DAL;
using eGrants.Models;
using eGrants.Services.Interfaces;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace eGrants.Services
{
    public class FlagMaintenanceService : IFlagMaintenanceService
    {
        
        private readonly ISessionInfoService _sessionInfoService;
        private readonly AppDbContext _context;

        public FlagMaintenanceService(ISessionInfoService sessionInfoService, AppDbContext context)
        {
            _sessionInfoService = sessionInfoService;
            _context = context;
        }

        /// <summary>
        ///     The load flag types.
        /// </summary>
        /// <returns>
        ///     The <see cref="System.Collections.Generic.List`1" /> .
        /// </returns>
        public List<FlagTypes> LoadFlagTypes()
        {
            using (var conn = new SqlConnection(_context.Database.GetConnectionString()))
            {
                var cmd = new SqlCommand(
               "SELECT UPPER(flag_type_code) as flag_type_code, flag_application_code FROM Grants_Flag_Master WHERE end_date is null",
               conn);

                cmd.CommandType = CommandType.Text;

                conn.Open();

                var list = new List<FlagTypes>();
                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    list.Add(
                        new FlagTypes { flag_type = rdr["flag_type_code"]?.ToString(), flag_application = rdr["flag_application_code"]?.ToString() });

                rdr.Close();
                conn.Close();

                return list;
            }
               
        }

        public List<Flags> LoadFlags(
            string act,
            string flag_type,
            string admin_code,
            int serial_num,
            string id_string,
            string ic,
            string userid)
        {
            using (var conn = new SqlConnection(_context.Database.GetConnectionString()))
            {
                var cmd = new SqlCommand("dbo.sp_web_admin_flag_maintenance", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
                cmd.Parameters.Add("@flag_type", SqlDbType.VarChar).Value = flag_type;
                cmd.Parameters.Add("@admin_code", SqlDbType.VarChar).Value = admin_code;
                cmd.Parameters.Add("@serial_num", SqlDbType.Int).Value = serial_num;
                cmd.Parameters.Add("@id_string", SqlDbType.VarChar).Value = id_string;
                cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
                cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;

                conn.Open();

                var list = new List<Flags>();
                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    list.Add(
                        new Flags
                        {
                            gf_id = rdr["gf_id"]?.ToString(),
                            serial_num = rdr["serial_num"]?.ToString(),
                            grant_id = rdr["grant_id"]?.ToString(),
                            appl_id = rdr["appl_id"]?.ToString(),
                            grant_num = rdr["grant_num"]?.ToString(),
                            full_grant_num = rdr["full_grant_num"]?.ToString(),
                            flag = rdr["flag"]?.ToString(),
                            flag_type = rdr["flag_type"]?.ToString(),
                            flag_application = rdr["flag_application"]?.ToString(),
                            flag_icon_namepath = rdr["flag_icon_namepath"]?.ToString()
                        });

                rdr.Close();
                conn.Close();

                return list;
            }
        }

        // load appls with flag
        public List<ApplFlags> LoadAppls(
            string act,
            string flag_type,
            string admin_code,
            int serial_num,
            string id_string,
            string ic,
            string userid)
        {
            using (var conn = new SqlConnection(_context.Database.GetConnectionString()))
            {
                var cmd = new SqlCommand("dbo.sp_web_admin_flag_maintenance", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
                cmd.Parameters.Add("@flag_type", SqlDbType.VarChar).Value = flag_type;
                cmd.Parameters.Add("@admin_code", SqlDbType.VarChar).Value = admin_code;
                cmd.Parameters.Add("@serial_num", SqlDbType.Int).Value = serial_num;
                cmd.Parameters.Add("@id_string", SqlDbType.VarChar).Value = id_string;
                cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
                cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;

                conn.Open();

                var list = new List<ApplFlags>();
                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    list.Add(
                        new ApplFlags
                        {
                            appl_id = Convert.ToInt32(rdr["appl_id"]),
                            fgn = rdr["fgn"]?.ToString(),
                            creator = rdr["creator"]?.ToString(),
                            created_date = rdr["created_date"]?.ToString(),
                            exclusion_reason = rdr["exclusion_reason"]?.ToString()
                        });

                rdr.Close();
                conn.Close();

                return list;
            }
           
            
        }

        // add, delete or edit flag
        public void run_db(string act, string flag_type, string admin_code, int serial_num, string id_string, string ic, string userid)
        {
            using (var conn = new SqlConnection(_context.Database.GetConnectionString()))
            {
                var cmd = new SqlCommand("dbo.sp_web_admin_flag_maintenance", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
                cmd.Parameters.Add("@flag_type", SqlDbType.VarChar).Value = flag_type;
                cmd.Parameters.Add("@admin_code", SqlDbType.VarChar).Value = admin_code;
                cmd.Parameters.Add("@serial_num", SqlDbType.Int).Value = serial_num;
                cmd.Parameters.Add("@id_string", SqlDbType.VarChar).Value = id_string;
                cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
                cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;
                conn.Open();
                var rdr = cmd.ExecuteReader();
                rdr.Close();
                conn.Close();
            }                
            
        }

    }
}

