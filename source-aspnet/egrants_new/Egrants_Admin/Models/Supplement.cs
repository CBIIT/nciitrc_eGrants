using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using egrants_new.Models;

namespace egrants_new.Egrants_Admin.Models
{
    public class Supplement
    {
        public class Notifications
        {
            public string id { get; set; }
            public string full_grant_num { get; set; }
            public string appl_id { get; set; }
            public string pa { get; set; }
            public string subjectLine { get; set; }
            public string NotificationBody { get; set; }
            public string NotRcvd_dt { get; set; }
            public string created_date { get; set; }
        }

        public static List<Notifications> LoadNotifications(string act, string pa, string detail, int id, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_admin_supplement_loaddata", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", System.Data.SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@pa", System.Data.SqlDbType.VarChar).Value = pa;
            cmd.Parameters.Add("@detail", System.Data.SqlDbType.VarChar).Value = detail;
            cmd.Parameters.Add("@id", System.Data.SqlDbType.Int).Value = id;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;
            conn.Open();

            var Notification = new List<Notifications>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                Notification.Add(new Notifications
                {
                    id = rdr["id"]?.ToString(),
                    full_grant_num = rdr["full_grant_num"]?.ToString(),
                    appl_id = rdr["appl_id"]?.ToString(),
                    pa = rdr["pa"]?.ToString(),
                    subjectLine = rdr["subjectLine"]?.ToString(),
                    NotificationBody = rdr["NotificationBody"]?.ToString(),
                    NotRcvd_dt = rdr["NotRcvd_dt"]?.ToString(),
                    created_date = rdr["created_date"]?.ToString()
                });
            }
            rdr.Close();
            conn.Close();

            return Notification;
        }

        public class NotificationStatus
        {
            public string tag { get; set; }
            public string full_grant_num { get; set; }
            public string pa { get; set; }
            public string id { get; set; }
            public string document_id { get; set; }
            public string document_date { get; set; }
            public string url { get; set; }
            public string category_name { get; set; }
        }

        public static List<NotificationStatus> ReviewNotifications(string act, string pa, string detail, int id, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_admin_supplement_loaddata", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", System.Data.SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@pa", System.Data.SqlDbType.VarChar).Value = pa;
            cmd.Parameters.Add("@detail", System.Data.SqlDbType.VarChar).Value = detail;
            cmd.Parameters.Add("@id", System.Data.SqlDbType.Int).Value = id;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;
            //cmd.Parameters["@return_notice"].Direction = System.Data.ParameterDirection.Output;
            conn.Open();

            var NotificationStatus = new List<NotificationStatus>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                NotificationStatus.Add(new NotificationStatus
                {
                    tag = rdr["tag"]?.ToString(),
                    full_grant_num = rdr["full_grant_num"]?.ToString(),
                    pa = rdr["pa"]?.ToString(),
                    id = rdr["id"]?.ToString(),
                    document_id = rdr["document_id"]?.ToString(),
                    document_date = rdr["document_date"]?.ToString(),
                    category_name = rdr["category_name"]?.ToString(),
                    url = rdr["url"]?.ToString()
                });
            }
            rdr.Close();
            conn.Close();

