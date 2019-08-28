using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using egrants_new.Models;

namespace egrants_new.Models
{
    public class SystemReport
    {
        public class EgrantAccessions
        {
            public string accession_id { get; set; }
            public string accession_number { get; set; }
            public string accession_year { get; set; }
            public string accession_counter { get; set; }
        }

        public static List<EgrantAccessions> LoadAccessions(string ic)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT accession_id,accession_number FROM eim.dbo.accessions WHERE contract = 0 and profile_id = (select profile_id from profiles where profile = @ic) order by accession_id desc", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic ;
                conn.Open();

                var Accessions = new List<EgrantAccessions>();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    Accessions.Add(new EgrantAccessions { accession_id = rdr["accession_id"].ToString(), accession_number = rdr["accession_number"].ToString() });
                }

                return Accessions;
        }
        public class EgrantFolders
        {
            public string folder_id { get; set; }      
            public string grant_num { get; set; }
            public string bar_code { get; set; }
            public string former_grant_num { get; set; }
            public string id_string { get; set; }
            public string latest_move_date { get; set; }
            public string current_status { get; set; }  
            public string closed_out { get; set; }
            public string accession_destroyed_date { get; set; }
        }

        public static List<EgrantFolders> LoadFolders(string act, int search_number, string ic, string userid)
        {

            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_management_system_report", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add("@act", System.Data.SqlDbType.VarChar).Value = act;
                cmd.Parameters.Add("@search_number", System.Data.SqlDbType.Int).Value = search_number;
                cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;                
                cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;      

                conn.Open();

                var EgrantsFolders = new List<EgrantFolders>();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    EgrantsFolders.Add(new EgrantFolders {
                        folder_id = rdr["folder_id"]?.ToString(),
                        bar_code = rdr["bar_code"]?.ToString(),
                        grant_num = rdr["grant_num"]?.ToString(),
                        former_grant_num = rdr["former_grant_num"]?.ToString(),
                        id_string = rdr["id_string"]?.ToString(),
                        latest_move_date = rdr["latest_move_date"]?.ToString(),
                        current_status = rdr["current_status"]?.ToString(),
                        closed_out = rdr["closed_out"]?.ToString(),
                        accession_destroyed_date = rdr["accession_destroyed_date"]?.ToString(),
                    });
                }

                rdr.Close();
                conn.Close();

                return EgrantsFolders;      
        }
    }
}