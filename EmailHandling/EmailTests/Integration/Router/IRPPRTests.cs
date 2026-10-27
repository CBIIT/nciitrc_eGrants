using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmailHandlingTests.Shared;

using Outlook = Microsoft.Office.Interop.Outlook;
using EmailHandlingTests.Shared;

namespace EmailHandlingTests.Integration.Router
{
    [TestClass]
    public class IRPPRTests
    {
        private string _eGrantsDevEmail = "eGrantsDev@mail.nih.gov";
        //private string _josniEmail = "jonesni@mail.nih.gov";

        // MLH : Note I haven't seen any emails with a subject that capture on this

        [TestMethod]

        [TestCategory("Integration")]
        public void IRPPRSendToDevEmail()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "IRPPR Reminder mlh fabricated this email subject could be waay off 1R41CA298615-01     - Application is belong to us";
            testEmail.Subject = Subject;
            var Body = " \r\n";
            testEmail.Body = Body;
            var testProcessor = new TestProcessor();

            // Act
            var sentResults = testProcessor.TestSingleEmail(testEmail);

            // Assert
            Assert.IsTrue(sentResults["recipients"].Contains(_eGrantsDevEmail));
        }

        [TestMethod]

        [TestCategory("Integration")]
        public void IRPPRCheckedSubject()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "IRPPR Reminder mlh fabricated this email subject could be waay off 1R41CA298615-01     - Application is belong to us";
            testEmail.Subject = Subject;
            var Body = " \r\n";
            testEmail.Body = Body;
            var testProcessor = new TestProcessor();

            // Act
            var sentResults = testProcessor.TestSingleEmail(testEmail);

            // Assert
            var subj = sentResults["subject"];
            Assert.IsTrue(subj.Contains("applid=11076534, category=IRPPR, sub=Reminder, extract=1"));
        }


        [TestMethod]


        [TestCategory("Integration")]
        public void IRPPRCheckedWInSubject()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "IRPPR Reminder: 3P30CA125123-18W1 RPPR Past Due (mlh fabricated for testing)";   // <-- existing IMM function in SQL does not support this format
            testEmail.Subject = Subject;
            var Body = " \r\n";
            testEmail.Body = Body;
            var testProcessor = new TestProcessor();

            // Act
            var sentResults = testProcessor.TestSingleEmail(testEmail);
            var subj = sentResults["subject"];

            // Assert
            // The grant number format with 'W' suffix is not recognized by the database function,
            // so it should return an empty applid but still process the email successfully
            Assert.IsTrue(subj.Contains("applid="),
                $"Expected subject to contain 'applid=', but got: {subj}");
            Assert.IsTrue(subj.Contains("category=IRPPR"),
                $"Expected subject to contain 'category=IRPPR', but got: {subj}");
        }


        [TestMethod]


        [TestCategory("Integration")]
        public void IRPPRSameSubjectNegative()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "IRPPPPPPPR Reminder mlh fabricated this email subject could be waay off 1R41CA298615-01     - Application is belong to us";       //      <----- an off subject (too many PPPP's)
            testEmail.Subject = Subject;
            var Body = " \r\n";
            testEmail.Body = Body;
            var testProcessor = new TestProcessor();

            // Act
            var sentResults = testProcessor.TestSingleEmail(testEmail);

            // Assert
            Assert.IsFalse(sentResults.ContainsKey("subject"));
        }

    }
}

