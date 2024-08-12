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
    public class ChangeNoticeForTests
    {
        private string _eGrantsDevEmail = "eGrantsDev@mail.nih.gov";
        private string _josniEmail = "jonesni@mail.nih.gov";

        // MLH : Note that the subject capture for this is : "CHANGE_NOTICE_FOR" ** and ** "Application is withdrawn request"
        // the closest email I could find was CHANGE_NOTICE_FOR 1R41CA298615-01     - Application number has changed.
        // But that would never capture (bc it's "Application number" and not "Application is withdrawn")
        // So I fabricated this :
        // CHANGE_NOTICE_FOR 1R41CA298615-01     - Application is withdrawn request
        // because I think it will probably be similar enough and should still work

        [TestMethod]
        public void ChangeNoticeForSendToDevEmail()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "CHANGE_NOTICE_FOR 1R41CA298615-01     - Application is withdrawn request";
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
        public void ChangeNoticeForAdjustedSubject()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "CHANGE_NOTICE_FOR 1R41CA298615-01     - Application is withdrawn request";
            testEmail.Subject = Subject;
            var Body = " \r\n";
            testEmail.Body = Body;
            var testProcessor = new TestProcessor();

            // Act
            var sentResults = testProcessor.TestSingleEmail(testEmail);

            // Assert
            var subj = sentResults["subject"];
            Assert.IsTrue(subj.Contains("applid=11076534, category=eRA Notification, sub=Application Withdrawn"));
        }


        [TestMethod]
        public void ChangeNoticeForSameSubjectNegative()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "CHANGE_NINJAS_FOR 1R41CA298615-01     - Application is withdrawn request";       //      <----- an off subject (NINJAS)
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
        public void ChangeNoticeForSameSubjectNegative2()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "CHANGE_NOTICE_FOR 1R41CA298615-01     - Application is belong to us";       //      <----- an off subject (is belong to us)
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
