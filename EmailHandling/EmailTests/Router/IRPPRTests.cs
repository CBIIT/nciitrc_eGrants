using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Outlook = Microsoft.Office.Interop.Outlook;

namespace EmailTests
{
    [TestClass]
    public class IRPPRTests
    {
        private string _eGrantsDevEmail = "eGrantsDev@mail.nih.gov";
        private string _josniEmail = "jonesni@mail.nih.gov";

        // MLH : Note I haven't seen any emails with a subject that capture on this

        [TestMethod]
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
            string exceptionMessage = string.Empty;

            // Act
            try
            {
                var sentResults = testProcessor.TestSingleEmail(testEmail);
            } catch (Exception ex)
            {
                exceptionMessage = ex.Message;
            }

            // Assert
            Assert.IsTrue(exceptionMessage.Contains("Get Appl Id query failed"));
        }


        [TestMethod]
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
