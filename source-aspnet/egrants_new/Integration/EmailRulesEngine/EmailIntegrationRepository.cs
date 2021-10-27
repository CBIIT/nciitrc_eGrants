using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.EnterpriseServices;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using egrants_new.Integration.Models;
using egrants_new.Integration.Shared;
using egrants_new.Integration.WebServices;
using egrants_new.Integration.EmailRulesEngine;
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



        public void SaveActionResult(EmailRuleActionResult result)
        {



        }


        public void SaveRuleMatch(EmailMessage msg, EmailRule rule, bool matched)
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
                    cmd.Parameters.Add("@emailmessageid", SqlDbType.VarChar).Value = msg.Id;
                    cmd.Parameters.Add("@createddate", SqlDbType.DateTime).Value = DateTime.Now.ToShortTimeString();
                    cmd.Parameters.Add("@actionscompleted", SqlDbType.Bit).Value = 0;
                    cmd.Parameters.Add("@matched", SqlDbType.Bit).Value = matched ? 1:0 ;

                    conn.Open();

                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        var obj = new EmailMessage();
                        SqlHelper.MapDataToObject(obj, dr);
                    }

                }
                catch (Exception ex)
                {
                    //TODO: Handle exception
                }

            }
        }

        public List<EmailMessage> GetEmailMessages(EmailRule rule)
        {
            var output = new List<EmailMessage>();

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
                        var obj = new EmailMessage();
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

        public EmailMessage GetEmailMessage(int messageId)
        {
            var output = new EmailMessage();

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
                        var obj = new EmailMessage();
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

        public List<EmailRule> GetEmailRules()
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
                        new SqlCommand("sp_email_get_rule_action", conn)
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


        public List<EmailRuleMatchedMessages> GetEmailRuleMatches(int ruleId)
        {
            var output = new List<EmailRuleMatchedMessages>();

            using (SqlConnection conn = new System.Data.SqlClient.SqlConnection(_conx))
            {
                try
                {
                    SqlCommand cmd =
                        new SqlCommand("sp_email_get_matchedmessages", conn)
                        {
                            CommandType = CommandType.StoredProcedure,
                        };
                    cmd.Parameters.Add("@ruleid", SqlDbType.Int).Value = ruleId;
                    conn.Open();

                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        var obj = new EmailRuleMatchedMessages();
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

    }
}
