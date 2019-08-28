using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using egrants_new.Models;

namespace egrants_new.Egrants_Admin.Models
{
    public class FlagMaintenance
    {
        public class FlagTypes
        {
            public string flag_type { get; set; }
            public string flag_application { get; set; }
        }
        public static List<FlagTypes> LoadFlagTypes()
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT UPPER(flag_type_code) as flag_type_code, flag_application_code FROM Grants_Flag_Master WHERE end_date is null", conn);
            cmd.CommandType = CommandType.Text;

            conn.Open();

            var Flagtypes = new List<FlagTypes>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                Flagtypes.Add(new FlagTypes
                {
                    flag_type = rdr["flag_type_code"]?.ToString(),
                    flag_application = rdr["flag_application_code"]?.ToString()
                });
            }
            rdr.Close();
            conn.Close();

            return Flagtypes;
        }

        public class Flags
        {
            public string gf_id { get; set; }
            public string serial_num { get; set; }
            public string grant_id { get; set; }
            public string appl_id { get; set; }
            public string grant_num { get; set; }
            public string full_grant_num { get; set; }
            public string flag { get; set; }
            public string flag_type { get; set; }
            public string flag_application { get; set; }
            public string flag_icon_namepath { get; set; }
        }

        //load flags
        public static List<Flags> LoadFlags(string act, string flag_type, string admin_code, int serial_num, string id_string, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("dbo.sp_web_admin_flag_maintenance", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", System.Data.SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@flag_type", System.Data.SqlDbType.VarChar).Value = flag_type;
            cmd.Parameters.Add("@admin_code", System.Data.SqlDbType.VarChar).Value = admin_code;
            cmd.Parameters.Add("@serial_num", System.Data.SqlDbType.Int).Value = serial_num;
            cmd.Parameters.Add("@id_string", System.Data.SqlDbType.VarChar).Value = id_string;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;

            conn.Open();

            var Flags = new List<Flags>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                Flags.Add(new Flags
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
            }
            rdr.Close();
            conn.Close();

            return Flags;
        }

        public class ApplFlags
        {
            public string appl_id { get; set; }
            public string fgn { get; set; }
            public string creator { get; set; }
            public string created_date { get; set; }
            public string exclusion_reason { get; set; }
        }

        //load appls with flag
        public static List<ApplFlags> LoadAppls(string act, string flag_type, string admin_code, int serial_num, string id_string, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("dbo.sp_web_admin_flag_maintenance", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", System.Data.SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@flag_type", System.Data.SqlDbType.VarChar).Value = flag_type;
            cmd.Parameters.Add("@admin_code", System.Data.SqlDbType.VarChar).Value = admin_code;
            cmd.Parameters.Add("@serial_num", System.Data.SqlDbType.Int).Value = serial_num;
            cmd.Parameters.Add("@id_string", System.Data.SqlDbType.VarChar).Value = id_string;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;

            conn.Open();

            var Appls = new List<ApplFlags>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                Appls.Add(new ApplFlags
                {                   
                    appl_id = rdr["appl_id"]?.ToString(),
                    fgn= rdr["fgn"]?.ToString(),
                    creator= rdr["creator"]?.ToString(),
                    created_date= rdr["created_date"]?.ToString(),
                    exclusion_reason= rdr["exclusion_reason"]?.ToString()                   
                });
            }
            rdr.Close();
            conn.Close();

            return Appls;
        }

        //add, delete or edit flag
        public static void run_db(string act, string flag_type, string admin_code, int serial_num, string id_string, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("dbo.sp_web_admin_flag_maintenance", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", System.Data.SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@flag_type", System.Data.SqlDbType.VarChar).Value = flag_type;
            cmd.Parameters.Add("@admin_code", System.Data.SqlDbType.VarChar).Value = admin_code;
            cmd.Parameters.Add("@serial_num", System.Data.SqlDbType.Int).Value = serial_num;
            cmd.Parameters.Add("@id_string", System.Data.SqlDbType.VarChar).Value = id_string;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;
            conn.Open();
            SqlDataReader rdr = cmd.ExecuteReader();
            rdr.Close();
            conn.Close();
        }
    }
}