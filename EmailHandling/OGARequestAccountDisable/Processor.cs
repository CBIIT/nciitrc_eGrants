using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using CommonUtilties;

namespace OGARequestAccountDisable
{
    /// <summary>
    /// Processor for OGA Account Disable Requests
    /// 
    /// RESPONSIBILITY:
    /// Identifies inactive eGrants user accounts (60+ days of inactivity) and sends
    /// deprovisioning requests to the OGA (Office of Grants Administration) team.
    /// 
    /// PROCESSING LOGIC:
    /// 1. Queries people_for_oga_to_disable table for accounts not yet sent to OGA
    /// 2. Filters out accounts with missing name information (keeps service accounts with person_name)
    /// 3. Constructs HTML email with table of users to deactivate
    /// 4. Sends email to OGA team (or dev team if in debug mode)
    /// 5. Updates database to mark accounts as sent to OGA with timestamp
    /// 
    /// NAME HANDLING:
    /// - Users with first AND last name: rendered as "FirstName LastName"
    /// - Service accounts (missing first/last but has person_name): use person_name
    /// - Accounts missing all name fields: filtered out and not sent to OGA
    /// 
    /// NOTE: There may be confusion because users in this process are already disabled
    /// in the eGrants DB but not yet disabled from OGA's perspective.
    /// 
    /// OUTLOOK INTEGRATION:
    /// Uses COM Interop to send emails via Microsoft Outlook.
    /// </summary>
    public class Processor
    {
        private readonly EmailSettings _emailSettings;

