using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using egrants_new.Models;

namespace egrants_new.IC_Coordinator.Models
{
    public class Coordinator
    {

        //load Coordinators
        //public static List<EgrantsCommon.EgrantsUsers> LoadCoordinators()
        //{
        //    System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
        //    System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select person_id, person_name from vw_people where is_coordinator=1 order by person_name", conn);
        //    cmd.CommandType = CommandType.Text;
        //    conn.Open();

        //    var Coordinators = new List<EgrantsCommon.EgrantsUsers>();
        //    SqlDataReader rdr = cmd.ExecuteReader();
        //    while (rdr.Read())
        //    {
        //        Coordinators.Add(new EgrantsCommon.EgrantsUsers
        //        {
        //            person_id = rdr["person_id"]?.ToString(),
        //            person_name = rdr["person_name"]?.ToString(),
        //        });
        //    }
        //    rdr.Close();
        //    conn.Close();
        //    return Coordinators;
        //}

        public class RequestedUsers
        {
            public string id { get; set; }
            public string person_id { get; set; }
            public string person_name { get; set; }
            public string userid { get; set; }
            public string first_name { get; set; }
            public string middle_name { get; set; }
            public string last_name { get; set; }
            public string phone_number { get; set; }
            public string email_address { get; set; }
            public string profile_id { get; set; }
            public string position_id { get; set; }
            public string position_name { get; set; }
            public string access_type { get; set; }
            public string comments { get; set; }
            public string division { get; set; }
            public string application_type { get; set; }
            public string status { get; set; }
            public string coordinator_id { get; set; }
            public string coordinator_name { get; set; }
            public string start_date { get; set; }
            public string status_date { get; set; }
            public string end_date { get; set; }
            public string create_date { get; set; }
            public string created_by_person_id { get; set; }
            public string review_by_person_id { get; set; }
            public string lastaccess_date { get; set; }
            public string requested_date { get; set; }
        }

        public static List<RequestedUsers> LoadRequestedUsers(string act, int cord_id, int request_user_id, string first_name, string middle_name, string last_name, string login_id, string email_address, string phone_number, string division, string access_type, string start_date, string end_date, string comments, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_egrants_ic_coordinator", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", System.Data.SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@cord_id", System.Data.SqlDbType.Int).Value = cord_id;
            cmd.Parameters.Add("@request_user_id", System.Data.SqlDbType.Int).Value = request_user_id;
            cmd.Parameters.Add("@first_name", System.Data.SqlDbType.VarChar).Value = first_name;
            cmd.Parameters.Add("@middle_name", System.Data.SqlDbType.VarChar).Value = middle_name;
            cmd.Parameters.Add("@last_name", System.Data.SqlDbType.VarChar).Value = last_name;
            cmd.Parameters.Add("@login_id", System.Data.SqlDbType.VarChar).Value = login_id;
            cmd.Parameters.Add("@email_address", System.Data.SqlDbType.VarChar).Value = email_address;
            cmd.Parameters.Add("@phone_number", System.Data.SqlDbType.VarChar).Value = phone_number;
            cmd.Parameters.Add("@division", System.Data.SqlDbType.VarChar).Value = division;
            cmd.Parameters.Add("@access_type", System.Data.SqlDbType.VarChar).Value = access_type;
            cmd.Parameters.Add("@start_date", System.Data.SqlDbType.VarChar).Value = start_date;
            cmd.Parameters.Add("@end_date", System.Data.SqlDbType.VarChar).Value = end_date;
            cmd.Parameters.Add("@comments", System.Data.SqlDbType.VarChar).Value = comments;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;

            conn.Open();

            var Users = new List<RequestedUsers>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                Users.Add(new RequestedUsers
                {
                    id = rdr["id"]?.ToString(),
                    first_name = rdr["first_name"]?.ToString(),
                    middle_name = rdr["middle_name"]?.ToString(),
                    last_name = rdr["last_name"]?.ToString(),
                    userid = rdr["userid"]?.ToString(),
                    division = rdr["division"]?.ToString(),
                    status = rdr["status"]?.ToString(),
                    access_type = rdr["access_type"]?.ToString(),
                    email_address = rdr["email_address"]?.ToString(),
                    phone_number = rdr["phone_number"]?.ToString(),
                    review_by_person_id = rdr["review_by_person_id"]?.ToString(),
                    created_by_person_id = rdr["created_by_person_id"]?.ToString(),
                    start_date = rdr["start_date"]?.ToString(),
                    end_date = rdr["end_date"]?.ToString(),
                    status_date = rdr["status_date"]?.ToString(),
                    requested_date = rdr["requested_date"]?.ToString(),
                    lastaccess_date = rdr["lastaccess_date"]?.ToString(),
                    coordinator_name = rdr["coordinator_name"]?.ToString(),
                    coordinator_id = rdr["coordinator_id"]?.ToString(),
                    comments = rdr["comments"]?.ToString()
                });
            }

            rdr.Close();
            conn.Close();
            return Users;
        }

        public static void AccessUsers(string act, int cord_id, int request_user_id, string first_name, string middle_name, string last_name, string login_id, string email_address, string phone_number, string division, string access_type, string start_date, string end_date, string comments, string ic, string userid)
        {
            if (middle_name == "null")
            {
                middle_name = "";
            }

            if (end_date == "null")
            {
                end_date = "";
            }

            if (comments == "null")
            {
                comments = "";
            }

            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_egrants_ic_coordinator", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", System.Data.SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@cord_id", System.Data.SqlDbType.Int).Value = cord_id;
            cmd.Parameters.Add("@request_user_id", System.Data.SqlDbType.Int).Value = request_user_id;
            cmd.Parameters.Add("@first_name", System.Data.SqlDbType.VarChar).Value = first_name;
            cmd.Parameters.Add("@middle_name", System.Data.SqlDbType.VarChar).Value = middle_name;
            cmd.Parameters.Add("@last_name", System.Data.SqlDbType.VarChar).Value = last_name;
            cmd.Parameters.Add("@login_id", System.Data.SqlDbType.VarChar).Value = login_id;
            cmd.Parameters.Add("@email_address", System.Data.SqlDbType.VarChar).Value = email_address;
            cmd.Parameters.Add("@phone_number", System.Data.SqlDbType.VarChar).Value = phone_number;
            cmd.Parameters.Add("@division", System.Data.SqlDbType.VarChar).Value = division;
            cmd.Parameters.Add("@access_type", System.Data.SqlDbType.VarChar).Value = access_type;
            cmd.Parameters.Add("@start_date", System.Data.SqlDbType.VarChar).Value = start_date;
            cmd.Parameters.Add("@end_date", System.Data.SqlDbType.VarChar).Value = end_date;
            cmd.Parameters.Add("@comments", System.Data.SqlDbType.VarChar).Value = comments;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;

            conn.Open();
            System.Data.SqlClient.SqlDataReader DataReader = cmd.ExecuteReader();
            DataReader.Close();
            conn.Close();
        }
    }
}