#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  Supplement.cs
// Solution: egrants_new
// Project:  egrants_new
// Created: 2022-05-05
// Contributors:
//      - Briggs, Robin (NIH/NCI) [C] - briggsr2
//      -
// Copyright (c) National Institute of Health
// 
// <Description of the file>
// 
// This source is subject to the NIH Softwre License.
// See https://ncihub.org/resources/899/download/Guidelines_for_Releasing_Research_Software_04062015.pdf
// All other rights reserved.
// 
// THE SOFTWARE IS PROVIDED "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT ARE DISCLAIMED. IN NO EVENT SHALL THE NATIONAL
// CANCER INSTITUTE (THE PROVIDER), THE NATIONAL INSTITUTES OF HEALTH, THE
// U.S. GOVERNMENT OR THE INDIVIDUAL DEVELOPERS BE LIABLE FOR ANY DIRECT,
// INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
// ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
// \***************************************************************************/

#endregion

#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

#endregion

namespace egrants_new.Egrants_Admin.Models
{
    /// <summary>
    /// The supplement.
    /// </summary>
    public class Supplement
    {

        /// <summary>
        /// The load notifications.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="pa">
        /// The pa.
        /// </param>
        /// <param name="detail">
        /// The detail.
        /// </param>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<Notifications> LoadNotifications(string act, string pa, string detail, int id, string ic, string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("sp_web_admin_supplement_loaddata", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@pa", SqlDbType.VarChar).Value = pa;
            cmd.Parameters.Add("@detail", SqlDbType.VarChar).Value = detail;
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;
            conn.Open();

            var Notification = new List<Notifications>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                Notification.Add(
                    new Notifications
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

            rdr.Close();
            conn.Close();

            return Notification;
        }

        /// <summary>
        /// The review notifications.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="pa">
        /// The pa.
        /// </param>
        /// <param name="detail">
        /// The detail.
        /// </param>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<NotificationStatus> ReviewNotifications(string act, string pa, string detail, int id, string ic, string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("sp_web_admin_supplement_loaddata", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@pa", SqlDbType.VarChar).Value = pa;
            cmd.Parameters.Add("@detail", SqlDbType.VarChar).Value = detail;
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;

            // cmd.Parameters["@return_notice"].Direction = System.Data.ParameterDirection.Output;
            conn.Open();

            var NotificationStatus = new List<NotificationStatus>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                NotificationStatus.Add(
                    new NotificationStatus
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

            rdr.Close();
            conn.Close();

            return NotificationStatus;
        }

        /// <summary>
        /// The review email status.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<EmailStatus> ReviewEmailStatus(int id)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);

            var cmd = new SqlCommand(
                "select id,email as email_type, position, person_name,email_address,convert(varchar, email_date, 101) as email_date, "
              + " convert(varchar, created_date, 101) as created_date, email_send_status, convert(varchar, reply_recieved_date, 101) as reply_recieved_date, reply_status "
              + " from adsup_Notification_email_status where Notification_id = @id",
                conn);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

            conn.Open();

            var EmailStatus = new List<EmailStatus>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                EmailStatus.Add(
                    new EmailStatus
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

            rdr.Close();
            conn.Close();

            return EmailStatus;
        }

        /// <summary>
        /// The load email position list.
        /// </summary>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<EmailPositions> LoadEmailPositionList()
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);

            var cmd = new SqlCommand(
                "select distinct ltrim(rtrim(email_position_code)) as email_position_code from dbo.adsup_email_position_master order by email_position_code",
                conn);

            cmd.CommandType = CommandType.Text;

            conn.Open();

            var EmailPositionList = new List<EmailPositions>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                EmailPositionList.Add(new EmailPositions { email_position_code = rdr["email_position_code"]?.ToString() });

            rdr.Close();
            conn.Close();

