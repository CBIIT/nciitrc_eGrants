using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using egrants_new.Integration.Shared;
using egrants_new.Integration.WebServices;
using egrants_new.Integration.EmailRulesEngine.Models;

using Newtonsoft.Json.Linq;

namespace egrants_new.Integration.EmailRulesEngine
{
    public class EmailIntegrationRepository
    {
        //Look at the data persistence utilized throughout the application possible ways to standardize
        //private DBContext db; 
        private readonly string _conx;


        public EmailIntegrationRepository()
        {
            _conx = ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString;
            //Any possible init needed in the constructor???
        }


        public void SaveRule(EmailRule emailRule)
        {
            using (SqlConnection conn = new System.Data.SqlClient.SqlConnection(_conx))
            {
                try
                {
                    SqlCommand cmd =
                        new SqlCommand("sp_email_save_actionresult", conn)
                        {
                            CommandType = CommandType.StoredProcedure,
                        };

                    //cmd.Parameters.Add("@emailruleid", SqlDbType.Int).Value = .RuleId;
                    //cmd.Parameters.Add("@EmailMessageid", SqlDbType.Int).Value = result.MessageId;
                    //cmd.Parameters.Add("@actionid", SqlDbType.Int).Value = result.ActionId;
                    //cmd.Parameters.Add("@actioncompleted", SqlDbType.Bit).Value = result.ActionCompleted;
                    //cmd.Parameters.Add("@successful", SqlDbType.Bit).Value = result.Successful;
                    //cmd.Parameters.Add("@actionmessage", SqlDbType.VarChar).Value = result.ActionMessage;
                    //cmd.Parameters.Add("@actionstarted", SqlDbType.Bit).Value = result.ActionStarted;
                    //cmd.Parameters.Add("@errorexception", SqlDbType.VarChar).Value = result.ErrorException.ToString();
                    //cmd.Parameters.Add("@createdate", SqlDbType.DateTimeOffset).Value = result.CreatedDate;
                    //cmd.Parameters.Add("@actiondata", SqlDbType.VarChar).Value = result.ActionDataPassed;
                    //conn.Open();

                    cmd.ExecuteNonQuery();

                }
                catch (Exception ex)
                {
                    //TODO: Handle exception
                }

            }


        }



        public void SaveActionResult(EmailRuleActionResult result)
        {
            using (SqlConnection conn = new System.Data.SqlClient.SqlConnection(_conx))
            {

                try
                {
                    SqlCommand cmd =
                        new SqlCommand("sp_email_save_actionresult", conn)
                        {
                            CommandType = CommandType.StoredProcedure,
                        };

                    string exceptionMessage = string.Empty;

                    if (result.ErrorException != null)
                    {
                        exceptionMessage = result.ErrorException.Message;
                    }

                    cmd.Parameters.Add("@emailruleid", SqlDbType.Int).Value = result.RuleId;
                    cmd.Parameters.Add("@EmailMessageid", SqlDbType.Int).Value = result.MessageId;
                    cmd.Parameters.Add("@actionid", SqlDbType.Int).Value = result.ActionId;
                    cmd.Parameters.Add("@actioncompleted", SqlDbType.Bit).Value = result.ActionCompleted;
                    cmd.Parameters.Add("@successful", SqlDbType.Bit).Value = result.Successful;
                    cmd.Parameters.Add("@actionmessage", SqlDbType.VarChar).Value = result.ActionMessage ?? string.Empty;
                    cmd.Parameters.Add("@actionstarted", SqlDbType.Bit).Value = result.ActionStarted;
                    cmd.Parameters.Add("@errorexception", SqlDbType.VarChar).Value = exceptionMessage;
                    cmd.Parameters.Add("@createdate", SqlDbType.DateTimeOffset).Value = result.CreatedDate;
                    cmd.Parameters.Add("@actiondata", SqlDbType.VarChar).Value = result.ActionDataPassed ?? string.Empty;
                    conn.Open();

                    cmd.ExecuteNonQuery();

                }
                catch (Exception ex)
                {
                    //TODO: Handle exception
                }

            }


        }


