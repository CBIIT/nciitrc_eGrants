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
    public class EgrantsAccess
    {
        //return user list
        public static List<EgrantsCommon.EgrantsUsers> LoadUsers(string act, int index_id, int active_id, int user_id, string login_id, string last_name, string first_name, string middle_name, string email_address, string phone_number, int coordinator_id, int position_id, int ic_id, int egrants_tab, int mgt_tab, int admin_tab, int docman_tab, int cft_tab, int dashboard_tab, int iccoord_tab, int is_coordinator, string end_date, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_egrants_access_control", conn);
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
            cmd.Parameters.Add("@ic_id", System.Data.SqlDbType.Int).Value = ic_id;
            cmd.Parameters.Add("@egrants_tab", System.Data.SqlDbType.Int).Value = egrants_tab;
            cmd.Parameters.Add("@mgt_tab", System.Data.SqlDbType.Int).Value = mgt_tab;
            cmd.Parameters.Add("@admin_tab", System.Data.SqlDbType.Int).Value = admin_tab;
            cmd.Parameters.Add("@docman_tab", System.Data.SqlDbType.Int).Value = docman_tab;
            cmd.Parameters.Add("@cft_tab", System.Data.SqlDbType.Int).Value = cft_tab;
            cmd.Parameters.Add("@dashboard_tab", System.Data.SqlDbType.Int).Value = dashboard_tab;
            cmd.Parameters.Add("@iccoord_tab", System.Data.SqlDbType.Int).Value = iccoord_tab;
            cmd.Parameters.Add("@is_coordinator", System.Data.SqlDbType.Int).Value = is_coordinator;
            cmd.Parameters.Add("@end_date", System.Data.SqlDbType.VarChar).Value = end_date;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;

            conn.Open();

            var Users = new List<EgrantsCommon.EgrantsUsers>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                Users.Add(new EgrantsCommon.EgrantsUsers
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
                    can_admin = rdr["can_admin"]?.ToString(),
                    can_egrants = rdr["can_egrants"]?.ToString(),
                    can_dashboard = rdr["can_dashboard"]?.ToString(),
                    can_mgt = rdr["can_mgt"]?.ToString(),
                    can_docman = rdr["can_docman"]?.ToString(),
                    can_cft = rdr["can_cft"]?.ToString(),
                    can_iccoord = rdr["can_iccoord"]?.ToString(),
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

        //to prevent user data duplicate, before create new or update, check user data and get return notice
        public static string to_preview(string act, int index_id, int active_id, int user_id, string login_id, string last_name, string first_name, string middle_name, string email_address, string phone_number, int coordinator_id, int position_id, int ic_id, int egrants_tab, int mgt_tab, int admin_tab, int docman_tab, int cft_tab, int dashboard_tab, int iccoord_tab, int is_coordinator, string end_date, string ic, string userid)
        {
            string return_notice = "";
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_egrants_access_control", conn);
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
            cmd.Parameters.Add("@ic_id", System.Data.SqlDbType.Int).Value = ic_id;
            cmd.Parameters.Add("@egrants_tab", System.Data.SqlDbType.Int).Value = egrants_tab;
            cmd.Parameters.Add("@mgt_tab", System.Data.SqlDbType.Int).Value = mgt_tab;
            cmd.Parameters.Add("@admin_tab", System.Data.SqlDbType.Int).Value = admin_tab;
            cmd.Parameters.Add("@docman_tab", System.Data.SqlDbType.Int).Value = docman_tab;
            cmd.Parameters.Add("@cft_tab", System.Data.SqlDbType.Int).Value = cft_tab;
            cmd.Parameters.Add("@dashboard_tab", System.Data.SqlDbType.Int).Value = dashboard_tab;
            cmd.Parameters.Add("@iccoord_tab", System.Data.SqlDbType.Int).Value = iccoord_tab;
            cmd.Parameters.Add("@is_coordinator", System.Data.SqlDbType.Int).Value = is_coordinator;
            cmd.Parameters.Add("@end_date", System.Data.SqlDbType.VarChar).Value = end_date;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;

            conn.Open();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                return_notice = rdr[0].ToString();
            }
            conn.Close();
      
            return return_notice;
        }

        //update user data
        public static void run_db(string act, int index_id, int active_id, int user_id, string login_id, string last_name, string first_name, string middle_name, string email_address, string phone_number, int coordinator_id, int position_id, int ic_id, int egrants_tab, int mgt_tab, int admin_tab, int docman_tab, int cft_tab, int dashboard_tab, int iccoord_tab, int is_coordinator, string end_date, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_egrants_access_control", conn);
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
            cmd.Parameters.Add("@ic_id", System.Data.SqlDbType.Int).Value = ic_id;
            cmd.Parameters.Add("@egrants_tab", System.Data.SqlDbType.Int).Value = egrants_tab;
            cmd.Parameters.Add("@mgt_tab", System.Data.SqlDbType.Int).Value = mgt_tab;
            cmd.Parameters.Add("@admin_tab", System.Data.SqlDbType.Int).Value = admin_tab;
            cmd.Parameters.Add("@docman_tab", System.Data.SqlDbType.Int).Value = docman_tab;
            cmd.Parameters.Add("@cft_tab", System.Data.SqlDbType.Int).Value = cft_tab;
            cmd.Parameters.Add("@dashboard_tab", System.Data.SqlDbType.Int).Value = dashboard_tab;
            cmd.Parameters.Add("@iccoord_tab", System.Data.SqlDbType.Int).Value = iccoord_tab;
            cmd.Parameters.Add("@is_coordinator", System.Data.SqlDbType.Int).Value = is_coordinator;
            cmd.Parameters.Add("@end_date", System.Data.SqlDbType.VarChar).Value = end_date;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;

            conn.Open();

            SqlDataReader rdr = cmd.ExecuteReader();

            rdr.Close();
            conn.Close();
        }

        //to review user data inserted by IC Coordinator
        public static List<EgrantsCommon.EgrantsUsers> To_Review(int id)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_egrants_access_control", conn);
            cmd.Parameters.Add("@act", System.Data.SqlDbType.Int).Value = "review";
            cmd.Parameters.Add("@user_id", System.Data.SqlDbType.Int).Value = id;

            conn.Open();

            var Users = new List<EgrantsCommon.EgrantsUsers>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                Users.Add(new EgrantsCommon.EgrantsUsers
                {
                    person_id = rdr["person_id"]?.ToString(),
                    userid = rdr["userid"]?.ToString(),
                    last_name = rdr["last_name"]?.ToString(),
                    first_name = rdr["first_name"]?.ToString(),
                    middle_name = rdr["middle_name"]?.ToString(),
                    email_address = rdr["email"]?.ToString(),
                    phone_number = rdr["phone_number"]?.ToString(),
                    position_id = rdr["position_id"]?.ToString(),
                    position_name = rdr["position_name"]?.ToString(),
                    active = rdr["active"]?.ToString(),
                    ic = rdr["ic"]?.ToString(),
                    coordinator_id = rdr["coordinator_id"]?.ToString(),
                    start_date = rdr["start_date"]?.ToString(),
                    end_date = rdr["end_date"]?.ToString()
                });
            }

            rdr.Close();
            conn.Close();

            return Users;
        }

        public static List<EgrantsCommon.EgrantsUsers> LoadAccept(int accect_person_id, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_egrants_ic_coordinator", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", System.Data.SqlDbType.VarChar).Value = "accept";
            cmd.Parameters.Add("@cord_id", System.Data.SqlDbType.Int).Value = 0;
            cmd.Parameters.Add("@request_user_id", System.Data.SqlDbType.Int).Value = accect_person_id;
            cmd.Parameters.Add("@first_name", System.Data.SqlDbType.VarChar).Value = "";
            cmd.Parameters.Add("@middle_name", System.Data.SqlDbType.VarChar).Value = "";
            cmd.Parameters.Add("@last_name", System.Data.SqlDbType.VarChar).Value = "";
            cmd.Parameters.Add("@login_id", System.Data.SqlDbType.VarChar).Value = "";
            cmd.Parameters.Add("@email_address", System.Data.SqlDbType.VarChar).Value = "";
            cmd.Parameters.Add("@phone_number", System.Data.SqlDbType.VarChar).Value = "";
            cmd.Parameters.Add("@division", System.Data.SqlDbType.VarChar).Value = "";
            cmd.Parameters.Add("@access_type", System.Data.SqlDbType.VarChar).Value = "";
            cmd.Parameters.Add("@start_date", System.Data.SqlDbType.VarChar).Value = "";
            cmd.Parameters.Add("@end_date", System.Data.SqlDbType.VarChar).Value = "";
            cmd.Parameters.Add("@comments", System.Data.SqlDbType.VarChar).Value = "";
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;

            conn.Open();

            var Users = new List<EgrantsCommon.EgrantsUsers>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                Users.Add(new EgrantsCommon.EgrantsUsers
                {
                    person_id = rdr["person_id"]?.ToString(),
                    userid = rdr["userid"]?.ToString(),
                    last_name = rdr["last_name"]?.ToString(),
                    first_name = rdr["first_name"]?.ToString(),
                    middle_name = rdr["middle_name"]?.ToString(),
                    email_address = rdr["email"]?.ToString(),
                    phone_number = rdr["phone_number"]?.ToString(),
                    position_id = rdr["position_id"]?.ToString(),
                    position_name = rdr["position_name"]?.ToString(),
                    active = rdr["active"]?.ToString(),
                    ic = rdr["ic"]?.ToString(),
                    coordinator_id = rdr["coordinator_id"]?.ToString(),
                    start_date = rdr["start_date"]?.ToString(),
                    end_date = rdr["end_date"]?.ToString()
                });
            }

            rdr.Close();
            conn.Close();

            return Users;
        }

        public static int getCharacterIndex(string first_letter)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select index_id from character_index where character_index=@first_letter", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@first_letter", System.Data.SqlDbType.VarChar).Value = first_letter;
       
            conn.Open();
            int CharacterIndex = 0;
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                CharacterIndex = Convert.ToInt16(rdr["index_id"]);
            }
            conn.Close();
            return CharacterIndex;
        }

        //check userid if exists in the system
        public static int ToCheckUserid(string userid)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
            {
                System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select count(*) from vw_people where application_type='egrants' and profile_id=1 and userid = @userid", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@userid", System.Data.SqlDbType.VarChar).Value = userid;

                conn.Open();
                int count_userid = 0;
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    count_userid = Convert.ToInt16(rdr[0]);
                }
                conn.Close();
                return count_userid;
            }
        }

            //public static List<EgrantsCommon.EgrantsUsers> LoadRequest(int id)
            //{
            //    System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            //    System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select id as person_id, user_login as userid, user_fname as first_name, user_lname as last_name,user_mi as middle_name,"+
            //    "email, phone_number, cord_id as coordinator_id, 1 as profile_id, access_type as active, pp.position_id, pp.position_name, 'nci' as ic, convert(varchar,start_date,101) as start_date, end_date " +
            //    " FROM dbo.cord_manager inner join people_positions as pp on dbo.fn_get_position_id(access_type) = pp.position_id WHERE id = @id ", conn);
            //    cmd.Parameters.Add("@id", System.Data.SqlDbType.Int).Value = id;

            //    conn.Open();

            //    var Users = new List<EgrantsCommon.EgrantsUsers>();
            //    SqlDataReader rdr = cmd.ExecuteReader();
            //    while (rdr.Read())
            //    {
            //        Users.Add(new EgrantsCommon.EgrantsUsers
            //        {
            //            person_id = rdr["person_id"]?.ToString(),
            //            userid = rdr["userid"]?.ToString(),
            //            last_name = rdr["last_name"]?.ToString(),
            //            first_name = rdr["first_name"]?.ToString(),
            //            middle_name = rdr["middle_name"]?.ToString(),
            //            email_address = rdr["email"]?.ToString(),
            //            phone_number = rdr["phone_number"]?.ToString(),
            //            position_id = rdr["position_id"]?.ToString(),
            //            position_name = rdr["position_name"]?.ToString(),
            //            active = rdr["active"]?.ToString(),
            //            ic = rdr["ic"]?.ToString(),
            //            coordinator_id = rdr["coordinator_id"]?.ToString(),
            //            start_date = rdr["start_date"]?.ToString(),
            //            end_date = rdr["end_date"]?.ToString()
            //        });
            //    }

            //    rdr.Close();
            //    conn.Close();

            //    return Users;
            //}
        }
}