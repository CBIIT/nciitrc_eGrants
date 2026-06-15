using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using CommonUtilties;

namespace OGARequestAccountDisable
{
    /// <summary>
    /// Processor for Sending Warning Emails to Users Approaching Account Deactivation
    /// 
    /// RESPONSIBILITY:
    /// Identifies eGrants user accounts approaching the 60-day inactivity threshold (at 46 days)
    /// and sends individual warning emails to each user notifying them that their account will
    /// be deactivated if they don't log in.
    /// 
    /// PROCESSING LOGIC:
    /// 1. Queries database for accounts approaching deactivation (46 days inactive)
    /// 2. Checks people_sent_warning table to see if warning already sent
    /// 3. For each user not yet warned:
    ///    - Sends individual warning email
    ///    - Updates people_sent_warning table
    /// 4. Handles re-sending warnings if user logs in and then becomes inactive again
    /// 
    /// WARNING EMAIL TIMING:
    /// - First warning: 46 days after last login
    /// - If user logs in after warning and becomes inactive again: warning resent after 46 days
    /// 
    /// OUTLOOK INTEGRATION:
    /// Uses late-bound COM automation (dynamic/Activator) to control Outlook.
    /// No Primary Interop Assembly (PIA) or NuGet interop package is required at compile time.
    /// Outlook must be installed and configured on the machine where this runs.
    /// </summary>
    public class ProcessorWarning
    {
        private readonly EmailSettings _emailSettings;
        private List<string> _lowerTierEmails = new List<string>();

