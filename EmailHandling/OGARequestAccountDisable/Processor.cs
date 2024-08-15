using CommonUtilties;
using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace OGARequestAccountDisable
{
    public class Processor
    {
        // MLH : there may be some confusion here because anyone coming into this process is already disabled in the eGrants DB
        // but they aren't disabled yet from OGA's point of view, so they're disabled but also not disabled ...

        private string _eGrantsDevEmail = "eGrantsDev@mail.nih.gov";
        private string _ogaProdEmail = "NCIOGABOBTeam2@mail.nih.gov";
        private string _ogaSubject = "eGrants: Deprovisioning Request Due to Inactivity ";

        public int Process(string dirPath, SqlConnection con, string verbose, string debug)
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

            var usersToDisable = GetDisabledAccounts(con);
            CommonUtilities.ShowDiagnosticIfVerbose($"Found list of {usersToDisable.Count} candidates that could be disabled.", verbose);

            var usersWhoHaveEmailsToDisable = FilterOutUsersWithMissingInfo(usersToDisable);
            CommonUtilities.ShowDiagnosticIfVerbose($"List contains {usersWhoHaveEmailsToDisable.Count} that we want to proceed with disabled.", verbose);

            var message = CreateEmailBody(usersWhoHaveEmailsToDisable);
            CommonUtilities.ShowDiagnosticIfVerbose($"Created the body for the email to OGA.", verbose);

            if (usersToDisable.Count() > 0)
            {
                SendEmailToOGA(message, oApp, debug);
                CommonUtilities.ShowDiagnosticIfVerbose($"Email sent to OGA.", verbose);

                UpdateStatusOfOGAEmailsToDisable(usersWhoHaveEmailsToDisable, con, verbose);
                CommonUtilities.ShowDiagnosticIfVerbose($"Updated the status of users in table people_for_oga_to_disable", verbose);
            }

            return usersWhoHaveEmailsToDisable.Count;
        }

        private void UpdateStatusOfOGAEmailsToDisable(List<DisabledListItem> usersWhoHaveEmailsToDisabled, SqlConnection con, string verbose)
        {
            var personIds = usersWhoHaveEmailsToDisabled.Select(x => x.PersonIdFromDB).ToList();
            var personIdsTokenized = string.Join<int>(",", personIds);

            var queryText = $"update [dbo].[people_for_oga_to_disable] set sent_to_oga_date=GetDate() where person_id in ({personIdsTokenized})";
            var disabledUsers = new List<DisabledListItem>();
            try
            {
                using (SqlCommand command = new SqlCommand(queryText, con))
                {
                    var rowsAffected = command.ExecuteNonQuery();
                    CommonUtilities.ShowDiagnosticIfVerbose($"Updated [people_for_oga_to_disable] table with date email sent per user", verbose);
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Query failed.");
                Console.WriteLine($"The query text (without inferred params) : '{queryText}'");
                throw new System.Exception($"Update status of OGA emails to disable failed in database call. Message: {ex.Message}");
            }
        }

        private string CreateEmailBody(List<DisabledListItem> usersWhoHaveEmailsToBeDisabled)
        {
            var sb = new StringBuilder();
            sb.AppendLine("The following eGrants accounts have been deactivated due to 120 days of inactivity in the system:");
            foreach(var disabledUser in usersWhoHaveEmailsToBeDisabled)
            {
                sb.AppendLine($"{disabledUser.FinalNameForOGA} {disabledUser.UserIdFromDB} {disabledUser.LastLoginDateFromDB}");
                //sb.AppendLine($"{disabledUser.FinalNameForOGA} {disabledUser.UserIdFromDB} {disabledUser.LastLoginDateFromDB}<br/>");
            }

            return sb.ToString();
        }

        public static List<DisabledListItem> FilterOutUsersWithMissingInfo(List<DisabledListItem> usersToDisable)
        {
            // active users who are missing first names or last names are filtered out here
            // currently out of 1721 non-disabled users, 11.8% have a missing First Name or Last Name
            // although 16 of these cases seem to have a service account name in person_name
            // like NCI OGA PROGRESS REPORT, nciogastage, ncigabawardunit, or CA ERA NOTIFICATIONS

            var newFilteredList = new List<DisabledListItem>();

            // include users who have a first AND last name (and render their name)
            foreach(var userToDisable in usersToDisable)
            {
                // if they have first AND last name, send them to OGA
                if (!string.IsNullOrWhiteSpace(userToDisable.FirstNameFromDB) &&
                    !string.IsNullOrWhiteSpace(userToDisable.LastNameFromDB))
                {
                    userToDisable.FinalNameForOGA = $"{userToDisable.FirstNameFromDB} {userToDisable.LastNameFromDB}";
                    newFilteredList.Add(userToDisable);
                }
                // if they are missing either first name or last name but have a (non null, non blank) person name, they are probably a service account
                else if ((string.IsNullOrWhiteSpace(userToDisable.FirstNameFromDB) ||
                    string.IsNullOrWhiteSpace(userToDisable.LastNameFromDB))
                    && !string.IsNullOrWhiteSpace(userToDisable.PersonNameFromDB))
                {
                    userToDisable.FinalNameForOGA = userToDisable.PersonNameFromDB;
                    newFilteredList.Add(userToDisable);
                } else
                {
                    userToDisable.FailedToRenderName = true;
                    // do NOT add this to the outgoing list to OGA
                }
            }

            return newFilteredList;
        }

        private static List<DisabledListItem> GetDisabledAccounts(SqlConnection con)
        {
            var queryText = "select p.person_id, p.first_name, p.last_name, p.person_name, p.email, p.userid, " +
                "CONVERT(varchar, last_login_date, 121) as last_login_date_tx " +
                "from [dbo].[people_for_oga_to_disable] pod " +
                "inner join [dbo].[people] p on p.person_id = pod.person_id " +
                "where sent_to_oga_date is null";
            var disabledUsers = new List<DisabledListItem>();
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
                            disabledUsers.Add(disabledPerson);
                        }
                    }
                }
                return disabledUsers;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Query failed.");
                Console.WriteLine($"The query text (without inferred params) : '{queryText}'");
                throw new System.Exception($"Get disabled users failed in database call. Message: {ex.Message}");
            }
        }

        private bool SendEmailToOGA(string bodyMessage, Application oApp, string debug)
        {
            Outlook.MailItem mailItem =
                (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);

            mailItem.Subject = _ogaSubject;
            if (debug == "n")
            {
                mailItem.To = _ogaProdEmail;
            } else
            {
                mailItem.To = _eGrantsDevEmail;
            }
            //mailItem.Body = bodyMessage;
            // MLH : rendering as HTML will cause Outlook to put the names on the same line as the rest of the content :p
            //mailItem.BodyFormat = OlBodyFormat.olFormatPlain;

            //mailItem.IsBodyHtml = false;  // no definition
            //mailItem.BodyFormat = OlBodyFormat.olFormatPlain;
            //mailItem.Body = bodyMessage;

            mailItem.BodyFormat = OlBodyFormat.olFormatRichText;
            //mailItem.RTFBody = bodyMessage;   // COM : operation failed
            mailItem.Body = bodyMessage;


            //mailItem.HTMLBody = bodyMessage;
            //mailItem.BodyFormat = OlBodyFormat.olFormatHTML;

            mailItem.Send();

            return true;
        }
    }
}
