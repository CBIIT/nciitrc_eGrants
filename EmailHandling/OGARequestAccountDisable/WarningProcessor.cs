using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using CommonUtilties;

namespace OGARequestAccountDisable
{
    /// <summary>
    /// OUTLOOK INTEGRATION:
    /// Uses late-bound COM automation (dynamic/Activator) to control Outlook.
    /// No Primary Interop Assembly (PIA) or NuGet interop package is required at compile time.
    /// Outlook must be installed and configured on the machine where this runs.
    /// </summary>
    public class ProcessorWarning
    {
        private string _userSubject = "Action Required: eGrants Account Deactivation";
        private List<string> _lowerTierEmails = new List<string>();

        public int ProcessWarning(string dirPath, SqlConnection con, string verbose, string debug)
        {
            CommonUtilities.ShowDiagnosticIfVerbose("Here we go ...", verbose);

            // Create Outlook application via late binding (no PIA needed)
            Type outlookType = Type.GetTypeFromProgID("Outlook.Application");
            if (outlookType == null)
                throw new InvalidOperationException("Outlook.Application COM class not found. Is Outlook installed?");
            dynamic oApp = Activator.CreateInstance(outlookType);
            CommonUtilities.ShowDiagnosticIfVerbose("Created the outlook object.", verbose);
            dynamic oNS = oApp.GetNamespace("MAPI");
            oNS.Logon("", "", false, true);
            CommonUtilities.ShowDiagnosticIfVerbose($"Logged on to Outlook.", verbose);

            var usersToSendWarning = GetAccountsForDisabledWarning(con);
            CommonUtilities.ShowDiagnosticIfVerbose($"Found list of {usersToSendWarning.Count} candidates that need to be sent disabled warning email", verbose);

            var usersWhoHaveEmailsToDisable = FilterOutUsersWithMissingInfo(usersToSendWarning);
            CommonUtilities.ShowDiagnosticIfVerbose($"List contains {usersWhoHaveEmailsToDisable.Count} that we want to proceed with sending email.", verbose);

            if (usersToSendWarning.Count() > 0)
            {
                foreach (var user in usersWhoHaveEmailsToDisable)
                {
                    if (!CheckIfEmailSent(user, con))
                    {
                        var message = CreateEmailBody(user);
                        SendEmailToUser(message, oApp, debug, user, con);
                        CommonUtilities.ShowDiagnosticIfVerbose($"Email sent to User.", verbose);
                    }
                    else
                    {
                        CommonUtilities.ShowDiagnosticIfVerbose($"Email already sent to User. Email not sent", verbose);
                    }
                }
            }
            else
            {
                CommonUtilities.ShowDiagnosticIfVerbose($"No users found to send email", verbose);
            }
            return usersWhoHaveEmailsToDisable.Count;
        }

