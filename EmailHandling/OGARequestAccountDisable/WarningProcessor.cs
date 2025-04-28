using CommonUtilties;
using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace OGARequestAccountDisable
{
    public class ProcessorWarning
    {

        private string _eGrantsDevEmail = "eGrantsDev@mail.nih.gov";
        private string _userSubject = "Action Required: eGrants Account Deactivation";

        public int ProcessWarning(string dirPath, SqlConnection con, string verbose, string debug)
        {
            // connect to everything
            CommonUtilities.ShowDiagnosticIfVerbose("Here we go ...", verbose);
            Outlook.Application oApp = new Outlook.Application();
            CommonUtilities.ShowDiagnosticIfVerbose("Created the outlook object.", verbose);
            Outlook.NameSpace oNS = oApp.GetNamespace("MAPI");
            oNS.Logon("", "", false, true);
            CommonUtilities.ShowDiagnosticIfVerbose($"Logged on to Outlook.", verbose);

            CommonUtilities.ShowDiagnosticIfVerbose($"Opening SQL connection ...", verbose);
            con.Open();
            CommonUtilities.ShowDiagnosticIfVerbose($"SQL connection opened.", verbose);

            var usersToSendWarning = GetAccountsForDisabledWarning(con);
            CommonUtilities.ShowDiagnosticIfVerbose($"Found list of {usersToSendWarning.Count} candidates that need to be sent disabled warning email", verbose);

            var usersWhoHaveEmailsToDisable = FilterOutUsersWithMissingInfo(usersToSendWarning);
            CommonUtilities.ShowDiagnosticIfVerbose($"List contains {usersWhoHaveEmailsToDisable.Count} that we want to proceed with sending email.", verbose);

            if (usersToSendWarning.Count() > 0)
            {
                foreach (var user in usersWhoHaveEmailsToDisable) {
                    var message = CreateEmailBody(user);
                    SendEmailToUser(message, oApp, debug, user);
                }
                
                CommonUtilities.ShowDiagnosticIfVerbose($"Email sent to User.", verbose);

            }

            return usersWhoHaveEmailsToDisable.Count;
        }


        private string CreateEmailBody(DisabledListItem user)
        {
//            
//            eGrants users are required to sign into the system every 120 days.< br >
//            In order to maintain access, you must sign into eGrants prior to { deactivation date}
//            or your account will be deactivated         
//            eGrants system link: https://egrants.nci.nih.gov
//              Thank you

            var sb = new StringBuilder();
            var priorToDate = DateTime.Parse(user.LastLoginDateFromDB).AddDays(120).Date;
            sb.AppendLine("eGrants users are required to sign into the system every 120 days.");
            sb.AppendLine("<br/>");
            sb.AppendLine("In order to maintain access, you must sign into eGrants prior to ");
            sb.AppendLine(priorToDate.Date.ToString("MM/dd/yyyy"));
            sb.AppendLine(" or your account will be deactivated.");
            sb.AppendLine("<br/>");
            sb.AppendLine("<p>eGrants system link: https://egrants.nci.nih.gov");
            sb.AppendLine("<br/>");
            sb.AppendLine("<br/>");
            sb.AppendLine("Thank you");

            return sb.ToString();
        }

        public static List<DisabledListItem> FilterOutUsersWithMissingInfo(List<DisabledListItem> usersToSendWarning)
        {
            // active users who are missing first names or last names are filtered out here
            // currently out of 1721 non-disabled users, 11.8% have a missing First Name or Last Name
            // although 16 of these cases seem to have a service account name in person_name
            // like NCI OGA PROGRESS REPORT, nciogastage, ncigabawardunit, or CA ERA NOTIFICATIONS

            var newFilteredList = new List<DisabledListItem>();

            // include users who have a first AND last name (and render their name)
            foreach (var userToWarn in usersToSendWarning)
            {
                // if they email, send them warnings
                if (!string.IsNullOrWhiteSpace(userToWarn.EmailFromDB))
                {
                    newFilteredList.Add(userToWarn);
                }
                else
                {
                    userToWarn.FailedToRenderName = true;
                    // do NOT add this to the outgoing list to OGA
                }
            }

            return newFilteredList;
        }

        private static List<DisabledListItem> GetAccountsForDisabledWarning(SqlConnection con)
        {
            var queryText = "select person_id, first_name, last_name, person_name, email, userid, " +
                "CONVERT(varchar, last_login_date, 101) as last_login_date_tx " +
                "FROM [dbo].[people]" +
                "where active = 1 and last_login_date < (DATEADD(day, -106, GETDATE()))";

            var usersToDisable = new List<DisabledListItem>();
            try
            {
                using (SqlCommand command = new SqlCommand(queryText, con))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var disabledPerson = new DisabledListItem
                            {
                                PersonIdFromDB = (reader[0] as int?) ?? 0,
                                FirstNameFromDB = reader[1] as string, // reader.GetString(1),
                                LastNameFromDB = reader[2] as string,
                                PersonNameFromDB = reader[3] as string,
                                EmailFromDB = reader[4] as string,
                                UserIdFromDB = reader[5] as string,
                                LastLoginDateFromDB = reader[6] as string
                            };
                            usersToDisable.Add(disabledPerson);
                        }
                    }
                }
                return usersToDisable;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Query failed.");
                Console.WriteLine($"The query text (without inferred params) : '{queryText}'");
                throw new System.Exception($"Get accounts for disabled warning failed in database call. Message: {ex.Message}");
            }
        }

        private bool SendEmailToUser(string bodyMessage, Application oApp, string debug, DisabledListItem user)
        {
                Outlook.MailItem mailItem =
                (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
                if (debug == "n")
                {
                    mailItem.Subject = _userSubject;
                    mailItem.To = user.EmailFromDB;
                }
                else
                {
                // Change this later
                    mailItem.Subject = _userSubject + " TEST for " + user.PersonNameFromDB;
                    mailItem.To = "aalyaan.feroz@nih.gov"; 
                }
                mailItem.BodyFormat = OlBodyFormat.olFormatHTML;
                mailItem.HTMLBody = bodyMessage;

                mailItem.Send();
                return true;
        }
    }
}
