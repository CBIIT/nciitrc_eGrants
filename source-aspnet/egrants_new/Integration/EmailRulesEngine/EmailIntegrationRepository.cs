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



        public void SaveHistory(WebServiceHistory history)
        {

            using (SqlConnection conn = new System.Data.SqlClient.SqlConnection(_conx))
            {
                try
                {
                    SqlCommand cmd =
                        new SqlCommand("sp_web_service_save_history", conn)
                        {
                            CommandType = CommandType.StoredProcedure,
                        };
                    cmd.Parameters.Add("@WSEndpoint_Id", SqlDbType.Int).Value = history.WebService.WSEndpoint_Id;
                    cmd.Parameters.Add("@Result", SqlDbType.VarChar).Value = history.Result ?? "Error";
                    cmd.Parameters.Add("@ResultStatusCode", SqlDbType.Int).Value = (int)history.ResultStatusCode;
                    cmd.Parameters.Add("@DateTriggered", SqlDbType.DateTimeOffset).Value = history.DateTriggered;
                    cmd.Parameters.Add("@DateCompleted", SqlDbType.DateTimeOffset).Value = history.DateCompleted;
                    cmd.Parameters.Add("@WebServiceName", SqlDbType.VarChar).Value = history.WebServiceName;
                    cmd.Parameters.Add("@EndpointUriSent", SqlDbType.VarChar).Value = history.EndpointUriSent;
                    cmd.Parameters.Add("@ExceptionMessage", SqlDbType.VarChar).Value = history.ExceptionMessage;
                    conn.Open();

                    cmd.ExecuteNonQuery();

                }
                catch (Exception ex)
                {
                    //throw ex;
                    //todo: handle exception
                }
            }
        }


        public List<WebServiceHistory> GetExceptions()
        {
            List<WebServiceHistory> histories = new List<WebServiceHistory>();

            using (SqlConnection conn = new System.Data.SqlClient.SqlConnection(_conx))
            {
                try
                {
                    SqlCommand cmd =
                        new SqlCommand("sp_web_service_get_history_exceptions", conn)
                        {
                            CommandType = CommandType.StoredProcedure,
                        };

                    conn.Open();

                    SqlDataReader dr = cmd.ExecuteReader();
                    WebServiceEndPoint ep = new WebServiceEndPoint();

                    while (dr.Read())
                    {
                        var history = new WebServiceHistory();
                        SqlHelper.MapDataToObject(history, dr);
                        histories.Add(history);
                    }
                }
                catch (Exception ex)
                {
                    //throw ex;
                    //todo: handle exception
                }

                return histories;
            }
        }


        public List<SQLJobError> GetSQLJobErrors()
        {
            List<SQLJobError> errors = new List<SQLJobError>();

            using (SqlConnection conn = new System.Data.SqlClient.SqlConnection(_conx))
            {
                try
                {
                    SqlCommand cmd =
                        new SqlCommand("sp_web_service_get_SQLJobErrors", conn)
                        {
                            CommandType = CommandType.StoredProcedure,
                        };

                    conn.Open();

                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        var sqlJobError = new SQLJobError();
                        SqlHelper.MapDataToObject(sqlJobError, dr);
                        errors.Add(sqlJobError);
                    }
                }
                catch (Exception ex)
                {
                    //throw ex;
                    //todo: handle exception
                }

                return errors;
            }
        }

        private void UpdateWebServiceScheduleInfo(WebServiceHistory history)
        {
            using (SqlConnection conn = new System.Data.SqlClient.SqlConnection(_conx))
            {

                //Save the updates for schedule
                SqlCommand cmd =
                    new SqlCommand("sp_web_service_save_schedule_updates", conn)
                    {
                        CommandType = CommandType.StoredProcedure,
                    };
                conn.Open();
                cmd.Parameters.Add("@WSEndpoint_Id", SqlDbType.Int).Value = history.WebService.WSEndpoint_Id;
                cmd.Parameters.Add("@NextRun", SqlDbType.DateTimeOffset).Value = history.WebService.NextRun;
                cmd.Parameters.Add("@LastRun", SqlDbType.DateTimeOffset).Value = history.WebService.LastRun;

                cmd.ExecuteNonQuery();
            }
        }



        public IEnumerable<IEgrantWebService> GetEgrantWebServiceDueToFire()
        {

            List<IEgrantWebService> listEndPoints = new List<IEgrantWebService>();

            using (SqlConnection conn = new System.Data.SqlClient.SqlConnection(_conx))
            {
                try
                {
                    SqlCommand cmd =
                        new SqlCommand("sp_web_service_get_endpoint_due_to_fire", conn)
                        {
                            CommandType = CommandType.StoredProcedure,
                        };

                    conn.Open();

                    SqlDataReader dr = cmd.ExecuteReader();
                    WebServiceEndPoint ep = new WebServiceEndPoint();
                    while (dr.Read())
                    {
                        IEgrantWebService ws = GetEgrantWebService((int)dr["WSEndpoint_Id"]);
                        listEndPoints.Add(ws);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                    //todo: handle exception
                }
            }

            return listEndPoints.AsEnumerable();
        }



        public IEgrantWebService GetEgrantWebService(int serviceId)
        {
            IEgrantWebService ws = null;

            using (SqlConnection conn = new System.Data.SqlClient.SqlConnection(_conx))
            {
                try
                {
                    SqlCommand cmd =
                        new SqlCommand("sp_web_service_get_endpoint", conn)
                        {
                            CommandType = CommandType.StoredProcedure,
                        };
                    cmd.Parameters.Add("@webserviceid", SqlDbType.Int).Value = serviceId;
                    conn.Open();

                    SqlDataReader dr = cmd.ExecuteReader();
                    WebServiceEndPoint ep = new WebServiceEndPoint();
                    while (dr.Read())
                    {
                        SqlHelper.MapDataToObject(ep, dr);
                    }

                    //TODO: Rename Querystring to Parameter List
                    string paramString = ep.QueryString;
                    foreach (string param in paramString.Split('&'))
                    {
                        var param_txt = param.Split('=');
                        ep.Params.Add(new WebServiceParam()
                        {
                            Name = param_txt[0],
                            Value = param_txt[1]
                        });
                    }

                    //LoadWebServiceNodeMappings(ep);

                    conn.Close();
                    switch (ep.AuthenticationType)
                    {
                        case IntegrationEnums.AuthenticationType.Certificate:
                            ws = new CertAuthWebService(ep);
                            //ws.WebService = ep;
                            break;
                        case IntegrationEnums.AuthenticationType.UserPassword:
                            throw new Exception("User Password Auth Type Not Implemented");
                            break;
                        case IntegrationEnums.AuthenticationType.OAuth:
                            ws = new MicrosoftGraphOAuthService(ep);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error creating WebService");
                }
                return ws;
            }
        }

        private void Load(WebServiceEndPoint ep)
        {
            if (ep.WSEndpoint_Id <= 0)
            {
                throw new Exception("This is not a valid, existing web service");
            }

            using (SqlConnection conn = new System.Data.SqlClient.SqlConnection(_conx))
            {
                try
                {
                    SqlCommand cmd =
                        new SqlCommand("sp_web_service_get_node_mapping", conn)
                        {
                            CommandType = CommandType.StoredProcedure,
                        };
                    cmd.Parameters.Add("@webserviceid", SqlDbType.Int).Value = ep.WSEndpoint_Id;
                    conn.Open();

                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        WSNodeMapping nodeMap = new WSNodeMapping();
                        SqlHelper.MapDataToObject(nodeMap, dr);
                        ep.NodeMappings.Add(nodeMap);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

        }

        public void MarkHistorySent(WebServiceHistory history)
        {
            using (SqlConnection conn = new System.Data.SqlClient.SqlConnection(_conx))
            {
                try
                {
                    conn.Open();
                    //Save the updates for schedule
                    SqlCommand cmd =
                        new SqlCommand("sp_web_service_update_history", conn)
                        {
                            CommandType = CommandType.StoredProcedure,
                        };
                    cmd.Parameters.Add("@WSHistory_Id", SqlDbType.Int).Value = history.WSHistory_Id;
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    //TODO: Handle exception
                }

            }
        }

        public void MarkSqlJobErrorSent(SQLJobError error)
        {
            using (SqlConnection conn = new System.Data.SqlClient.SqlConnection(_conx))
            {
                try
                {
                    conn.Open();
                    //Save the updates for schedule
                    SqlCommand cmd =
                        new SqlCommand("sp_web_service_update_SQLJobError", conn)
                        {
                            CommandType = CommandType.StoredProcedure,
                        };
                    cmd.Parameters.Add("@Error_Id", SqlDbType.Int).Value = error.ErrorId;
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



        }


        public void SaveRuleMatch(EmailMessage msg, EmailRule rule)
        {


        }

        public List<EmailMessage> GetEmailMessages()
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
                   // cmd.Parameters.Add("@webserviceid", SqlDbType.Int).Value = ep.WSEndpoint_Id;
                    conn.Open();

                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        EmailMessage msg = new EmailMessage();
                        SqlHelper.MapDataToObject(msg, dr);
                        output.Add(msg);
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
                        EmailRule rule = new EmailRule();
                        SqlHelper.MapDataToObject(rule, dr);
                        output.Add(rule);
                    }

                }
                catch (Exception ex)
                {
                    //TODO: Handle exception
                }
            }
            return output;
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
                        EmailRuleCriteria criteria = new EmailRuleCriteria();
                        SqlHelper.MapDataToObject(criteria, dr);
                        output.Add(criteria);
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