        public void SaveRuleMessageMatch(EmailMsg msg, EmailRule rule, bool matched)
        {

            using (SqlConnection conn = new System.Data.SqlClient.SqlConnection(_conx))
            {
                try
                {
                    SqlCommand cmd =
                        new SqlCommand("sp_email_save_matches", conn)
                        {
                            CommandType = CommandType.StoredProcedure,
                        };

                    cmd.Parameters.Add("@emailruleid", SqlDbType.Int).Value = rule.Id;
                    cmd.Parameters.Add("@EmailMessageid", SqlDbType.VarChar).Value = msg.Id;
                    cmd.Parameters.Add("@createddate", SqlDbType.DateTime).Value = DateTime.Now.ToShortTimeString();
                    cmd.Parameters.Add("@actionscompleted", SqlDbType.Bit).Value = 0;
                    cmd.Parameters.Add("@matched", SqlDbType.Bit).Value = matched ? 1 : 0;

                    conn.Open();
                    cmd.ExecuteNonQuery();

                }
                catch (Exception ex)
                {
                    //TODO: Handle exception
                }

            }
        }


        public void SaveRuleMatch(EmailRuleMatchedMessages match)
        {

            using (SqlConnection conn = new System.Data.SqlClient.SqlConnection(_conx))
            {
                try
                {
                    SqlCommand cmd =
                        new SqlCommand("sp_email_update_matched_messages", conn)
                        {
                            CommandType = CommandType.StoredProcedure,
                        };

                    cmd.Parameters.Add("@emailruleid", SqlDbType.Int).Value = match.EmailRuleId;
                    cmd.Parameters.Add("@EmailMessageid", SqlDbType.VarChar).Value = match.EmailMessageId;
                    cmd.Parameters.Add("@actionscompleted", SqlDbType.Bit).Value = match.ActionsCompleted;
                    cmd.Parameters.Add("@matched", SqlDbType.Bit).Value = match.Matched;

                    conn.Open();
                    cmd.ExecuteNonQuery();

                }
                catch (Exception ex)
                {
                    //TODO: Handle exception
                }

            }
        }



        public List<EmailMsg> GetEmailMessages(EmailRule rule)
        {
            var output = new List<EmailMsg>();

            using (SqlConnection conn = new System.Data.SqlClient.SqlConnection(_conx))
            {
                try
                {
                    SqlCommand cmd =
                        new SqlCommand("sp_email_get_messages", conn)
                        {
                            CommandType = CommandType.StoredProcedure,
                        };
                    cmd.Parameters.Add("@ruleid", SqlDbType.Int).Value = rule.Id;
                    conn.Open();

                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        var obj = new EmailMsg();
                        SqlHelper.MapDataToObject(obj, dr);
                        output.Add(obj);
                    }

                }
                catch (Exception ex)
                {
                    //TODO: Handle exception
                }
            }
            return output;

        }

