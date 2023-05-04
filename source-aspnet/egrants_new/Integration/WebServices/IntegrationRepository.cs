#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  IntegrationRepository.cs
// Solution: egrants_new
// Project:  egrants_new
// Created: 2022-11-21
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
using System.Linq;

using egrants_new.Integration.Models;

using Newtonsoft.Json.Linq;

#endregion

namespace egrants_new.Integration.WebServices
{
    /// <summary>
    ///     The integration repository.
    /// </summary>
    public class IntegrationRepository
    {
        // Look at the data persistence utilized throughout the application possible ways to standardize
        // private DBContext db; 
        /// <summary>
        ///     The _conx.
        /// </summary>
        private readonly string _conx;

        /// <summary>
        ///     Initializes a new instance of the <see cref="IntegrationRepository" /> class.
        /// </summary>
        public IntegrationRepository()
        {
            _conx = ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString;

            // Any possible init needed in the constructor???
        }

        /// <summary>
        ///     The save data.
        /// </summary>
        /// <param name="history">
        ///     The history.
        /// </param>
        public void SaveData(WebServiceHistory history)
        {
            try
            {
                SaveData(history.Result, history.WebService);
            }
            catch (Exception ex)
            {
                var nl = Environment.NewLine;
                history.ExceptionMessage += $"{nl}Exception in Integration Repository SaveData method.{nl + ex.Message + nl + ex.StackTrace}";
            }

            try
            {
                SaveHistory(history);
            }
            catch (Exception ex)
            {
                var nl = Environment.NewLine;
                history.ExceptionMessage += $"{nl}Exception in Integration Repository SaveHistory method.{nl + ex.Message + nl + ex.StackTrace}";
            }

            UpdateWebServiceScheduleInfo(history);
        }

