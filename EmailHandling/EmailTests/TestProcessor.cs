using Microsoft.Office.Interop.Outlook;
using Router;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailTests
{

    internal class TestProcessor : Processor
    {
        /// <summary>
        /// Overload using COM object (has registry problems)
        /// </summary>
        /// <param name="testEmail"></param>
        /// <returns></returns>
        internal Dictionary<string, string> TestSingleEmail(MailItem testEmail)
        {
            var dirPath = TestUtilities.GetConfigVal("logDir");
            var conStr = TestUtilities.GetConfigVal("conStr");
            var verbose = TestUtilities.GetConfigVal("Verbose");
            var debug = "y";    // NEVER send out emails from these tests
            SqlConnection connection = new SqlConnection(conStr);
            connection.Open();

            HandleSingleEmail(testEmail, testEmail.Subject, testEmail.Body, verbose, connection, debug);
            var result = emailsSentThisSession;
            return result;
        }

        internal Dictionary<string, string> TestSingleEmail(string From, string Subject, string Body)
        {
            var dirPath = TestUtilities.GetConfigVal("logDir");
            var conStr = TestUtilities.GetConfigVal("conStr");
            var verbose = TestUtilities.GetConfigVal("Verbose");
            var debug = "y";    // NEVER send out emails from these tests
            SqlConnection connection = new SqlConnection(conStr);
            connection.Open();

            HandleSingleEmail(From, Subject, Body, verbose, connection, debug);
            //public void HandleSingleEmail(string from, string v_SubLine, string v_Body, string verbose, SqlConnection con, string debug)
            //HandleSingleEmail(testEmail, testEmail.Subject, testEmail.Body, verbose, connection, debug);

            var result = emailsSentThisSession;
            return result;
        }

        protected override Dictionary<string, string> Send(MailItem mailItem)
        {
            // don't send here because this is the test method, just gather info to be returned to test method

            var recipients = new List<string>();

            foreach (Recipient recipient in mailItem.Recipients)
            {
                // somehow recipient.Address is always null and the email isn't in the object
                recipients.Add(recipient.Name);     
            }

            if (emailsSentThisSession.ContainsKey("recipients"))
            {
                var combinedFromHere = String.Join(", ", recipients.ToArray());
                emailsSentThisSession["recipients"] = $"{emailsSentThisSession["recipients"]},{combinedFromHere}";
            } else
            {
                emailsSentThisSession["recipients"] = String.Join(", ", recipients.ToArray());
            }

            if (emailsSentThisSession.ContainsKey("subject"))
            {
                emailsSentThisSession["subject"] = $"{emailsSentThisSession["subject"]},{mailItem.Subject}";
            } else
            {
                emailsSentThisSession["subject"] = mailItem.Subject;
            }
            

            return null;
        }
    }

}
