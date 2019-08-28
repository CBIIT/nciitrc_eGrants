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
    public class ApplDestructed
    {
        public class DestructionYears
        {
            public string year { get; set; }
        }
        public static List<DestructionYears> LoadYears()
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT distinct year(EGRANTS_CREATED_DATE) as [year] FROM dbo.IMPAC_DESTRUCTED_APPL order by [year] desc", conn);
            conn.Open();

            var Years = new List<DestructionYears>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                Years.Add(new DestructionYears
                {
                    year = rdr["year"]?.ToString()
                });
            }
            rdr.Close();
            conn.Close();
            return Years;
        }

        public class DescripCodes
        {
            public string descrip_code { get; set; }

        }

        public static List<DescripCodes> LoadDescripCodes()
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT distinct APPL_STATUS_GRP_DESCRIP as descrip_code FROM dbo.IMPAC_DESTRUCTED_APPL " +
            "WHERE APPL_STATUS_GRP_DESCRIP is not null ORDER BY APPL_STATUS_GRP_DESCRIP", conn);
            conn.Open();

            var Codes = new List<DescripCodes>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                Codes.Add(new DescripCodes
                {
                    descrip_code = rdr["descrip_code"]?.ToString()
                });
            }
            rdr.Close();
            conn.Close();
            return Codes;
        }
        public class ExceptionCodes
        {
            public string id { get; set; }
            public string exception_code { get; set; }
            public string detail { get; set; }
            public string created_date { get; set; }
            public string created_by { get; set; }
        }
        public static List<ExceptionCodes> LoadExceptionCodes()
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT id, code as exception_code, detail, convert(varchar,created_date,101) as created_date, dbo.fn_get_person_name(created_by_person_id) as created_by "+
            " FROM dbo.IMPAC_DESTRUCT_OGA_EXCEPTION WHERE disable_date is null ORDER BY exception_code", conn);
            conn.Open();

            var Codes = new List<ExceptionCodes>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                Codes.Add(new ExceptionCodes
                {
                    id = rdr["id"]?.ToString(),
                    exception_code = rdr["exception_code"]?.ToString(),
                    detail = rdr["detail"]?.ToString(),
                    created_date = rdr["created_date"]?.ToString(),
                    created_by= rdr["created_by"]?.ToString()
                });
            }
            rdr.Close();
            conn.Close();
            return Codes;
        }

        public class DestructedsAppls
        {
            public string appl_id{ get; set; }
            public string full_grant_num { get; set; }
            public string serial_num { get; set; }
            public string exception_code { get; set; }
            public string status_code { get; set; }
            public string step_code { get; set; }
            public string appl_editable{ get; set; }
        }

        public static List<DestructedsAppls> LoadAppls(string act, int year, string status_code, string exception_code, string str, string id_string, string exception_type, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_admin_appl_destructed", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", System.Data.SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@year", System.Data.SqlDbType.Int).Value = year;
            cmd.Parameters.Add("@status_code", System.Data.SqlDbType.VarChar).Value = status_code;
            cmd.Parameters.Add("@exception_code", System.Data.SqlDbType.VarChar).Value = exception_code;
            cmd.Parameters.Add("@str", System.Data.SqlDbType.VarChar).Value = str;
            cmd.Parameters.Add("@id_string", System.Data.SqlDbType.VarChar).Value = id_string;
            cmd.Parameters.Add("@exception_type", System.Data.SqlDbType.VarChar).Value = exception_type;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;
            conn.Open();

            var Appls = new List<DestructedsAppls>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                Appls.Add(new DestructedsAppls
                {
                    appl_id = rdr["appl_id"]?.ToString(),
                    full_grant_num = rdr["full_grant_num"]?.ToString(),
                    serial_num = rdr["serial_num"]?.ToString(),
                    exception_code = rdr["exception_code"]?.ToString(),
                    status_code = rdr["status_code"]?.ToString(),
                    step_code = rdr["step_code"]?.ToString(),
                    appl_editable = rdr["appl_editable"]?.ToString()
                });
            }
            rdr.Close();
            conn.Close();
            return Appls;
        }

        public class SearchInfo
        {          
            public string total_appls { get; set; }
            public string total_pages { get; set; }
            public string per_page { get; set; }
        }

        public static List<SearchInfo> LoadSearchInfo(int year, string status_code, string exception_code, string str)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_admin_appl_destructed_index", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@year", System.Data.SqlDbType.Int).Value = year;
            cmd.Parameters.Add("@status_code", System.Data.SqlDbType.VarChar).Value = status_code;
            cmd.Parameters.Add("@exception_code", System.Data.SqlDbType.VarChar).Value = exception_code;
            cmd.Parameters.Add("@str", System.Data.SqlDbType.VarChar).Value = str;

            conn.Open();

            var SearchInfo = new List<SearchInfo>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                SearchInfo.Add(new SearchInfo
                {                  
                    total_appls = rdr["total_appls"]?.ToString(),
                    total_pages = rdr["total_pages"]?.ToString(),
                    per_page = rdr["per_page"]?.ToString()
                });
            }
            rdr.Close();
            conn.Close();
            return SearchInfo;
        }

        public static string CheckPermission(int year, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select dbo.fn_is_Archival_admin(@year,(select person_id from people where userid=@userid)) as permission", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@year", System.Data.SqlDbType.Int).Value = year;
            cmd.Parameters.Add("@userid", System.Data.SqlDbType.VarChar).Value = userid;
            conn.Open();        
            string Processable= (string)cmd.ExecuteScalar();
            conn.Close();         
            return Processable;
        }

        public static void EditExceptionCode( string act, int id, string detail, string code, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_admin_appl_destructed_edit", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", System.Data.SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@id", System.Data.SqlDbType.Int).Value = id;
            cmd.Parameters.Add("@detail", System.Data.SqlDbType.VarChar).Value = detail;
            cmd.Parameters.Add("@code", System.Data.SqlDbType.VarChar).Value = code;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@Operator", System.Data.SqlDbType.VarChar).Value = userid;
            conn.Open();
            SqlDataReader rdr = cmd.ExecuteReader();
            rdr.Close();
            conn.Close();
        }
    }
}