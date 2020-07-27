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
using System.Xml.Linq;
using egrants_new.Integration.Models;
using Newtonsoft.Json.Linq;

namespace egrants_new.Integration.WebServices
{
    public class IntegrationRepository
    {
        //Look at the data persistence utilized throughout the application possible ways to standardize
        //private DBContext db; 
        private readonly string _conx;


        public IntegrationRepository()
        {
            _conx = ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString;
            //Any possible init needed in the constructor???
        }


        public void SaveData(WebServiceHistory history)
        {
            try
            {
                SaveData(history.Result, history.WebService);
            }
            catch (Exception ex)
            {
                var nl = Environment.NewLine;
                history.ExceptionMessage +=
                    $"{nl}Exception in Integration Repository SaveData method.{nl + ex.Message + nl + ex.StackTrace}";
            }

            SaveHistory(history);
            UpdateWebServiceScheduleInfo(history);

        }

        public void SaveData(string json, WebServiceEndPoint webService)
        {
            //Usually not recommended, but utilizing dynamic SQL here will make sure that the generic web service 
            //will be as versatile as needed. 
            //Ensure that the incoming data, although from a trusted source is checked to prevent SQL injection attack
            //and prevent remote code execution
            if (!string.IsNullOrWhiteSpace(json))
            {
                //First build data list
                var records = JArray.Parse(json);


                //TODO:  If there are no records, then skip and update that the job ran and return

                //Read the destination table's data structure, quick select 
                string columnList = String.Join(", ", webService.NodeMappings.Select(map => map.DestinationField));

                string strQueryForTable =
                    $"Select top 1 {columnList} from {webService.Database}.{webService.Schema}.{webService.DestinationTable}";

                //Find the Connection String for the database destination of the import data

                DataTable tbl = new DataTable();

                using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(_conx))
                {
                    try
                    {
                        System.Data.SqlClient.SqlCommand cmd =
                            new System.Data.SqlClient.SqlCommand(strQueryForTable, conn);
                        cmd.CommandType = CommandType.Text;
                        conn.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(tbl);
                        conn.Close();
                    }
                    catch (Exception ex)
                    {
                        //TODO:  Handle Exception
                        throw new Exception("Failed to retrieve destination table schema.  Verify Mapping data");
                    }
                    finally
                    {
                        conn.Close();
                    }
                }

                List<DataRow> rows = new List<DataRow>();


                foreach (JObject obj in records.Children<JObject>())
                {
                    DataRow row = tbl.NewRow();
                    foreach (WSNodeMapping nodeMapping in webService.NodeMappings)
                    {
                        //TODO: Later implement data transformation or reconciliation 
                        //TODO: Handle DataTypes
                        if (tbl.Columns[nodeMapping.DestinationField].DataType ==
                            System.Type.GetType("System.DateTime"))
                        {
                            string dateValue = (string)obj[nodeMapping.NodeName];
                            dateValue = dateValue.Replace('T', ' ');
                            DateTimeOffset date = DateTimeOffset.Parse(dateValue);
                            row[nodeMapping.DestinationField] = date.LocalDateTime;
                        }
                        else if (tbl.Columns[nodeMapping.DestinationField].DataType.ToString().ToLower()
                            .Contains("int"))
                        {
                            row[nodeMapping.DestinationField] = (int)obj[nodeMapping.NodeName];
                        }
                        else
                        {
                            row[nodeMapping.DestinationField] = obj[nodeMapping.NodeName];
                        }
                    }

                    rows.Add(row);
                }

                using (SqlBulkCopy bulkCopy =
                    new SqlBulkCopy(_conx))
                {
                    bulkCopy.DestinationTableName = webService.DestinationTable;
                    foreach (WSNodeMapping node in webService.NodeMappings)
                    {
                        bulkCopy.ColumnMappings.Add(node.DestinationField, node.DestinationField);
                    }

                    try
                    {
                        // Write from the source to the destination.
                        bulkCopy.WriteToServer(rows.ToArray());
                        bulkCopy.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
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
                        MapDataToObject(history, dr);
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
                        this.MapDataToObject(ep, dr);
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

                    LoadWebServiceNodeMappings(ep);

                    conn.Close();
                    switch (ep.AuthenticationType)
                    {
                        case Enumerations.AuthenticationType.Certificate:
                            ws = new CertAuthWebService(ep);
                            //ws.WebService = ep;
                            break;
                        case Enumerations.AuthenticationType.UserPassword:
                            throw new Exception("User Password Auth Type Not Implemented");
                            break;
                        case Enumerations.AuthenticationType.OAuth:
                            throw new Exception("O Auth Type Not Implemented");
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

        private void LoadWebServiceNodeMappings(WebServiceEndPoint ep)
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
                        MapDataToObject(nodeMap, dr);
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

        private void MapDataToObject(Object obj, SqlDataReader reader)
        {
            PropertyInfo[] properties = obj.GetType().GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                try
                {
                    if (!Convert.IsDBNull(reader[prop.Name]))
                    {
                        switch (prop.PropertyType.Name.ToLower())
                        {

                            case "string":
                                prop.SetValue(obj, reader[prop.Name].ToString());
                                break;
                            case "int32":
                                prop.SetValue(obj, (int)reader[prop.Name]);
                                break;

                            case "authenticationtype":
                                prop.SetValue(obj, (Enumerations.AuthenticationType)reader[prop.Name]);
                                break;

                            case "datetimeoffset":
                                prop.SetValue(obj, (DateTimeOffset)reader[prop.Name]);
                                break;
                            case "datetimeunits":
                                prop.SetValue(obj, (Enumerations.DateTimeUnits)reader[prop.Name]);
                                break;
                            case "boolean":
                                prop.SetValue(obj, (bool)reader[prop.Name]);
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        prop.SetValue(obj, null, null);
                    }
                }
                catch (Exception ex)
                {
                    //throw ex;
                    //TODO: Handle Exception here
                }
            }

        }

    }
}