        private Boolean CheckIfEmailSent(DisabledListItem user, SqlConnection con)
        {
            var queryText = "SELECT psw.email_sent, p.last_login_date " +
                                    "FROM [dbo].[people_sent_warning] psw " +
                                    "inner join dbo.people p on p.person_id = psw.person_id " +
                                    $"where p.person_id = {user.PersonIdFromDB}";
            var insertText = "insert into " +
                             "[dbo].people_sent_warning (person_id, email_sent)" +
                             "SELECT person_id, 0 AS email_sent from [dbo].people";
            var updateText = "update [dbo].[people_sent_warning] " +
             $"set email_sent=0 where person_id = {user.PersonIdFromDB}";

            var warningListItem = new WarningListItem
            {
                sentFlag = 0,
                lastLoginDate = DateTime.Now
            };
            var count = 0;
            try
            {
                using (SqlCommand command = new SqlCommand(queryText, con))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            warningListItem = new WarningListItem
                            {
                                sentFlag = (reader[0] as int?) ?? 0,
                                lastLoginDate = (DateTime)reader[1]
                            };
                            count++;
                        }
                        con.Close();

                        if (warningListItem.sentFlag == 1
                            &&
                            warningListItem.lastLoginDate.AddDays(46)
                            .ToString("yyyy-MM-dd")
                            .Equals(DateTime.Now.ToString("yyyy-MM-dd")))
                        {
                            con.Open();
                            using (SqlCommand command2 = new SqlCommand(updateText, con))
                            {
                                var rowsAffected = command2.ExecuteNonQuery();
                                if (rowsAffected > 0)
                                {
                                    if (CheckIfEmailSent(user, con))
                                    {
                                        return true;
                                    }
                                    else { return false; }
                                }
                            }
                            con.Close();
                        }
                        if (count == 0)
                        {
                            con.Open();
                            using (SqlCommand command3 = new SqlCommand(insertText, con))
                            {
                                var rowsAffected = command3.ExecuteNonQuery();
                                if (rowsAffected > 0)
                                {
                                    if (CheckIfEmailSent(user, con))
                                    {
                                        return true;
                                    }
                                    else { return false; }
                                }
                            }
                            con.Close();
                        }
                    }
                }
                return warningListItem.sentFlag != 0 ? true : false;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Query failed.");
                Console.WriteLine($"The query text (without inferred params) : '{queryText}'");
                throw new System.Exception($"Check if email sent failed in database call. Message: {ex.Message}");
            }
        }

        private string CreateEmailBody(DisabledListItem user)
        {
            var sb = new StringBuilder();
            var priorToDate = DateTime.Parse(user.LastLoginDateFromDB).AddDays(60).Date;
            sb.AppendLine("eGrants users are required to sign into the system every 60 days.");
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
            var newFilteredList = new List<DisabledListItem>();
            foreach (var userToWarn in usersToSendWarning)
            {
                if (!string.IsNullOrWhiteSpace(userToWarn.EmailFromDB))
                {
                    newFilteredList.Add(userToWarn);
                }
            }
            return newFilteredList;
        }

        private static List<DisabledListItem> GetAccountsForDisabledWarning(SqlConnection con)
        {
            var queryText = "select person_id, first_name, last_name, person_name, email, userid, " +
                "CONVERT(varchar, last_login_date, 101) as last_login_date_tx " +
                "FROM [dbo].[people]" +
                "where active = 1 and last_login_date < (DATEADD(day, -46, GETDATE()))";

            var usersToDisable = new List<DisabledListItem>();
            try
            {
                using (SqlCommand command = new SqlCommand(queryText, con))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var warnPerson = new DisabledListItem
                            {
                                PersonIdFromDB = (reader[0] as int?) ?? 0,
                                FirstNameFromDB = reader[1] as string,
                                LastNameFromDB = reader[2] as string,
                                PersonNameFromDB = reader[3] as string,
                                EmailFromDB = reader[4] as string,
                                UserIdFromDB = reader[5] as string,
                                LastLoginDateFromDB = reader[6] as string
                            };
                            usersToDisable.Add(warnPerson);
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

        private bool SendEmailToUser(string bodyMessage, dynamic oApp,
            string debug, DisabledListItem user, SqlConnection con)
        {
            var queryText = "update [dbo].[people_sent_warning] " +
             $"set email_sent=1 where person_id = {user.PersonIdFromDB}";

            try
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(queryText, con))
                {
                    var rowsAffected = command.ExecuteNonQuery();
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Query failed.");
                Console.WriteLine($"The query text (without inferred params) : '{queryText}'");
                throw new System.Exception($"Update status of people_sent_waring failed in database call. Message: {ex.Message}");
            }

            if (debug == "n")
            {
                // Create mail item: 0 = olMailItem
                dynamic mailItem = oApp.CreateItem(0);
                mailItem.Subject = _userSubject;
                mailItem.To = user.EmailFromDB;
                mailItem.BodyFormat = 2; // olFormatHTML
                mailItem.HTMLBody = bodyMessage;
                mailItem.Send();
            }
            else
            {
                foreach (var email in _lowerTierEmails)
                {
                    dynamic mailItem = oApp.CreateItem(0);
                    mailItem.Subject = "[TEST] " + _userSubject + " for " + user.PersonNameFromDB;
                    mailItem.To = email;
                    mailItem.BodyFormat = 2; // olFormatHTML
                    mailItem.HTMLBody = bodyMessage;
                    mailItem.Send();
                }
            }
            return true;
        }
    }
}
