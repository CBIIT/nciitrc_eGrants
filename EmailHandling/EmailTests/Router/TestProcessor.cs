using CommonUtilties;
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
        private string _testSender = null;

        /// <summary>
        /// Overload using COM object (has registry problems)
        /// </summary>
        /// <param name="testEmail"></param>
        /// <returns></returns>
        internal Dictionary<string, string> TestSingleEmail(dynamic testEmail, string sender = null)
        {
            var config = AppConfig.Load();
            var dirPath = config["AppSettings:LogDir"] ?? @"C:\egrants\apps\log\";
            var conStr = AppConfig.GetConnectionString(config, "EIM");
            var verbose = config["AppSettings:Verbose"] ?? "n";

            if (!string.IsNullOrWhiteSpace(sender))
            {
                _testSender = sender;
            } else
            {
                _testSender = "anyNynn@anyNynn.com";    // hello every nynn !
            }

            var debug = "y";    // NEVER send out emails from these tests
            SqlConnection connection = new SqlConnection(conStr);
            connection.Open();

            HandleSingleEmail((string)testEmail.Subject, (string)testEmail.Body, (string)testEmail.Subject, verbose, connection, debug);
            var result = emailsSentThisSession;
            return result;
        }

        public override string GetSenderId(RouterMailItem testEmail)
        {
            return _testSender;
        }

        internal Dictionary<string, string> TestSingleEmail(string From, string Subject, string Body)
        {
            var config = AppConfig.Load();
            var dirPath = config["AppSettings:LogDir"] ?? @"C:\egrants\apps\log\";
            var conStr = AppConfig.GetConnectionString(config, "EIM");
            var verbose = config["AppSettings:Verbose"] ?? "n";
            var debug = "y";    // NEVER send out emails from these tests
            SqlConnection connection = new SqlConnection(conStr);
            connection.Open();

            HandleSingleEmail(From, Subject, Body, verbose, connection, debug);

            var result = emailsSentThisSession;
            return result;
        }

        protected override Dictionary<string, string> Send(RouterOutgoingMail mailItem)
        {
            // don't send here because this is the test method, just gather info to be returned to test method

            var recipients = mailItem.Recipients;

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
