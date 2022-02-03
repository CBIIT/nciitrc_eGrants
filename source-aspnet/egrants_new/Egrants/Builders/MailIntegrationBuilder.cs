using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Services.Description;
using egrants_new.Egrants.Models;
using egrants_new.Integration.EmailRulesEngine;

namespace egrants_new.Egrants.Builders
{
    public class MailIntegrationBuilder
    {
        private EmailIntegrationRepository _repo; 

        public MailIntegrationBuilder()
        {
           _repo = new EmailIntegrationRepository();
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


        public MailIntegrationPage GetMailProcessingResults(int ruleId, int messageId)
        {
            MailIntegrationPage output = new MailIntegrationPage();


            var results = _repo.GetActionResults(ruleId, messageId);


            return new MailIntegrationPage();
        }


    }
}