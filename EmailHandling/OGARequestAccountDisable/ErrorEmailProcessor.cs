using CommonUtilties;
using Microsoft.Office.Interop.Outlook;
using OGARequestAccountDisable.DTOs;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace OGARequestAccountDisable
{
    public class ErrorEmailProcessor
    {
        private string _supportSubject = "eGrants: Deprovisioning Request Due to Inactivity ";

        public int Process(string dirPath, SqlConnection con, string verbose, string email)
        {
            // connect to everything
            CommonUtilities.ShowDiagnosticIfVerbose("Here we go sending out the errors via email ...", verbose);
            Outlook.Application oApp = new Outlook.Application();
            CommonUtilities.ShowDiagnosticIfVerbose("Created the outlook object.", verbose);
            Outlook.NameSpace oNS = oApp.GetNamespace("MAPI");
            oNS.Logon("", "", false, true);
            CommonUtilities.ShowDiagnosticIfVerbose($"Logged on to Outlook.", verbose);

            CommonUtilities.ShowDiagnosticIfVerbose($"Opening SQL connection ...", verbose);
            con.Open();
            CommonUtilities.ShowDiagnosticIfVerbose($"SQL connection opened.", verbose);

            var errorEvents = GetErrors(con);
            CommonUtilities.ShowDiagnosticIfVerbose($"Found list of {errorEvents.Count} error events to email stakeholders about.", verbose);

            if (errorEvents.Count() > 0)
            {
                var message = CreateEmailBody(errorEvents);
                CommonUtilities.ShowDiagnosticIfVerbose($"Created the body for the error email notification.", verbose);

                SendEmailToSupportTeam(message, email, oApp);
                CommonUtilities.ShowDiagnosticIfVerbose($"Email sent to OGA.", verbose);

                // MLH no retention is necessary as this table is just being used to transfer data from the web app to the email handling server
                DeleteErrorList(con);
                CommonUtilities.ShowDiagnosticIfVerbose($"Error messages cleaned up.", verbose);
            }

            return errorEvents.Count;
        }

        private void DeleteErrorList(SqlConnection con)
        {
            var queryText = "delete [EIM].[dbo].[email_error]";
            var errorEvents = new List<ErrorInfo>();
            try
            {
                using (SqlCommand command = new SqlCommand(queryText, con))
                {
                    command.ExecuteNonQuery();
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Query failed.");
                Console.WriteLine($"The query text : '{queryText}'");
                throw new System.Exception($"Cleaning up the error data failed. Message: {ex.Message}");
            }
        }

        private string CreateEmailBody(List<ErrorInfo> errorInfos)
        {
            //The following eGrants accounts have been deactivated due to 120 days of inactivity in the system:
            //Barbara Fisher bfisher 2024-01-04 06:03:27.940

            var sb = new StringBuilder();
            sb.AppendLine("The following errors have been detected : <br/>");
            sb.AppendLine("<br/>&nbsp;&nbsp;<br/>");
            sb.AppendLine("date/time, grant year, description of the error<br/>");
            sb.AppendLine(@"<table style=""padding-top:10px""><tr><th style=""text-align:left"">User</th><th style=""text-align:left"">UserName</th><th style=""text-align:left"">Last Login Date</th></tr>");
            foreach (var errorInfo in errorInfos)
            {
                sb.AppendLine($"<tr><td>{errorInfo.RecordedTime.ToString()}</td><td>{errorInfo.GrantYear}</td><td>{errorInfo.ErrorMessage}</td></tr>");
            }
            sb.AppendLine("</table>");

            return sb.ToString();
        }

        private static List<ErrorInfo> GetErrors(SqlConnection con)
        {
            var queryText = "select e.appl_id, e.created_date, e.error_type, e.message, a.grant_id, a.support_year + a.suffix_code " +
                                "from[EIM].[dbo].[email_error] e " +
                                "inner join [EIM].[dbo].appls a on e.appl_id = a.appl_id " +
                                "inner join [EIM].[dbo].appls";
            var errorEvents = new List<ErrorInfo>();
            try
            {
                using (SqlCommand command = new SqlCommand(queryText, con))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var errorEvent = new ErrorInfo
                            {
                                ApplId = (reader[0] as int?) ?? 0,
                                //RecordedTime = reader[1] as string,
                                RecordedTime = Convert.ToDateTime(reader[1]),
                                ErrorType = reader[2] as string,
                                ErrorMessage = reader[3] as string,
                                GrantId = (reader[4] as int?) ?? 0,
                                GrantYear = reader[5] as string
                            };
                            errorEvents.Add(errorEvent);
                        }
                    }
                }
                return errorEvents;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Query failed.");
                Console.WriteLine($"The query text (without inferred params) : '{queryText}'");
                throw new System.Exception($"Get disabled users failed in database call. Message: {ex.Message}");
            }
        }

        private bool SendEmailToSupportTeam(string bodyMessage, string emailAddress, Application oApp)
        {
            Outlook.MailItem mailItem =
                (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);

            mailItem.Subject = _supportSubject;
            mailItem.To = emailAddress;
            mailItem.BodyFormat = OlBodyFormat.olFormatHTML;
            mailItem.HTMLBody = bodyMessage;

            mailItem.Send();

            return true;
        }
    }
}
