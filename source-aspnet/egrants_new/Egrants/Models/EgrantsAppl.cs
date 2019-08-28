using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using egrants_new.Models;

namespace egrants_new.Egrants.Models
{
    public class EgrantsAppl
    {
        public class Appl
        {
            public string appl_id { get; set; }
            public string grant_id { get; set; }
            public string full_grant_num { get; set; }
            public string support_year { get; set; }
        }

        public class ApplType
        {
            public string appl_type_code { get; set; }
        }

        //check if this appl_id is existing in appls table, added by Leon 7/10/2019
        public static int CheckApplID(int appl_id)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("select count(*) as count_id from appls where appl_id = @appl_id", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@appl_id", System.Data.SqlDbType.Int).Value = appl_id;

                conn.Open();
                int isexisting = 0;
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    isexisting = Convert.ToInt32(rdr["count_id"]);
                }
                conn.Close();
                return isexisting;          
            }
        }

        //to load Appl Type
        public static List<ApplType> LoadApplType()
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select distinct appl_type_code from appls where appl_id > 0", conn);
            cmd.CommandType = CommandType.Text;

            var ApplTypeList = new List<ApplType>();
            conn.Open();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                ApplTypeList.Add(new ApplType
                {
                    appl_type_code = rdr["appl_type_code"]?.ToString(),

                });
            }
            rdr.Close();
            conn.Close();
            return ApplTypeList;
        }

        public class ActivityCode
        {
            public string activity_code { get; set; }
        }

        //to load Activity Code
        public static List<ActivityCode> LoadActivityCode(string admin_code)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select distinct activity_code from vw_appls where appl_id > 0 and admin_phs_org_code = @admin_code order by activity_code", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@admin_code", System.Data.SqlDbType.VarChar).Value = admin_code;

            conn.Open();

            var ActivityCodeList = new List<ActivityCode>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                ActivityCodeList.Add(new ActivityCode
                {
                    activity_code = rdr["activity_code"]?.ToString(),
                });
            }
            rdr.Close();
            conn.Close();
            return ActivityCodeList;
        }
        
        //to create a new appl
        public static string CreateNewAppl(string admin_code, int serial_num, int appl_type, string activity_code, int support_year, string suffix_code, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("dbo.sp_web_egrants_create_appl", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@admin_code", System.Data.SqlDbType.VarChar).Value = admin_code;
            cmd.Parameters.Add("@serial_num", System.Data.SqlDbType.Int).Value = serial_num;
            cmd.Parameters.Add("@appl_type_code", System.Data.SqlDbType.Int).Value = appl_type;
            cmd.Parameters.Add("@activity_code", System.Data.SqlDbType.VarChar).Value = activity_code;
            cmd.Parameters.Add("@support_year", System.Data.SqlDbType.Int).Value = support_year;
            cmd.Parameters.Add("@suffix_code", System.Data.SqlDbType.VarChar).Value = suffix_code;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;
            cmd.Parameters.Add("@return_notice", System.Data.SqlDbType.VarChar, 200);
            cmd.Parameters["@return_notice"].Direction = System.Data.ParameterDirection.Output;

            conn.Open();
            System.Data.SqlClient.SqlDataReader DataReader = cmd.ExecuteReader();
            DataReader.Close();
            conn.Close();

            var return_message = Convert.ToString(cmd.Parameters["@return_notice"].Value);
            return return_message;
        }

        //create an appl_id list string by year or flag_type
        public static string GetApplsList(int grant_id, string flag_type = null, string years = null)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_web_egrants_load_applid_string", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@grant_id", System.Data.SqlDbType.Int).Value = grant_id;
                cmd.Parameters.Add("@flag_type", System.Data.SqlDbType.VarChar).Value = flag_type;
                cmd.Parameters.Add("@years", System.Data.SqlDbType.VarChar).Value = years;

                conn.Open();
                string appls_list = (string)cmd.ExecuteScalar();
                conn.Close();
                return appls_list;
            }
        }


        //to load appls by admin_code and serial_num
        public static List<EgrantsAppl.Appl> LoadAppls_by_serialnum(string admin_code, int serial_num)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select appl_id, full_grant_num from vw_appls where admin_phs_org_code = @admincode and serial_num=@serialnum order by support_year desc", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@admincode", System.Data.SqlDbType.VarChar).Value = admin_code;
            cmd.Parameters.Add("@serialnum", System.Data.SqlDbType.Int).Value = serial_num;
            conn.Open();

            var GrantYearList = new List<EgrantsAppl.Appl>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                GrantYearList.Add(new EgrantsAppl.Appl
                {        
                    appl_id = rdr["appl_id"]?.ToString(),
                    //support_year = rdr["support_year"]?.ToString(),
                    full_grant_num = rdr["full_grant_num"]?.ToString()
                });
            }
            rdr.Close();
            conn.Close();
            return GrantYearList;
        }

        //to load appls by appl_id
        public static List<EgrantsAppl.Appl> LoadAppls_by_grantid(int grant_id)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select appl_id, full_grant_num from vw_appls where grant_id=@grantid order by support_year desc", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@grantid", System.Data.SqlDbType.Int).Value = grant_id;
            conn.Open();

            var ApplsList = new List<EgrantsAppl.Appl>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                ApplsList.Add(new EgrantsAppl.Appl
                {
                    appl_id = rdr["appl_id"]?.ToString(),
                    full_grant_num = rdr["full_grant_num"]?.ToString()
                });
            }
            rdr.Close();
            conn.Close();
            return ApplsList;
        }

        //to load appls by appl_id
        public static List<EgrantsAppl.Appl> LoadAppls_by_applid(int appl_id)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select appl_id, support_year, full_grant_num from vw_appls where grant_id=(select grant_id from appls where appl_id=@applid) order by support_year desc", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@applid", System.Data.SqlDbType.Int).Value = appl_id;
            conn.Open();

            var GrantYearList = new List<EgrantsAppl.Appl>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                GrantYearList.Add(new EgrantsAppl.Appl
                {
                    appl_id = rdr["appl_id"]?.ToString(),
                    support_year = rdr["support_year"]?.ToString(),
                    full_grant_num = rdr["full_grant_num"]?.ToString()
                });
            }
            rdr.Close();
            conn.Close();
            return GrantYearList;
        }

        //get all appls by appl_id
        public static List<string> GetAllAppls(int grant_id)
        {
            List<string> applsList = new List<string>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
            {
                System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select appl_id, full_grant_num, support_year from vw_appls where doc_count>0 and grant_id=@grant_id order by support_year desc", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@grant_id", System.Data.SqlDbType.Int).Value = grant_id;

                conn.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    string applData = rdr[0].ToString() + ":" + rdr[1].ToString();
                    applsList.Add(applData);
                }
            }
            return applsList;
        }

        //get latest 12 appls by appl_id
        public static List<string> GetDefaultAppls(int grant_id)
        {
            List<string> applsList = new List<string>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
            {
                System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select top 12 appl_id, full_grant_num, support_year from vw_appls where doc_count>0 and grant_id=@grant_id order by support_year desc", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@grant_id", System.Data.SqlDbType.Int).Value = grant_id;

                conn.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    string applData = rdr[0].ToString() + ":" + rdr[1].ToString();
                    applsList.Add(applData);
                }
            }
            return applsList;
        }

        //load uploadable appls by admin_code and serial_num
        public static List<EgrantsAppl.Appl> LoadUploadableAppls_by_serialnum(string admin_code, int serial_num)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select appl_id, full_grant_num,support_year from vw_appls "+
            " where admin_phs_org_code = @admincode and serial_num=@serialnum and frc_destroyed=0 and deleted_by_impac='n' order by support_year desc", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@admincode", System.Data.SqlDbType.VarChar).Value = admin_code;
            cmd.Parameters.Add("@serialnum", System.Data.SqlDbType.VarChar).Value = serial_num;
            conn.Open();

            var GrantYearList = new List<EgrantsAppl.Appl>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                GrantYearList.Add(new EgrantsAppl.Appl
                {
                    appl_id = rdr["appl_id"]?.ToString(),
                    support_year = rdr["support_year"]?.ToString(),
                    full_grant_num = rdr["full_grant_num"]?.ToString()
                });
            }
            rdr.Close();
            conn.Close();
            return GrantYearList;
        }

        //load uploadable appls by appl_id
        public static List<EgrantsAppl.Appl> LoadUploadableAppls_by_applid(int appl_id)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select appl_id, support_year, full_grant_num from vw_appls "+
            " where grant_id = (select grant_id from appls where appl_id = @applid) and frc_destroyed=0 and deleted_by_impac='n' order by support_year desc", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@applid", System.Data.SqlDbType.Int).Value = appl_id;
            conn.Open();

            var GrantYearList = new List<EgrantsAppl.Appl>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                GrantYearList.Add(new EgrantsAppl.Appl
                {
                    appl_id = rdr["appl_id"]?.ToString(),
                    support_year = rdr["support_year"]?.ToString(),
                    full_grant_num = rdr["full_grant_num"]?.ToString()
                });
            }
            rdr.Close();
            conn.Close();
            return GrantYearList;
        }

        //load all appls with documents or without documents
        public static List<string> GetAllApplsList(string admin_code, string serial_num)
        {
            List<string> yearList = new List<string>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("select full_grant_num, appl_id from vw_appls where admin_phs_org_code = @admincode and serial_num = @serialnum order by support_year desc", conn);
                //SqlCommand cmd = new SqlCommand("select support_year_suffix from vw_appls where fy = @fy and activity_code = @mechan  and admin_phs_org_code = @ic and serial_num = @serialnum", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@admincode", admin_code);
                cmd.Parameters.AddWithValue("@serialnum", serial_num);

                conn.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    string applData = rdr[0].ToString() + ":" + rdr[1].ToString();
                    yearList.Add(applData);
                }
            }
            return yearList;
        }             
    }
}