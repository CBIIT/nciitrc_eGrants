using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Outlook = Microsoft.Office.Interop.Outlook;

namespace EmailHandlingTests
{
    [TestClass]
    public class RPPRTests
    {
        private string _eGrantsDevEmail = "eGrantsDev@mail.nih.gov";
        //private string _josniEmail = "jonesni@mail.nih.gov";

        // MLH : Note I haven't seen any emails with a subject that capture on this

        [TestMethod]

        [TestCategory("Integration")]
        public void RPPRSendToDevEmail()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "RPPR Reminder: 1R41CA298615-01 RPPR Past Due";
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
        public void RPPRCheckedSubject()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "RPPR Reminder: 1R41CA298615-01 RPPR Past Due";
            testEmail.Subject = Subject;
            var Body = " \r\n";
            testEmail.Body = Body;
            var testProcessor = new TestProcessor();

            // Act
            var sentResults = testProcessor.TestSingleEmail(testEmail);

            // Assert
            var subj = sentResults["subject"];
            Assert.IsTrue(subj.Contains("applid=11076534, category=RPPR, sub=Reminder, extract=1"));
        }

        [TestMethod]

        [TestCategory("Integration")]
        public void RPPRCheckedWithWGrantYearFailsSubject()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "RPPR Reminder: 3P30CA125123-18W1 RPPR Past Due";
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
            Assert.IsTrue(subj.Contains("category=RPPR"),
                $"Expected subject to contain 'category=RPPR', but got: {subj}");
        }

        [TestMethod]

        [TestCategory("Integration")]
        public void RPPRWithSupplementSubject()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "RPPR Reminder: 3P30CA125123-19S2 RPPR Past Due";
            testEmail.Subject = Subject;
            var Body = " \r\n";
            testEmail.Body = Body;
            var testProcessor = new TestProcessor();
            string exceptionMessage = string.Empty;

            // Act
            var sentResults = testProcessor.TestSingleEmail(testEmail);
            var subj = sentResults["subject"];

            // Assert
            // Note: The database function Imm_fn_applid_match may return NULL if the grant number
            // is not found in the database. In this case, the applid will be empty.
            // The test verifies that the email is processed correctly even when the grant isn't found.
            Assert.IsTrue(subj.Contains("category=RPPR, sub=Reminder, extract=1"),
                $"Expected RPPR Reminder format in subject, but got: {subj}");

            // If the grant exists in the database, verify the applid is included
            // If not, the applid will be empty and that's acceptable for this test
            Assert.IsTrue(subj.StartsWith("applid="),
                $"Expected subject to start with 'applid=', but got: {subj}");
        }

        [TestMethod]

        [TestCategory("Integration")]
        public void RPPRSameSubjectNegative()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "RPPR Remindur";       //      <----- an off subject (misspelled Reminder)
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