        /// <summary>
        /// Initializes the warning processor with email configuration settings.
        /// </summary>
        /// <param name="emailSettings">Email configuration from appsettings</param>
        public ProcessorWarning(EmailSettings emailSettings)
        {
            _emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));
        }

        /// <summary>
        /// Main processing method for sending warning emails to users.
        /// Connects to Outlook, retrieves accounts needing warnings, and sends individual emails.
        /// </summary>
        /// <param name="dirPath">Directory path (currently not used but kept for compatibility)</param>
        /// <param name="con">SQL connection to the EIM database</param>
        /// <param name="verbose">Verbose mode flag for diagnostic output</param>
        /// <returns>Number of warning emails sent to users</returns>
        public int ProcessWarning(string dirPath, SqlConnection con, string verbose)
        {
            CommonUtilities.ShowDiagnosticIfVerbose("Initializing warning email process...", verbose);

            // Create Outlook application via late binding (no PIA needed)
            Type outlookType = Type.GetTypeFromProgID("Outlook.Application");
            if (outlookType == null)
                throw new InvalidOperationException("Outlook.Application COM class not found. Is Outlook installed?");
            dynamic oApp = Activator.CreateInstance(outlookType);
            CommonUtilities.ShowDiagnosticIfVerbose("Created the Outlook object.", verbose);
            dynamic oNS = oApp.GetNamespace("MAPI");
            oNS.Logon("", "", false, true);
            CommonUtilities.ShowDiagnosticIfVerbose($"Logged on to Outlook.", verbose);

            // Get accounts that need warning emails
            var usersToSendWarning = GetAccountsForDisabledWarning(con);
            CommonUtilities.ShowDiagnosticIfVerbose($"Found list of {usersToSendWarning.Count} candidate(s) that need to be sent disabled warning email", verbose);

            // Filter out users with missing email addresses
            var usersWhoHaveEmailsToDisable = FilterOutUsersWithMissingInfo(usersToSendWarning);
            CommonUtilities.ShowDiagnosticIfVerbose($"List contains {usersWhoHaveEmailsToDisable.Count} user(s) to proceed with sending email.", verbose);

            // Send warning emails to each user
            if (usersToSendWarning.Count() > 0)
            {
                foreach (var user in usersWhoHaveEmailsToDisable)
                {
                    // Check if email already sent to this user
                    if (!CheckIfEmailSent(user, con))
                    {
                        var message = CreateEmailBody(user);
                        SendEmailToUser(message, oApp, user, con);
                        CommonUtilities.ShowDiagnosticIfVerbose($"Warning email sent to user: {user.UserIdFromDB}", verbose);
                    }
                    else
                    {
                        CommonUtilities.ShowDiagnosticIfVerbose($"Warning email already sent to user: {user.UserIdFromDB}. Email not sent.", verbose);
                    }
                }
            }
            else
            {
                CommonUtilities.ShowDiagnosticIfVerbose($"No users found to send warning email", verbose);
            }
            
            return usersWhoHaveEmailsToDisable.Count;
        }

        /// <summary>
        /// Checks if a warning email has already been sent to a user.
        /// Also handles resetting the sent flag if user logged in after warning and is inactive again.
        /// </summary>
        /// <param name="user">User to check</param>
        /// <param name="con">SQL connection (may be opened/closed within this method)</param>
        /// <returns>True if email was already sent and shouldn't be sent again, false otherwise</returns>
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

                        // If email was sent (flag=1) AND it's been 46 days since last login,
                        // reset the flag so warning can be sent again
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
                                    // Recursively check again after update
                                    if (CheckIfEmailSent(user, con))
                                    {
                                        return true;
                                    }
                                    else { return false; }
                                }
                            }
                            con.Close();
                        }
                        
                        // If no record exists for this user, insert a new one
                        if (count == 0)
                        {
                            con.Open();
                            using (SqlCommand command3 = new SqlCommand(insertText, con))
                            {
                                var rowsAffected = command3.ExecuteNonQuery();
                                if (rowsAffected > 0)
                                {
                                    // Recursively check again after insert
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
                // Return true if email already sent (flag != 0), false otherwise
                return warningListItem.sentFlag != 0 ? true : false;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Query failed.");
                Console.WriteLine($"The query text (without inferred params): '{queryText}'");
                throw new System.Exception($"Check if email sent failed in database call. Message: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates the warning email body for a user.
        /// Includes notification about 60-day requirement and deadline to log in.
        /// </summary>
        /// <param name="user">User receiving the warning</param>
        /// <returns>HTML formatted email body</returns>
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

        /// <summary>
        /// Filters out users with missing email addresses.
        /// Only users with valid email addresses can receive warning emails.
        /// </summary>
        /// <param name="usersToSendWarning">Raw list of users from database</param>
        /// <returns>Filtered list with valid email addresses</returns>
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

        /// <summary>
        /// Queries the database for accounts that need warning emails.
        /// Selects active users who haven't logged in for 46+ days.
        /// </summary>
        /// <param name="con">SQL connection</param>
        /// <returns>List of users needing warning emails</returns>
        private static List<DisabledListItem> GetAccountsForDisabledWarning(SqlConnection con)
        {
            var queryText = "select person_id, first_name, last_name, person_name, email, userid, " +
                "CONVERT(varchar, last_login_date, 101) as last_login_date_tx " +
                "FROM [dbo].[people] " +
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
                Console.WriteLine($"The query text (without inferred params): '{queryText}'");
                throw new System.Exception($"Get accounts for disabled warning failed in database call. Message: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends warning email to a user via Outlook COM automation.
        /// Updates the people_sent_warning table to mark email as sent.
        /// In development mode, sends to debug email instead of actual user.
        /// </summary>
        /// <param name="bodyMessage">HTML formatted email body</param>
        /// <param name="oApp">Outlook Application object (dynamic)</param>
        /// <param name="user">User receiving the warning</param>
        /// <param name="con">SQL connection for updating sent status</param>
        /// <returns>True if email was sent successfully</returns>
        private bool SendEmailToUser(string bodyMessage, dynamic oApp,
            DisabledListItem user, SqlConnection con)
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
                Console.WriteLine($"The query text (without inferred params): '{queryText}'");
                throw new System.Exception($"Update status of people_sent_warning failed in database call. Message: {ex.Message}");
            }

            // Create mail item: 0 = olMailItem
            dynamic mailItem = oApp.CreateItem(0);
            mailItem.BodyFormat = 2; // olFormatHTML
            mailItem.HTMLBody = bodyMessage;

            // In development mode, send to debug email instead of actual user
            if (IsDevEnvironment())
            {
                mailItem.Subject = "[TEST] " + _emailSettings.UserWarningSubject + " for " + user.PersonNameFromDB;
                mailItem.To = _emailSettings.EGrantsDevEmail;
                CommonUtilities.Logger?.Information("DEVELOPMENT MODE: Sending warning email to {DebugEmail} instead of {UserEmail}", 
                    _emailSettings.EGrantsDevEmail, user.EmailFromDB);
            }
            // In production mode, send to actual user
            else
            {
                mailItem.Subject = _emailSettings.UserWarningSubject;
                mailItem.To = user.EmailFromDB;
            }

            mailItem.Send();
            return true;
        }

        /// <summary>
        /// Checks if the current environment is a development environment.
        /// Looks for ASPNETCORE_ENVIRONMENT or DOTNET_ENVIRONMENT variables set to "Development".
        /// </summary>
        /// <returns>True if running in development environment, false otherwise</returns>
        private bool IsDevEnvironment()
        {
            string aspNetEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            string dotNetEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

            return string.Equals(aspNetEnv, "Development", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(dotNetEnv, "Development", StringComparison.OrdinalIgnoreCase);
        }
    }
}