        public EmailMsg GetEmailMessage(int messageId)
        {
            EmailMsg output = null; // = new EmailMessage();

            using (SqlConnection conn = new System.Data.SqlClient.SqlConnection(_conx))
            {
                try
                {
                    SqlCommand cmd =
                        new SqlCommand("sp_email_get_message", conn)
                        {
                            CommandType = CommandType.StoredProcedure,
                        };
                    cmd.Parameters.Add("@msgid", SqlDbType.Int).Value = messageId;
                    conn.Open();

                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        var obj = new EmailMsg();
                        SqlHelper.MapDataToObject(obj, dr);
                        output = obj;
                    }

                }
                catch (Exception ex)
                {
                    //TODO: Handle exception
                }
            }
            return output;

        }


        public List<GrantEmailAttachment> GetEmailAttachments(string messageId)
        {
            var integrationRepo = new IntegrationRepository();
            List<GrantEmailAttachment> output = new List<GrantEmailAttachment>(); // = new EmailMessage();
            MicrosoftGraphOAuthService ws = (MicrosoftGraphOAuthService)integrationRepo.GetEgrantWebService(3); //Hardcoded should be changed
            string arrOfAttachments = ws.GetEmailAttachments(messageId);

            var arrayAttachments = JArray.Parse(arrOfAttachments);

            foreach (JObject attachment in arrayAttachments)
            {
                //JObject tmp = JObject.Parse(attachment);
                var att = new GrantEmailAttachment();


                att.Name = (string)attachment["name"];
                att.Id = (string)attachment["id"];
                att.Size = (int)attachment["size"];
                att.ContentBytes = (string)attachment["contentBytes"];

                output.Add(att);
            }

            return output;

        }


        public EmailRule GetEmailRuleById(int ruleId)
        {
            EmailRule output = null;

            using (SqlConnection conn = new System.Data.SqlClient.SqlConnection(_conx))
            {
                try
                {
                    SqlCommand cmd =
                        new SqlCommand($"Select * from EmailRules WHERE Id = {ruleId}", conn)
                        {
                            CommandType = CommandType.Text,
                        };
                    //cmd.Parameters.Add("@all", SqlDbType.Bit).Value = all;
                    // cmd.Parameters.Add("@webserviceid", SqlDbType.Int).Value = ep.WSEndpoint_Id;
                    conn.Open();

                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        var obj = new EmailRule();
                        SqlHelper.MapDataToObject(obj, dr);

                        LoadRule(obj);
                        output = obj;
                    }

                }
                catch (Exception ex)
                {
                    //TODO: Handle exception
                }
            }
            return output;
        }


        public List<EmailRule> GetEmailRules(bool all = false)
        {
            var output = new List<EmailRule>();

            using (SqlConnection conn = new System.Data.SqlClient.SqlConnection(_conx))
            {
                try
                {
                    SqlCommand cmd =
                        new SqlCommand("sp_email_get_rules", conn)
                        {
                            CommandType = CommandType.StoredProcedure,
                        };
                    cmd.Parameters.Add("@all", SqlDbType.Bit).Value = all;
                    // cmd.Parameters.Add("@webserviceid", SqlDbType.Int).Value = ep.WSEndpoint_Id;
                    conn.Open();

                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        var obj = new EmailRule();
                        SqlHelper.MapDataToObject(obj, dr);

                        LoadRule(obj);
                        output.Add(obj);
                    }

                }
                catch (Exception ex)
                {
                    //TODO: Handle exception
                }
            }
            return output;
        }


        private void LoadRule(EmailRule rule)
        {
            if (rule.Id <= 0)
            {
                throw new Exception("This is not a valid, existing web service");
            }

            try
            {
                rule.Criteria = GetEmailCriteria(rule.Id);
                rule.Actions = GetEmailRuleActions(rule.Id);

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public List<EmailRuleCriteria> GetEmailCriteria(int ruleId)
        {
            var output = new List<EmailRuleCriteria>();

            using (SqlConnection conn = new System.Data.SqlClient.SqlConnection(_conx))
            {
                try
                {
                    SqlCommand cmd =
                        new SqlCommand("sp_email_get_rule_criteria", conn)
                        {
                            CommandType = CommandType.StoredProcedure,
                        };
                    cmd.Parameters.Add("@ruleid", SqlDbType.Int).Value = ruleId;
                    conn.Open();

                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        var obj = new EmailRuleCriteria();
                        SqlHelper.MapDataToObject(obj, dr);
                        output.Add(obj);
                    }

                }
                catch (Exception ex)
                {
                    //TODO: Handle exception
                }
            }
            return output;

        }

        public List<EmailRuleAction> GetEmailRuleActions(int ruleId)
        {
            var output = new List<EmailRuleAction>();

            using (SqlConnection conn = new System.Data.SqlClient.SqlConnection(_conx))
            {
                try
                {
                    SqlCommand cmd =
                        new SqlCommand("sp_email_get_rule_actions", conn)
                        {
                            CommandType = CommandType.StoredProcedure,
                        };
                    cmd.Parameters.Add("@ruleid", SqlDbType.Int).Value = ruleId;
                    conn.Open();

                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        var obj = new EmailRuleAction();
                        SqlHelper.MapDataToObject(obj, dr);
                        output.Add(obj);
                    }

                }
                catch (Exception ex)
                {
                    //TODO: Handle exception
                }
            }
            return output;

        }


        public List<EmailRuleMatchedMessages> GetEmailRuleMatches(int ruleId, bool forceAll, int messageId = 0)
        {
            var output = new List<EmailRuleMatchedMessages>();

            using (SqlConnection conn = new System.Data.SqlClient.SqlConnection(_conx))
            {
                try
                {
                    SqlCommand cmd =
                        new SqlCommand("sp_email_get_matched_messages", conn)
                        {
                            CommandType = CommandType.StoredProcedure,
                        };
                    cmd.Parameters.Add("@ruleid", SqlDbType.Int).Value = ruleId;
                    cmd.Parameters.Add("@all", SqlDbType.Bit).Value = forceAll;
                    cmd.Parameters.Add("@msgId", SqlDbType.Int).Value = messageId;
                    conn.Open();

                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        var obj = new EmailRuleMatchedMessages();
                        SqlHelper.MapDataToObject(obj, dr);
                        obj.Message = GetEmailMessage(obj.EmailMessageId);
                        output.Add(obj);
                    }

                }
                catch (Exception ex)
                {
                    //TODO: Handle exception
                }
            }
            return output;

        }

        public string GetPlaceHolder(ExtractedMessageDetails details)
        {
            string output = string.Empty;

            using (SqlConnection conn = new System.Data.SqlClient.SqlConnection(_conx))
            {
                try
                {
                    SqlCommand cmd =
                        new SqlCommand("getPlaceHolder_new", conn)
                        {
                            CommandType = CommandType.StoredProcedure,
                        };
                    cmd.Parameters.Add("@PARENTAPPLID", SqlDbType.Int).Value = details.Parentapplid;
                    cmd.Parameters.Add("@pa", SqlDbType.VarChar).Value = details.Pa;
                    cmd.Parameters.Add("@Rcvd_dt", SqlDbType.SmallDateTime).Value = details.Rcvd_dt;
                    cmd.Parameters.Add("@Catname", SqlDbType.VarChar).Value = details.Catname;
                    cmd.Parameters.Add("@filetype", SqlDbType.VarChar).Value = details.Filetype;
                    cmd.Parameters.Add("@sub", SqlDbType.VarChar).Value = details.Sub;
                    cmd.Parameters.Add("@body", SqlDbType.VarChar).Value = details.Body;
                    cmd.Parameters.Add("@subcatname", SqlDbType.VarChar).Value = details.Subcatname;

                    conn.Open();

                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        output = (string)dr["ABC"];
                    }

                }
                catch (Exception ex)
                {
                    //TODO: Handle exception
                }
            }

            return output;

        }

        public int GetTempApplId(string notificationId)
        {
            //string output = string.Empty;
            int applId = 0;
            if (!string.IsNullOrWhiteSpace(notificationId))
            {

                string baseSql = "select appl_id from adsup_notification where id = {0}";
                string SQL = String.Format(baseSql, notificationId);



                using (SqlConnection conn = new System.Data.SqlClient.SqlConnection(_conx))
                {
                    try
                    {
                        SqlCommand cmd =
                            new SqlCommand(SQL, conn)
                            {
                                CommandType = CommandType.Text,
                            };

                        conn.Open();

                        SqlDataReader dr = cmd.ExecuteReader();

                        while (dr.Read())
                        {
                            applId = (int)dr["appl_id"];
                        }

                    }
                    catch (Exception ex)
                    {
                        //TODO: Handle exception
                    }
                }
            }

            return applId;
        }


        public int GetApplId(string searchtext)
        {
            //string output = string.Empty;
            int applId = 0;
            if (!string.IsNullOrWhiteSpace(searchtext))
            {

                string baseSql = "select dbo.Imm_fn_applid_match( '{0}') as applid";
                string SQL = String.Format(baseSql, searchtext);



                using (SqlConnection conn = new System.Data.SqlClient.SqlConnection(_conx))
                {
                    try
                    {
                        SqlCommand cmd =
                            new SqlCommand(SQL, conn)
                            {
                                CommandType = CommandType.Text,
                            };

                        conn.Open();

                        SqlDataReader dr = cmd.ExecuteReader();

                        while (dr.Read())
                        {
                            applId = (int)dr["applid"];
                        }

                    }
                    catch (Exception ex)
                    {
                        //TODO: Handle exception
                    }
                }
            }

            return applId;
        }

        public bool ChecklIsReply(string notificationId, string senderId)
        {

            if (!string.IsNullOrWhiteSpace(notificationId) && (!string.IsNullOrWhiteSpace(senderId)))
            {

                string baseSql = "update dbo.adsup_Notification_email_status set reply_recieved_date=GETDATE() where Notification_id={0} and email_address LIKE '%{1}%'";
                string SQL = String.Format(baseSql, notificationId, senderId);

                using (SqlConnection conn = new System.Data.SqlClient.SqlConnection(_conx))
                {
                    try
                    {
                        SqlCommand cmd =
                            new SqlCommand(SQL, conn)
                            {
                                CommandType = CommandType.Text,
                            };

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        //TODO: Handle exception
                    }
                }
            }

            return true;
        }



        public string GetPa(string subjectLine)
        {
            string baseSql = "select dbo.fn_PA_match( '{0}') as pa";
            string SQL = String.Format(baseSql, subjectLine);

            string output = string.Empty;

            using (SqlConnection conn = new System.Data.SqlClient.SqlConnection(_conx))
            {
                try
                {
                    SqlCommand cmd =
                        new SqlCommand(SQL, conn)
                        {
                            CommandType = CommandType.Text,
                        };

                    conn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        output = (string)dr["pa"];
                    }
                }
                catch (Exception ex)
                {
                    //TODO: Handle exception
                }
            }
            return output;
        }



        public List<EmailRuleActionResult> GetActionResults(int ruleId, int msgId)
        {
            string baseSql = "SELECT [Id],[ActionId],[RuleId],[MessageId],[Successful],[ActionStarted],[ActionCompleted],[ActionMessage],[ExceptionText],[CreatedDate],[ActionDataPassed] FROM[EIM].[dbo].[EmailRulesActionResults] where ruleid {0} = and messageId = {1}";
            string SQL = String.Format(baseSql, ruleId,msgId);

            var output = new List<EmailRuleActionResult>();

            using (SqlConnection conn = new System.Data.SqlClient.SqlConnection(_conx))
            {
                try
                {
                    SqlCommand cmd =
                        new SqlCommand(SQL, conn)
                        {
                            CommandType = CommandType.Text,
                        };

                    conn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        var obj = new EmailRuleActionResult();
                        SqlHelper.MapDataToObject(obj, dr);
                        output.Add(obj);
                    }
                }
                catch (Exception ex)
                {
                    //TODO: Handle exception
                }
            }
            return output;
        }


        public List<ViewEmailActionResults> GetActionResultsII(int ruleId, int msgId = 0)
        {
            string baseSql = @"Select erar.RuleId, er.Description, erar.MessageId, erar.ActionCompleted, era.Description, erar.ActionMessage, erar.ExceptionText, erar.CreatedDate as ActionTriggered 
                               from  EmailRulesActionResults erar 
                               inner join EmailRulesActions era on era.Id = erar.ActionId
                               inner join EmailRules er on erar.RuleId = er.Id
                               where (erar.RuleId = {0} or {0}=0) and ({1}=0 or erar.MessageId={1})
                               order by  erar.Id, erar.MessageId";
            string SQL = String.Format(baseSql, ruleId, msgId);

            var output = new List<ViewEmailActionResults>();

            using (SqlConnection conn = new System.Data.SqlClient.SqlConnection(_conx))
            {
                try
                {
                    SqlCommand cmd =
                        new SqlCommand(SQL, conn)
                        {
                            CommandType = CommandType.Text,
                        };

                    conn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        var obj = new ViewEmailActionResults();
                        SqlHelper.MapDataToObject(obj, dr);
                        output.Add(obj);
                    }
                }
                catch (Exception ex)
                {
                    //TODO: Handle exception
                }
            }
            return output;
        }


        public bool GrantAttachmentPermission(string sender)
        {
            bool output = false;

            using (SqlConnection conn = new System.Data.SqlClient.SqlConnection(_conx))
            {
                try
                {
                    SqlCommand cmd =
                        new SqlCommand("sp_email_verify_attachment_permission", conn)
                        {
                            CommandType = CommandType.StoredProcedure,
                        };
                    cmd.Parameters.Add("@Sender", SqlDbType.VarChar).Value = sender;

                    conn.Open();

                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        //if there is a result then the saender was found
                        output = true;
                    }

                }
                catch (Exception ex)
                {
                    //TODO: Handle exception
                }
            }

            return output;
        }

    }
}