            return EmailPositionList;
        }

        /// <summary>
        /// The get notice.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="pa">
        /// The pa.
        /// </param>
        /// <param name="detail">
        /// The detail.
        /// </param>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="subject">
        /// The subject.
        /// </param>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetNotice(string act, string pa, string detail, int id, string name, string subject, string ic, string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("sp_web_admin_supplement_modify", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@pa", SqlDbType.VarChar).Value = pa;
            cmd.Parameters.Add("@detail", SqlDbType.VarChar).Value = detail;
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
            cmd.Parameters.Add("@name", SqlDbType.VarChar).Value = name;
            cmd.Parameters.Add("@subject", SqlDbType.VarChar).Value = subject;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;
            cmd.Parameters.Add("@return_notice", SqlDbType.VarChar, 200);
            cmd.Parameters["@return_notice"].Direction = ParameterDirection.Output;

            conn.Open();
            var DataReader = cmd.ExecuteReader();

            DataReader.Close();
            conn.Close();

            var return_notice = Convert.ToString(cmd.Parameters["@return_notice"].Value);

            return return_notice;
        }

        /// <summary>
        /// The load email templates.
        /// </summary>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<EmailTemplates> LoadEmailTemplates()
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);

            var cmd = new SqlCommand(
                "SELECT ltrim(rtrim(template_name)) as template_name,id,[subject] as subject, dbo.fn_clean_characters(body) as body,"
              + "created_by_person_id,convert(varchar,created_date,101) as created_date FROM dbo.adsup_email_master order by template_name",
                conn);

            cmd.CommandType = CommandType.Text;

            conn.Open();

            var EmailTemplate = new List<EmailTemplates>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                EmailTemplate.Add(
                    new EmailTemplates
                        {
                            id = rdr["id"]?.ToString(),
                            template_name = rdr["template_name"]?.ToString(),
                            subject = rdr["subject"]?.ToString(),
                            body = rdr["body"]?.ToString(),
                            created_date = rdr["created_date"]?.ToString(),
                            created_by_person_id = rdr["created_by_person_id"]?.ToString()
                        });

            rdr.Close();
            conn.Close();

            return EmailTemplate;
        }

        /// <summary>
        /// The load email rules list.
        /// </summary>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<EmailRules> LoadEmailRulesList()
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);

            var cmd = new SqlCommand(
                "SELECT id, ltrim(rtrim(pa)) as pa FROM dbo.adsup_email_rules WHERE end_date is null order by ltrim(rtrim(pa))",
                conn); /*and email_template_id is not null*/

            cmd.CommandType = CommandType.Text;

            conn.Open();

            var EmailRules = new List<EmailRules>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                EmailRules.Add(new EmailRules { id = rdr["id"]?.ToString(), pa = rdr["pa"]?.ToString() });

            rdr.Close();
            conn.Close();

            return EmailRules;
        }

        /// <summary>
        /// The load email rule.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="pa">
        /// The pa.
        /// </param>
        /// <param name="detail">
        /// The detail.
        /// </param>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<EmailRule> LoadEmailRule(string act, string pa, string detail, int id, string ic, string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("sp_web_admin_supplement_loaddata", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@pa", SqlDbType.VarChar).Value = pa;
            cmd.Parameters.Add("@detail", SqlDbType.VarChar).Value = detail;
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;
            conn.Open();

            var emailRule = new List<EmailRule>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                emailRule.Add(
                    new EmailRule
                        {
                            pa = rdr["pa"]?.ToString(),
                            email_to = rdr["email_to"]?.ToString(),
                            email_cc = rdr["email_cc"]?.ToString(),
                            email_template_id = rdr["email_template_id"]?.ToString(),
                            email_template_name = rdr["email_template_name"]?.ToString(),
                            email_subject = rdr["email_subject"]?.ToString(),
                            email_body = rdr["email_body"]?.ToString(),
                            person_name = rdr["person_name"]?.ToString(),
                            start_date = rdr["start_date"]?.ToString(),
                            end_date = rdr["end_date"]?.ToString()
                        });

            rdr.Close();
            conn.Close();

            return emailRule;
        }

        /// <summary>
        /// The notifications.
        /// </summary>
        public class Notifications
        {
            /// <summary>
            /// Gets or sets the id.
            /// </summary>
            public string id { get; set; }

            /// <summary>
            /// Gets or sets the full_grant_num.
            /// </summary>
            public string full_grant_num { get; set; }

            /// <summary>
            /// Gets or sets the appl_id.
            /// </summary>
            public string appl_id { get; set; }

            /// <summary>
            /// Gets or sets the pa.
            /// </summary>
            public string pa { get; set; }

            /// <summary>
            /// Gets or sets the subject line.
            /// </summary>
            public string subjectLine { get; set; }

            /// <summary>
            /// Gets or sets the notification body.
            /// </summary>
            public string NotificationBody { get; set; }

            /// <summary>
            /// Gets or sets the not rcvd_dt.
            /// </summary>
            public string NotRcvd_dt { get; set; }

            /// <summary>
            /// Gets or sets the created_date.
            /// </summary>
            public string created_date { get; set; }
        }

        /// <summary>
        /// The notification status.
        /// </summary>
        public class NotificationStatus
        {
            /// <summary>
            /// Gets or sets the tag.
            /// </summary>
            public string tag { get; set; }

            /// <summary>
            /// Gets or sets the full_grant_num.
            /// </summary>
            public string full_grant_num { get; set; }

            /// <summary>
            /// Gets or sets the pa.
            /// </summary>
            public string pa { get; set; }

            /// <summary>
            /// Gets or sets the id.
            /// </summary>
            public string id { get; set; }

            /// <summary>
            /// Gets or sets the document_id.
            /// </summary>
            public string document_id { get; set; }

            /// <summary>
            /// Gets or sets the document_date.
            /// </summary>
            public string document_date { get; set; }

            /// <summary>
            /// Gets or sets the url.
            /// </summary>
            public string url { get; set; }

            /// <summary>
            /// Gets or sets the category_name.
            /// </summary>
            public string category_name { get; set; }
        }

        /// <summary>
        /// The email status.
        /// </summary>
        public class EmailStatus
        {
            /// <summary>
            /// Gets or sets the id.
            /// </summary>
            public string id { get; set; }

            /// <summary>
            /// Gets or sets the email_type.
            /// </summary>
            public string email_type { get; set; }

            /// <summary>
            /// Gets or sets the email_address.
            /// </summary>
            public string email_address { get; set; }

            /// <summary>
            /// Gets or sets the email_date.
            /// </summary>
            public string email_date { get; set; }

            /// <summary>
            /// Gets or sets the email_send_status.
            /// </summary>
            public string email_send_status { get; set; }

            /// <summary>
            /// Gets or sets the position.
            /// </summary>
            public string position { get; set; }

            /// <summary>
            /// Gets or sets the person_name.
            /// </summary>
            public string person_name { get; set; }

            /// <summary>
            /// Gets or sets the created_date.
            /// </summary>
            public string created_date { get; set; }

            /// <summary>
            /// Gets or sets the reply_status.
            /// </summary>
            public string reply_status { get; set; }

            /// <summary>
            /// Gets or sets the reply_recieved_date.
            /// </summary>
            public string reply_recieved_date { get; set; }
        }

        /// <summary>
        /// The email positions.
        /// </summary>
        public class EmailPositions
        {
            /// <summary>
            /// Gets or sets the email_position_code.
            /// </summary>
            public string email_position_code { get; set; }
        }

        /// <summary>
        /// The email templates.
        /// </summary>
        public class EmailTemplates
        {
            /// <summary>
            /// Gets or sets the id.
            /// </summary>
            public string id { get; set; }

            /// <summary>
            /// Gets or sets the template_name.
            /// </summary>
            public string template_name { get; set; }

            /// <summary>
            /// Gets or sets the body.
            /// </summary>
            public string body { get; set; }

            /// <summary>
            /// Gets or sets the subject.
            /// </summary>
            public string subject { get; set; }

            /// <summary>
            /// Gets or sets the created_date.
            /// </summary>
            public string created_date { get; set; }

            /// <summary>
            /// Gets or sets the created_by_person_id.
            /// </summary>
            public string created_by_person_id { get; set; }
        }

        /// <summary>
        /// The email rules.
        /// </summary>
        public class EmailRules
        {
            /// <summary>
            /// Gets or sets the id.
            /// </summary>
            public string id { get; set; }

            /// <summary>
            /// Gets or sets the pa.
            /// </summary>
            public string pa { get; set; }
        }

        /// <summary>
        /// The email rule.
        /// </summary>
        public class EmailRule
        {
            /// <summary>
            /// Gets or sets the pa.
            /// </summary>
            public string pa { get; set; }

            /// <summary>
            /// Gets or sets the email_to.
            /// </summary>
            public string email_to { get; set; }

            /// <summary>
            /// Gets or sets the email_cc.
            /// </summary>
            public string email_cc { get; set; }

            /// <summary>
            /// Gets or sets the start_date.
            /// </summary>
            public string start_date { get; set; }

            /// <summary>
            /// Gets or sets the end_date.
            /// </summary>
            public string end_date { get; set; }

            /// <summary>
            /// Gets or sets the email_template_id.
            /// </summary>
            public string email_template_id { get; set; }

            /// <summary>
            /// Gets or sets the email_template_name.
            /// </summary>
            public string email_template_name { get; set; }

            /// <summary>
            /// Gets or sets the email_body.
            /// </summary>
            public string email_body { get; set; }

            /// <summary>
            /// Gets or sets the email_subject.
            /// </summary>
            public string email_subject { get; set; }

            /// <summary>
            /// Gets or sets the person_name.
            /// </summary>
            public string person_name { get; set; }
        }
    }
}