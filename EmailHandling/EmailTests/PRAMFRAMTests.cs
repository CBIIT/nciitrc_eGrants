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
    public class PRAMFRAMTests
    {
        private string _eGrantsDevEmail = "eGrantsDev@mail.nih.gov";
        private string _josniEmail = "jonesni@mail.nih.gov";

        // MLH : These tests use an actual FRAM subject I found, but I never saw a correspondin PRAM email

        [TestMethod]
        public void FRAMSendToDevEmail()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "RE: FRAM Requested for 5 K08 CA230185-05 PD/PI: Irwin, Kelly; Identifying Information: K08 CA230185 - Non-compliant publication";
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
        public void FRAMAdjustedSubject()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "RE: FRAM Requested for 5 K08 CA230185-05 PD/PI: Irwin, Kelly; Identifying Information: K08 CA230185 - Non-compliant publication";
            testEmail.Subject = Subject;
            var Body = " \r\n";
            testEmail.Body = Body;
            var testProcessor = new TestProcessor();

            // Act
            var sentResults = testProcessor.TestSingleEmail(testEmail);

            // Assert
            var subj = sentResults["subject"];
            Assert.IsTrue(subj.Contains("applid=10475597, category=FRAM, sub=Request"));
        }

        [TestMethod]
        public void PRAMAdjustedSubject()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "RE: PRAM Requested for 5 K08 CA230185-05 PD/PI: Irwin, Kelly; Identifying Information: K08 CA230185 - Non-compliant publication";
            testEmail.Subject = Subject;
            var Body = " \r\n";
            testEmail.Body = Body;
            var testProcessor = new TestProcessor();

            // Act
            var sentResults = testProcessor.TestSingleEmail(testEmail);

            // Assert
            var subj = sentResults["subject"];
            Assert.IsTrue(subj.Contains("applid=10475597, category=PRAM, sub=Request"));
        }


        [TestMethod]
        public void FRAMSameSubjectNegative()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            // off subject : FRAM changed to JAM
            var Subject = "RE: JAM Requested for 5 K08 CA230185-05 PD/PI: Irwin, Kelly; Identifying Information: K08 CA230185 - Non-compliant publication";       //      <----- an off subject (FCOL instead of FCOI)
            testEmail.Subject = Subject;
            var Body = " \r\n";
            testEmail.Body = Body;
            var testProcessor = new TestProcessor();

            // Act
            var sentResults = testProcessor.TestSingleEmail(testEmail);

            // Assert
            Assert.IsFalse(sentResults.ContainsKey("subject"));
        }

        [TestMethod]
        public void PRAMSameSubjectNegative()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            // off subject : Requested changed to Rejected
            var Subject = "RE: PRAM Rejected for 5 K08 CA230185-05 PD/PI: Irwin, Kelly; Identifying Information: K08 CA230185 - Non-compliant publication";       //      <----- an off subject (FCOL instead of FCOI)
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
