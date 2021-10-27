using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace egrants_new.Egrants.Builders
{
    public class MailIntegrationBuilder
    {


        public MailIntegrationBuilder()
        {
        }


        public string GetGoodData(int recordId)
        {
            string output = "";
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            SqlCommand cmd = new SqlCommand("select Result from WSHistory where WSHistory_Id = @Id", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@Id", System.Data.SqlDbType.Int).Value = recordId;

            conn.Open();

            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                output = rdr["Result"].ToString();

            };
            
            conn.Close();
            return output;
        }


    }
}