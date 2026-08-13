using System;
using System.Collections.Generic;
using OGARequestAccountDisable;

namespace EmailHandlingTests.Unit.OGADisableEmail
{
    /// <summary>
    /// Test processor that extends OGARequestAccountDisable.Processor to intercept email sending
    /// and capture processing details for test verification.
    /// 
    /// This allows testing the processor logic without requiring:
    /// - Active Outlook connection
    /// - Real email folders
    /// - Database connections
    /// </summary>
    internal class TestOGADisableProcessor : Processor
    {
        /// <summary>
        /// Initializes the test processor with default email settings for testing.
        /// </summary>
        public TestOGADisableProcessor() : base(CreateTestEmailSettings(), CreateTestSmtpService())
        {
        }

        /// <summary>
        /// Initializes the test processor with custom email settings.
        /// </summary>
        /// <param name="emailSettings">Custom email settings for testing</param>
        public TestOGADisableProcessor(EmailSettings emailSettings) : base(emailSettings, CreateTestSmtpService())
        {
        }

        private static CommonUtilties.SmtpEmailService CreateTestSmtpService()
        {
            return new CommonUtilties.SmtpEmailService("localhost", 25, "test@test.com");
        }

        /// <summary>
        /// Creates default email settings for testing.
        /// </summary>
        /// <returns>EmailSettings configured for testing</returns>
        private static EmailSettings CreateTestEmailSettings()
        {
            return new EmailSettings
            {
                EGrantsDevEmail = "test-dev@nih.gov",
                OgaProdEmail = "test-oga@nih.gov",
                OgaSubject = "TEST: eGrants: Deprovisioning Request Due to Inactivity ",
                UserWarningSubject = "TEST: Action Required: eGrants Account Deactivation"
            };
        }

        /// <summary>
        /// Tracks all emails that would have been sent during the test session.
        /// </summary>
        public List<TestOGAEmailRecord> EmailsSentThisSession { get; } = new List<TestOGAEmailRecord>();

        /// <summary>
        /// Tracks all users processed for disabling.
        /// </summary>
        public List<DisabledListItem> UsersProcessedThisSession { get; } = new List<DisabledListItem>();

        /// <summary>
        /// Count of users processed during the test.
        /// </summary>
        public int ProcessedCount { get; private set; } = 0;

        /// <summary>
        /// Indicates if an error occurred during processing.
        /// </summary>
        public bool ErrorOccurred { get; private set; } = false;

        /// <summary>
        /// Error message if an error occurred.
        /// </summary>
        public string LastErrorMessage { get; private set; } = null;

        /// <summary>
        /// Simulated users to process (for testing without database).
        /// </summary>
        public List<DisabledListItem> SimulatedDisabledUsers { get; set; } = new List<DisabledListItem>();

        /// <summary>
        /// Simulated email recipient for testing.
        /// </summary>
        public string SimulatedRecipient { get; set; } = "test@nih.gov";

        /// <summary>
        /// Test method to process simulated disabled users without database/Outlook access.
        /// </summary>
        /// <param name="verbose">Verbose mode flag</param>
        /// <returns>Number of users processed</returns>
        public int TestProcessSimulatedUsers(string verbose = "n")
        {
            try
            {
                ProcessedCount = 0;

                // Filter out users with missing info (same logic as production)
                var filteredUsers = Processor.FilterOutUsersWithMissingInfo(SimulatedDisabledUsers);

                foreach (var user in filteredUsers)
                {
                    ProcessedCount++;
                    UsersProcessedThisSession.Add(user);

                    if (verbose.ToLower().Contains("y"))
                    {
                        Console.WriteLine($"TEST: Processed user: {user.FinalNameForOGA}");
                    }
                }

                // Simulate sending email if there are users to disable
                if (filteredUsers.Count > 0)
                {
                    var emailBody = TestCreateEmailBody(filteredUsers);
                    var emailRecord = new TestOGAEmailRecord
                    {
                        To = SimulatedRecipient,
                        Subject = "eGrants: Deprovisioning Request Due to Inactivity ",
                        Body = emailBody,
                        UserCount = filteredUsers.Count,
                        TimeCaptured = DateTime.Now
                    };
                    EmailsSentThisSession.Add(emailRecord);
                }

                return ProcessedCount;
            }
            catch (System.Exception ex)
            {
                ErrorOccurred = true;
                LastErrorMessage = ex.Message;
                return ProcessedCount;
            }
        }

        /// <summary>
        /// Test method to create email body for disabled users.
        /// </summary>
        /// <param name="users">List of users to include in the email</param>
        /// <returns>HTML email body</returns>
        public string TestCreateEmailBody(List<DisabledListItem> users)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("The following eGrants accounts have been deactivated due to 60 days of inactivity in the system:");
            sb.AppendLine("<br/>&nbsp;&nbsp;<br/>");
            sb.AppendLine(@"<table style=""padding-top:10px""><tr><th style=""text-align:left"">User</th><th style=""text-align:left"">UserName</th><th style=""text-align:left"">Last Login Date</th></tr>");
            foreach (var user in users)
            {
                sb.AppendLine($"<tr><td>{user.FinalNameForOGA}</td><td>{user.UserIdFromDB}</td><td>{user.LastLoginDateFromDB}</td></tr>");
            }
            sb.AppendLine("</table>");
            return sb.ToString();
        }

        /// <summary>
        /// Test method to filter users with missing info.
        /// Wraps the static method for easier testing.
        /// </summary>
        /// <param name="users">List of users to filter</param>
        /// <returns>Filtered list of users</returns>
        public List<DisabledListItem> TestFilterUsers(List<DisabledListItem> users)
        {
            return Processor.FilterOutUsersWithMissingInfo(users);
        }

        /// <summary>
        /// Adds a simulated disabled user for testing.
        /// </summary>
        /// <param name="firstName">First name</param>
        /// <param name="lastName">Last name</param>
        /// <param name="personName">Person name (for service accounts)</param>
        /// <param name="userId">User ID</param>
        /// <param name="email">Email address</param>
        /// <param name="lastLoginDate">Last login date string</param>
        public void AddSimulatedDisabledUser(
            string firstName,
            string lastName,
            string personName = "",
            string userId = "testuser",
            string email = "test@nih.gov",
            string lastLoginDate = "01/01/2024")
        {
            SimulatedDisabledUsers.Add(new DisabledListItem
            {
                FirstNameFromDB = firstName,
                LastNameFromDB = lastName,
                PersonNameFromDB = personName,
                UserIdFromDB = userId,
                EmailFromDB = email,
                LastLoginDateFromDB = lastLoginDate,
                PersonIdFromDB = SimulatedDisabledUsers.Count + 1
            });
        }

        /// <summary>
        /// Resets the test processor state for a new test run.
        /// </summary>
        public void Reset()
        {
            EmailsSentThisSession.Clear();
            UsersProcessedThisSession.Clear();
            SimulatedDisabledUsers.Clear();
            ProcessedCount = 0;
            ErrorOccurred = false;
            LastErrorMessage = null;
            SimulatedRecipient = "test@nih.gov"; // Reset to default
        }
    }

    /// <summary>
    /// Represents an email that was captured during testing.
    /// </summary>
    public class TestOGAEmailRecord
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public int UserCount { get; set; }
        public DateTime TimeCaptured { get; set; }
    }
}