        /// <summary>
        /// Initializes the processor with email configuration settings.
        /// </summary>
        /// <param name="emailSettings">Email configuration from appsettings</param>
        public Processor(EmailSettings emailSettings)
        {
            _emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));
        }

        /// <summary>
        /// Main processing method that orchestrates the account disable workflow.
        /// Connects to Outlook and database, retrieves disabled accounts, sends email to OGA,
        /// and updates the database.
        /// </summary>
        /// <param name="dirPath">Directory path (currently not used but kept for compatibility)</param>
        /// <param name="con">SQL connection to the EIM database</param>
        /// <param name="verbose">Verbose mode flag for diagnostic output</param>
        /// <returns>Number of user accounts requested to be disabled</returns>
        public int Process(string dirPath, SqlConnection con, string verbose)
        {
            // Connect to Outlook via late binding (no PIA needed)
            CommonUtilities.ShowDiagnosticIfVerbose("Initializing Outlook connection...", verbose);
            Type outlookType = Type.GetTypeFromProgID("Outlook.Application");
            if (outlookType == null)
                throw new InvalidOperationException("Outlook.Application COM class not found. Is Outlook installed?");
            dynamic oApp = Activator.CreateInstance(outlookType);
            CommonUtilities.ShowDiagnosticIfVerbose("Created the Outlook object.", verbose);
            dynamic oNS = oApp.GetNamespace("MAPI");
            oNS.Logon("", "", false, true);
            CommonUtilities.ShowDiagnosticIfVerbose($"Logged on to Outlook.", verbose);

            // Open database connection
            CommonUtilities.ShowDiagnosticIfVerbose($"Opening SQL connection...", verbose);
            con.Open();
            CommonUtilities.ShowDiagnosticIfVerbose($"SQL connection opened.", verbose);

            // Retrieve accounts to be disabled from database
            var usersToDisable = GetDisabledAccounts(con);
            CommonUtilities.ShowDiagnosticIfVerbose($"Found list of {usersToDisable.Count} candidate(s) that could be disabled.", verbose);

            // Filter out users with missing name information
            var usersWhoHaveEmailsToDisable = FilterOutUsersWithMissingInfo(usersToDisable);
            CommonUtilities.ShowDiagnosticIfVerbose($"List contains {usersWhoHaveEmailsToDisable.Count} user(s) to proceed with disabling.", verbose);

            // Create HTML email body with user table
            var message = CreateEmailBody(usersWhoHaveEmailsToDisable);
            CommonUtilities.ShowDiagnosticIfVerbose($"Created the body for the email to OGA.", verbose);

            // Send email if there are users to disable
            if (usersToDisable.Count() > 0)
            {
                SendEmailToOGA(message, oApp);
                CommonUtilities.ShowDiagnosticIfVerbose($"Email sent to OGA.", verbose);

                // Update database to mark accounts as sent
                UpdateStatusOfOGAEmailsToDisable(usersWhoHaveEmailsToDisable, con, verbose);
                CommonUtilities.ShowDiagnosticIfVerbose($"Updated the status of users in table people_for_oga_to_disable", verbose);
            }

            return usersWhoHaveEmailsToDisable.Count;
        }

        /// <summary>
        /// Updates the people_for_oga_to_disable table to mark accounts as sent to OGA.
        /// Sets the sent_to_oga_date field to the current date/time.
        /// </summary>
        /// <param name="usersWhoHaveEmailsToDisabled">List of users that were sent to OGA</param>
        /// <param name="con">Open SQL connection</param>
        /// <param name="verbose">Verbose mode flag</param>
        private void UpdateStatusOfOGAEmailsToDisable(List<DisabledListItem> usersWhoHaveEmailsToDisabled, SqlConnection con, string verbose)
        {
            var personIds = usersWhoHaveEmailsToDisabled.Select(x => x.PersonIdFromDB).ToList();
            var personIdsTokenized = string.Join<int>(",", personIds);

            // Adding this condition to avoid exception with empty string search
            if (!string.IsNullOrEmpty(personIdsTokenized))
            {
                var queryText = $"update [dbo].[people_for_oga_to_disable] set sent_to_oga_date=GetDate() where person_id in ({personIdsTokenized})";
                var disabledUsers = new List<DisabledListItem>();
                try
                {
                    using (SqlCommand command = new SqlCommand(queryText, con))
                    {
                        var rowsAffected = command.ExecuteNonQuery();
                        CommonUtilities.ShowDiagnosticIfVerbose($"Updated [people_for_oga_to_disable] table with date email sent per user ({rowsAffected} row(s) affected)", verbose);
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("Query failed.");
                    Console.WriteLine($"The query text (without inferred params): '{queryText}'");
                    throw new System.Exception($"Update status of OGA emails to disable failed in database call. Message: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Creates the HTML email body containing a table of users to be deprovisioned.
        /// Email format includes user name, username, and last login date.
        /// </summary>
        /// <param name="usersWhoHaveEmailsToBeDisabled">List of users to include in email</param>
        /// <returns>HTML formatted email body</returns>
        private string CreateEmailBody(List<DisabledListItem> usersWhoHaveEmailsToBeDisabled)
        {
            var sb = new StringBuilder();
            sb.AppendLine("The following eGrants accounts have been deactivated due to 60 days of inactivity in the system:");
            sb.AppendLine("<br/>&nbsp;&nbsp;<br/>");
            sb.AppendLine(@"<table style=""padding-top:10px""><tr><th style=""text-align:left"">User</th><th style=""text-align:left"">UserName</th><th style=""text-align:left"">Last Login Date</th></tr>");
            
            foreach (var disabledUser in usersWhoHaveEmailsToBeDisabled)
            {
                sb.AppendLine($"<tr><td>{disabledUser.FinalNameForOGA}</td><td>{disabledUser.UserIdFromDB}</td><td>{disabledUser.LastLoginDateFromDB}</td></tr>");
            }
            
            sb.AppendLine("</table>");

            return sb.ToString();
        }

        /// <summary>
        /// Filters out users with missing name information.
        /// Keeps users with first AND last name, or service accounts with person_name.
        /// </summary>
        /// <param name="usersToDisable">Raw list of users from database</param>
        /// <returns>Filtered list with renderable names</returns>
        /// <remarks>
        /// Currently out of active users, about 11.8% have a missing First Name or Last Name.
        /// Many of these are service accounts with names like "NCI OGA PROGRESS REPORT", 
        /// "nciogastage", "ncigabawardunit", or "CA ERA NOTIFICATIONS".
        /// </remarks>
        public static List<DisabledListItem> FilterOutUsersWithMissingInfo(List<DisabledListItem> usersToDisable)
        {
            var newFilteredList = new List<DisabledListItem>();

            foreach (var userToDisable in usersToDisable)
            {
                // If they have first AND last name, send them to OGA
                if (!string.IsNullOrWhiteSpace(userToDisable.FirstNameFromDB) &&
                    !string.IsNullOrWhiteSpace(userToDisable.LastNameFromDB))
                {
                    userToDisable.FinalNameForOGA = $"{userToDisable.FirstNameFromDB} {userToDisable.LastNameFromDB}";
                    newFilteredList.Add(userToDisable);
                }
                // If they are missing either first name or last name but have a (non null, non blank) person name,
                // they are probably a service account
                else if ((string.IsNullOrWhiteSpace(userToDisable.FirstNameFromDB) ||
                    string.IsNullOrWhiteSpace(userToDisable.LastNameFromDB))
                    && !string.IsNullOrWhiteSpace(userToDisable.PersonNameFromDB))
                {
                    userToDisable.FinalNameForOGA = userToDisable.PersonNameFromDB;
                    newFilteredList.Add(userToDisable);
                }
                else
                {
                    // Mark as failed to render name - do NOT add to outgoing list to OGA
                    userToDisable.FailedToRenderName = true;
                }
            }

            return newFilteredList;
        }

        /// <summary>
        /// Queries the database for accounts pending OGA deprovisioning.
        /// Selects from people_for_oga_to_disable joined with people table,
        /// where sent_to_oga_date is null (not yet sent).
        /// </summary>
        /// <param name="con">Open SQL connection</param>
        /// <returns>List of disabled accounts with user details</returns>
        private static List<DisabledListItem> GetDisabledAccounts(SqlConnection con)
        {
            var queryText = "select p.person_id, p.first_name, p.last_name, p.person_name, p.email, p.userid, " +
                "CONVERT(varchar, last_login_date, 101) as last_login_date_tx " +
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
                                FirstNameFromDB = reader[1] as string,
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
                Console.WriteLine($"The query text (without inferred params): '{queryText}'");
                throw new System.Exception($"Get disabled users failed in database call. Message: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends the deprovisioning email to OGA or dev team via Outlook COM automation.
        /// In development mode, sends to debug email. In production mode, sends to OGA prod email.
        /// </summary>
        /// <param name="bodyMessage">HTML formatted email body</param>
        /// <param name="oApp">Outlook Application object</param>
        /// <returns>True if email was sent successfully</returns>
        private bool SendEmailToOGA(string bodyMessage, dynamic oApp)
        {
            // Create mail item: 0 = olMailItem
            dynamic mailItem = oApp.CreateItem(0);

            mailItem.Subject = GetEnvironmentPrefix() + _emailSettings.OgaSubject;

            // In development mode, send to debug email. In production, send to OGA team
            if (IsDevEnvironment())
            {
                mailItem.To = _emailSettings.EGrantsDevEmail;
                CommonUtilities.Logger?.Information("DEVELOPMENT MODE: Sending to {DebugEmail}", _emailSettings.EGrantsDevEmail);
            }
            else
            {
                mailItem.To = _emailSettings.OgaProdEmail;
            }

            mailItem.BodyFormat = 2; // olFormatHTML
            mailItem.HTMLBody = bodyMessage;

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

        /// <summary>
        /// Returns the environment name in parentheses (e.g. "(Development) ") if not Production.
        /// Returns empty string for Production or if DOTNET_ENVIRONMENT is not set.
        /// </summary>
        private static string GetEnvironmentPrefix()
        {
            var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            if (string.IsNullOrWhiteSpace(env) || env.Equals("Production", StringComparison.OrdinalIgnoreCase))
                return string.Empty;
            return $"({env}) ";
        }
    }

    ///// <summary>
    ///// Represents a user account that is disabled or approaching deactivation.
    ///// Used for both disable requests to OGA and warning emails to users.
    ///// </summary>
    //public class DisabledListItem
    //{
    //    public int PersonIdFromDB { get; set; }
    //    public string FirstNameFromDB { get; set; }
    //    public string LastNameFromDB { get; set; }
    //    public string PersonNameFromDB { get; set; }
    //    public string EmailFromDB { get; set; }
    //    public string UserIdFromDB { get; set; }
    //    public string LastLoginDateFromDB { get; set; }
    //    public string FinalNameForOGA { get; set; }
    //    public bool FailedToRenderName { get; set; }
    //}

    ///// <summary>
    ///// Represents warning email tracking information for a user.
    ///// </summary>
    //public class WarningListItem
    //{
    //    public int sentFlag { get; set; }
    //    public DateTime lastLoginDate { get; set; }
    //}
}