        /// <summary>
        ///     The save data.
        /// </summary>
        /// <param name="json">
        ///     The json.
        /// </param>
        /// <param name="webService">
        ///     The web service.
        /// </param>
        /// <exception cref="Exception">
        /// </exception>
        public void SaveData(string json, WebServiceEndPoint webService)
        {
            // Usually not recommended, but utilizing dynamic SQL here will make sure that the generic web service 
            // will be as versatile as needed. 
            // Ensure that the incoming data, although from a trusted source is checked to prevent SQL injection attack
            // and prevent remote code execution
            if (!string.IsNullOrWhiteSpace(json))
            {
                // First build data list
                var records = JArray.Parse(json);

                // TODO:  If there are no records, then skip and update that the job ran and return

                // Read the destination table's data structure, quick select 
                var columnList = string.Join(", ", webService.NodeMappings.Select(map => map.DestinationField));

                var strQueryForTable = $"Select top 1 {columnList} from {webService.Database}.{webService.Schema}.{webService.DestinationTable}";

                // Find the Connection String for the database destination of the import data
                var tbl = new DataTable();

                using (var conn = new SqlConnection(_conx))
                {
                    try
                    {
                        var cmd = new SqlCommand(strQueryForTable, conn);

                        cmd.CommandType = CommandType.Text;
                        conn.Open();
                        var da = new SqlDataAdapter(cmd);
                        da.Fill(tbl);
                    }
                    catch (Exception ex)
                    {
                        // TODO:  Handle Exception
                        throw new Exception("Failed to retrieve destination table schema.  Verify Mapping data" + ex);
                    }
                }

                var rows = new List<DataRow>();

                foreach (var obj in records.Children<JObject>())
                {
                    var row = tbl.NewRow();

                    foreach (var nodeMapping in webService.NodeMappings)

                        // TODO: Later implement data transformation or reconciliation 
                        // TODO: Handle DataTypes
                        if (tbl.Columns[nodeMapping.DestinationField].DataType == Type.GetType("System.DateTime"))
                        {
                            var dateValue = (string)obj[nodeMapping.NodeName];
                            dateValue = dateValue.Replace('T', ' ');
                            var date = DateTimeOffset.Parse(dateValue);
                            row[nodeMapping.DestinationField] = date.LocalDateTime;
                        }
                        else if (tbl.Columns[nodeMapping.DestinationField].DataType.ToString().ToLower().Contains("int"))
                        {
                            row[nodeMapping.DestinationField] = (int)obj[nodeMapping.NodeName];
                        }
                        else
                        {
                            row[nodeMapping.DestinationField] = obj[nodeMapping.NodeName];
                        }

                    rows.Add(row);
                }

                using (var bulkCopy = new SqlBulkCopy(_conx, SqlBulkCopyOptions.FireTriggers))
                {
                    bulkCopy.DestinationTableName = webService.DestinationTable;

                    foreach (var node in webService.NodeMappings)
                        bulkCopy.ColumnMappings.Add(node.DestinationField, node.DestinationField);

                    try
                    {
                        // Write from the source to the destination.
                        bulkCopy.WriteToServer(rows.ToArray());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        /// <summary>
        ///     The save history.
        /// </summary>
        /// <param name="history">
        ///     The history.
        /// </param>
        public void SaveHistory(WebServiceHistory history)
        {
            using (var conn = new SqlConnection(_conx))
            {
                var cmd = new SqlCommand("sp_web_service_save_history", conn) { CommandType = CommandType.StoredProcedure };
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
        }

        /// <summary>
        ///     The get exceptions.
        /// </summary>
        /// <returns>
        ///     The <see cref="List" />.
        /// </returns>
        public List<WebServiceHistory> GetExceptions()
        {
            var histories = new List<WebServiceHistory>();

            using (var conn = new SqlConnection(_conx))
            {
                try
                {
                    var cmd = new SqlCommand("sp_web_service_get_history_exceptions", conn) { CommandType = CommandType.StoredProcedure };

                    conn.Open();

                    var dr = cmd.ExecuteReader();
                    var ep = new WebServiceEndPoint();

                    while (dr.Read())
                    {
                        var history = new WebServiceHistory();
                        MapDataToObject(history, dr);
                        histories.Add(history);
                    }
                }
                catch (Exception ex)
                {
                    // throw ex;
                    // todo: handle exception
                }

                return histories;
            }
        }

        /// <summary>
        ///     The get sql job errors.
        /// </summary>
        /// <returns>
        ///     The <see cref="List" />.
        /// </returns>
        public List<SQLJobError> GetSQLJobErrors()
        {
            var errors = new List<SQLJobError>();

            using (var conn = new SqlConnection(_conx))
            {
                try
                {
                    var cmd = new SqlCommand("sp_web_service_get_SQLJobErrors", conn) { CommandType = CommandType.StoredProcedure };

                    conn.Open();

                    var dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        var sqlJobError = new SQLJobError();
                        MapDataToObject(sqlJobError, dr);
                        errors.Add(sqlJobError);
                    }
                }
                catch (Exception ex)
                {
                    // throw ex;
                    // todo: handle exception
                }

                return errors;
            }
        }

        /// <summary>
        ///     The update web service schedule info.
        /// </summary>
        /// <param name="history">
        ///     The history.
        /// </param>
        private void UpdateWebServiceScheduleInfo(WebServiceHistory history)
        {
            using (var conn = new SqlConnection(_conx))
            {
                // Save the updates for schedule
                var cmd = new SqlCommand("sp_web_service_save_schedule_updates", conn) { CommandType = CommandType.StoredProcedure };
                conn.Open();
                cmd.Parameters.Add("@WSEndpoint_Id", SqlDbType.Int).Value = history.WebService.WSEndpoint_Id;
                cmd.Parameters.Add("@NextRun", SqlDbType.DateTimeOffset).Value = history.WebService.NextRun;
                cmd.Parameters.Add("@LastRun", SqlDbType.DateTimeOffset).Value = history.WebService.LastRun;

                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        ///     The get egrant web service due to fire.
        /// </summary>
        /// <returns>
        ///     The <see cref="IEnumerable" />.
        /// </returns>
        public IEnumerable<IEgrantWebService> GetEgrantWebServiceDueToFire()
        {
            var listEndPoints = new List<IEgrantWebService>();

            using (var conn = new SqlConnection(_conx))
            {
                try
                {
                    var cmd = new SqlCommand("sp_web_service_get_endpoint_due_to_fire", conn) { CommandType = CommandType.StoredProcedure };

                    conn.Open();

                    var dr = cmd.ExecuteReader();
                    var ep = new WebServiceEndPoint();

                    while (dr.Read())
                    {
                        var ws = GetEgrantWebService((int)dr["WSEndpoint_Id"]);
                        listEndPoints.Add(ws);
                    }
                }
                catch (Exception ex)
                {
                    // throw ex;
                    // todo: handle exception
                }
            }

            return listEndPoints.AsEnumerable();
        }

        /// <summary>
        ///     The get egrant web service.
        /// </summary>
        /// <param name="serviceId">
        ///     The service id.
        /// </param>
        /// <returns>
        ///     The <see cref="IEgrantWebService" />.
        /// </returns>
        /// <exception cref="Exception">
        /// </exception>
        public IEgrantWebService GetEgrantWebService(int serviceId)
        {
            IEgrantWebService ws = null;

            using (var conn = new SqlConnection(_conx))
            {
                try
                {
                    var cmd = new SqlCommand("sp_web_service_get_endpoint", conn) { CommandType = CommandType.StoredProcedure };
                    cmd.Parameters.Add("@webserviceid", SqlDbType.Int).Value = serviceId;
                    conn.Open();

                    var dr = cmd.ExecuteReader();
                    var ep = new WebServiceEndPoint();

                    while (dr.Read())
                        MapDataToObject(ep, dr);

                    // TODO: Rename Querystring to Parameter List
                    var paramString = ep.QueryString;

                    foreach (var param in paramString.Split('&'))
                    {
                        var param_txt = param.Split('=');
                        ep.Params.Add(new WebServiceParam { Name = param_txt[0], Value = param_txt[1] });
                    }

                    LoadWebServiceNodeMappings(ep);

                    conn.Close();

                    switch (ep.AuthenticationType)
                    {
                        case Enumerations.AuthenticationType.Certificate:
                            ws = new CertAuthWebService(ep);

                            // ws.WebService = ep;
                            break;
                        case Enumerations.AuthenticationType.UserPassword:
                            throw new Exception("User Password Auth Type Not Implemented");

                            break;
                        case Enumerations.AuthenticationType.OAuth:
                            throw new Exception("O Auth Type Not Implemented");

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

        /// <summary>
        ///     The load web service node mappings.
        /// </summary>
        /// <param name="ep">
        ///     The ep.
        /// </param>
        /// <exception cref="Exception">
        /// </exception>
        private void LoadWebServiceNodeMappings(WebServiceEndPoint ep)
        {
            if (ep.WSEndpoint_Id <= 0)
                throw new Exception("This is not a valid, existing web service");

            using (var conn = new SqlConnection(_conx))
            {
                var cmd = new SqlCommand("sp_web_service_get_node_mapping", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@webserviceid", SqlDbType.Int).Value = ep.WSEndpoint_Id;
                conn.Open();

                var dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    var nodeMap = new WSNodeMapping();
                    MapDataToObject(nodeMap, dr);
                    ep.NodeMappings.Add(nodeMap);
                }
            }
        }

        /// <summary>
        ///     The mark history sent.
        /// </summary>
        /// <param name="history">
        ///     The history.
        /// </param>
        public void MarkHistorySent(WebServiceHistory history)
        {
            using (var conn = new SqlConnection(_conx))
            {
                try
                {
                    conn.Open();

                    // Save the updates for schedule
                    var cmd = new SqlCommand("sp_web_service_update_history", conn) { CommandType = CommandType.StoredProcedure };
                    cmd.Parameters.Add("@WSHistory_Id", SqlDbType.Int).Value = history.WSHistory_Id;
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    // TODO: Handle exception
                }
            }
        }

        /// <summary>
        ///     The mark sql job error sent.
        /// </summary>
        /// <param name="error">
        ///     The error.
        /// </param>
        public void MarkSQLJobErrorSent(SQLJobError error)
        {
            using (var conn = new SqlConnection(_conx))
            {
                try
                {
                    conn.Open();

                    // Save the updates for schedule
                    var cmd = new SqlCommand("sp_web_service_update_SQLJobError", conn) { CommandType = CommandType.StoredProcedure };
                    cmd.Parameters.Add("@Error_Id", SqlDbType.Int).Value = error.ErrorId;
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    // TODO: Handle exception
                }
            }
        }

        /// <summary>
        ///     The map data to object.
        /// </summary>
        /// <param name="obj">
        ///     The obj.
        /// </param>
        /// <param name="reader">
        ///     The reader.
        /// </param>
        private void MapDataToObject(object obj, SqlDataReader reader)
        {
            var properties = obj.GetType().GetProperties();

            foreach (var prop in properties)
                try
                {
                    if (!Convert.IsDBNull(reader[prop.Name]))
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
                        }
                    else
                        prop.SetValue(obj, null, null);
                }
                catch (Exception ex)
                {
                    // throw ex;
                    // TODO: Handle Exception here
                }
        }
    }
}