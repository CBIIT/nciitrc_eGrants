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
    public class DocmanAccess
    {
        //to load user data
        public static List<DocmanCommon.DocmanUsers> LoadUsers(string act, int index_id, int active_id, int user_id, string login_id, string last_name, string first_name, string middle_name, string email_address, string phone_number, int coordinator_id, int position_id, int docman_tab, int cft_tab, int is_coordinator, string end_date, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_docman_access_control", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", System.Data.SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@index_id", System.Data.SqlDbType.Int).Value = index_id;
            cmd.Parameters.Add("@active_id", System.Data.SqlDbType.Int).Value = active_id;
            cmd.Parameters.Add("@user_id", System.Data.SqlDbType.Int).Value = user_id;
            cmd.Parameters.Add("@login_id", System.Data.SqlDbType.VarChar).Value = login_id;
            cmd.Parameters.Add("@first_name", System.Data.SqlDbType.VarChar).Value = first_name;
            cmd.Parameters.Add("@middle_name", System.Data.SqlDbType.VarChar).Value = middle_name;
            cmd.Parameters.Add("@last_name", System.Data.SqlDbType.VarChar).Value = last_name;
            cmd.Parameters.Add("@email_address", System.Data.SqlDbType.VarChar).Value = email_address;
            cmd.Parameters.Add("@phone_number", System.Data.SqlDbType.VarChar).Value = phone_number;     
            cmd.Parameters.Add("@position_id", System.Data.SqlDbType.Int).Value = position_id;
            cmd.Parameters.Add("@docman_tab", System.Data.SqlDbType.Int).Value = docman_tab;
            cmd.Parameters.Add("@cft_tab", System.Data.SqlDbType.Int).Value = cft_tab;
            cmd.Parameters.Add("@coordinator_id", System.Data.SqlDbType.Int).Value = coordinator_id;
            cmd.Parameters.Add("@is_coordinator", System.Data.SqlDbType.Int).Value = is_coordinator;
            cmd.Parameters.Add("@end_date", System.Data.SqlDbType.VarChar).Value = end_date;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;
            conn.Open();

            var Users = new List<DocmanCommon.DocmanUsers>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                Users.Add(new DocmanCommon.DocmanUsers
                {
                    person_id = rdr["person_id"]?.ToString(),
                    userid = rdr["userid"]?.ToString(),
                    person_name = rdr["person_name"]?.ToString(),
                    last_name = rdr["last_name"]?.ToString(),
                    first_name = rdr["first_name"]?.ToString(),
                    middle_name = rdr["middle_name"]?.ToString(),
                    email_address = rdr["email"]?.ToString(),
                    phone_number = rdr["phone_number"]?.ToString(),
                    position_id = rdr["position_id"]?.ToString(),
                    position_name = rdr["position_name"]?.ToString(),
                    application_type = rdr["application_type"]?.ToString(),
                    active = rdr["active"]?.ToString(),
                    ic = rdr["ic"]?.ToString(),
                    can_cft = rdr["can_cft"]?.ToString(),
                    can_docman = rdr["can_docman"]?.ToString(),
                    is_coordinator = rdr["is_coordinator"]?.ToString(),
                    coordinator_id = rdr["coordinator_id"]?.ToString(),
                    start_date = rdr["start_date"]?.ToString(),
                    end_date = rdr["end_date"]?.ToString()
                });
            }
            rdr.Close();
            conn.Close();
            return Users;
        }
                      
        //to create or update user data            
        public static void run_db(string act, int index_id, int active_id, int user_id, string login_id, string last_name, string first_name, string middle_name, string email_address, string phone_number, int coordinator_id, int position_id, int docman_tab, int cft_tab, int is_coordinator, string end_date, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_docman_access_control", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", System.Data.SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@index_id", System.Data.SqlDbType.Int).Value = index_id;
            cmd.Parameters.Add("@active_id", System.Data.SqlDbType.Int).Value = active_id;
            cmd.Parameters.Add("@user_id", System.Data.SqlDbType.Int).Value = user_id;
            cmd.Parameters.Add("@login_id", System.Data.SqlDbType.VarChar).Value = login_id;
            cmd.Parameters.Add("@first_name", System.Data.SqlDbType.VarChar).Value = first_name;
            cmd.Parameters.Add("@middle_name", System.Data.SqlDbType.VarChar).Value = middle_name;
            cmd.Parameters.Add("@last_name", System.Data.SqlDbType.VarChar).Value = last_name;
            cmd.Parameters.Add("@email_address", System.Data.SqlDbType.VarChar).Value = email_address;
            cmd.Parameters.Add("@phone_number", System.Data.SqlDbType.VarChar).Value = phone_number;
            cmd.Parameters.Add("@coordinator_id", System.Data.SqlDbType.Int).Value = coordinator_id;
            cmd.Parameters.Add("@position_id", System.Data.SqlDbType.Int).Value = position_id;
            cmd.Parameters.Add("@docman_tab", System.Data.SqlDbType.Int).Value = docman_tab;
            cmd.Parameters.Add("@cft_tab", System.Data.SqlDbType.Int).Value = cft_tab;
            cmd.Parameters.Add("@is_coordinator", System.Data.SqlDbType.Int).Value = is_coordinator;
            cmd.Parameters.Add("@end_date", System.Data.SqlDbType.VarChar).Value = end_date;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;
            conn.Open();
            SqlDataReader rdr = cmd.ExecuteReader();
            rdr.Close();
            conn.Close();
        }
    }
}