            return NotificationStatus;
        }

        public class EmailStatus
        {
            public string id { get; set; }
            public string email_type { get; set; }
            public string email_address { get; set; }
            public string email_date { get; set; }
            public string email_send_status { get; set; }
            public string position { get; set; }
            public string person_name { get; set; }
            public string created_date { get; set; }
            public string reply_status { get; set; }
            public string reply_recieved_date { get; set; }
        }

        public static List<EmailStatus> ReviewEmailStatus(int id)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select id,email as email_type, position, person_name,email_address,convert(varchar, email_date, 101) as email_date, " +
            " convert(varchar, created_date, 101) as created_date, email_send_status, convert(varchar, reply_recieved_date, 101) as reply_recieved_date, reply_status " +
            " from adsup_Notification_email_status where Notification_id = @id", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@id", System.Data.SqlDbType.Int).Value = id;

            conn.Open();

            var EmailStatus = new List<EmailStatus>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                EmailStatus.Add(new EmailStatus
                {
                    id = rdr["id"]?.ToString(),
                    email_type = rdr["email_type"]?.ToString(),
                    email_date = rdr["email_date"]?.ToString(),
                    email_address = rdr["email_address"]?.ToString(),
                    email_send_status = rdr["email_send_status"]?.ToString(),
                    position = rdr["position"]?.ToString(),
                    person_name = rdr["person_name"]?.ToString(),
                    reply_status = rdr["reply_status"]?.ToString(),
                    reply_recieved_date = rdr["reply_recieved_date"]?.ToString(),
                    created_date = rdr["created_date"]?.ToString()
                });
            }
            rdr.Close();
            conn.Close();

            return EmailStatus;
        }

        public class EmailPositions
        {
            public string email_position_code { get; set; }
        }

        public static List<EmailPositions> LoadEmailPositionList()
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select distinct ltrim(rtrim(email_position_code)) as email_position_code from dbo.adsup_email_position_master order by email_position_code", conn);
            cmd.CommandType = CommandType.Text;

            conn.Open();

            var EmailPositionList = new List<EmailPositions>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                EmailPositionList.Add(new EmailPositions
                {
                    email_position_code = rdr["email_position_code"]?.ToString()
                });
            }
            rdr.Close();
            conn.Close();

            return EmailPositionList;
        }

        public static string GetNotice(string act, string pa, string detail, int id, string name, string subject, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_admin_supplement_modify", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", System.Data.SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@pa", System.Data.SqlDbType.VarChar).Value = pa;         
            cmd.Parameters.Add("@detail", System.Data.SqlDbType.VarChar).Value = detail;
            cmd.Parameters.Add("@id", System.Data.SqlDbType.Int).Value = id;
            cmd.Parameters.Add("@name", System.Data.SqlDbType.VarChar).Value = name;
            cmd.Parameters.Add("@subject", System.Data.SqlDbType.VarChar).Value = subject;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;
            cmd.Parameters.Add("@return_notice", System.Data.SqlDbType.VarChar, 200);
            cmd.Parameters["@return_notice"].Direction = System.Data.ParameterDirection.Output;

            conn.Open();
            System.Data.SqlClient.SqlDataReader DataReader = cmd.ExecuteReader();

            DataReader.Close();
            conn.Close();

            var return_notice = Convert.ToString(cmd.Parameters["@return_notice"].Value);
            return return_notice;
        }

        public class EmailTemplates
        {
            public string id { get; set; }
            public string template_name { get; set; }
            public string body { get; set; }
            public string subject { get; set; }
            public string created_date { get; set; }
            public string created_by_person_id { get; set; }
        }

        public static List<EmailTemplates> LoadEmailTemplates()
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT ltrim(rtrim(template_name)) as template_name,id,[subject] as subject, dbo.fn_clean_characters(body) as body," +
            "created_by_person_id,convert(varchar,created_date,101) as created_date FROM dbo.adsup_email_master order by template_name", conn);
            cmd.CommandType = CommandType.Text;

            conn.Open();

            var EmailTemplate = new List<EmailTemplates>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                EmailTemplate.Add(new EmailTemplates
                {
                    id = rdr["id"]?.ToString(),
                    template_name= rdr["template_name"]?.ToString(),
                    subject = rdr["subject"]?.ToString(),
                    body = rdr["body"]?.ToString(),
                    created_date = rdr["created_date"]?.ToString(),
                    created_by_person_id= rdr["created_by_person_id"]?.ToString()
                });
            }
            rdr.Close();
            conn.Close();

            return EmailTemplate;
        }

        public class EmailRules
        {
            public string id { get; set; }
            public string pa { get; set; }
        }

        public static List<EmailRules> LoadEmailRulesList()
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT id, ltrim(rtrim(pa)) as pa FROM dbo.adsup_email_rules WHERE end_date is null order by ltrim(rtrim(pa))", conn); /*and email_template_id is not null*/
            cmd.CommandType = CommandType.Text;

            conn.Open();

            var EmailRules = new List<EmailRules>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                EmailRules.Add(new EmailRules
                {
                    id = rdr["id"]?.ToString(),
                    pa = rdr["pa"]?.ToString()
                });
            }
            rdr.Close();
            conn.Close();

            return EmailRules;
        }

        public class EmailRule
        { 
            public string pa { get; set; }
            public string email_to { get; set; }
            public string email_cc { get; set; }
            public string start_date { get; set; }
            public string end_date { get; set; }
            public string email_template_id { get; set; }
            public string email_template_name { get; set; }
            public string email_body { get; set; }
            public string email_subject { get; set; }
            public string person_name { get; set; } 
        }

        public static List<EmailRule> LoadEmailRule(string act, string pa, string detail, int id, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_admin_supplement_loaddata", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", System.Data.SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@pa", System.Data.SqlDbType.VarChar).Value = pa;
            cmd.Parameters.Add("@detail", System.Data.SqlDbType.VarChar).Value = detail;
            cmd.Parameters.Add("@id", System.Data.SqlDbType.Int).Value = id;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;
            conn.Open();

            var emailRule = new List<EmailRule>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                emailRule.Add(new EmailRule
                {
                    pa = rdr["pa"]?.ToString(),
                    email_to = rdr["email_to"]?.ToString(),
                    email_cc= rdr["email_cc"]?.ToString(),
                    email_template_id= rdr["email_template_id"]?.ToString(),
                    email_template_name = rdr["email_template_name"]?.ToString(),
                    email_subject = rdr["email_subject"]?.ToString(),
                    email_body = rdr["email_body"]?.ToString(),
                    person_name = rdr["person_name"]?.ToString(),
                    start_date = rdr["start_date"]?.ToString(),
                    end_date = rdr["end_date"]?.ToString()
                });
            }
            rdr.Close();
            conn.Close();

            return emailRule;
        }

    }
}