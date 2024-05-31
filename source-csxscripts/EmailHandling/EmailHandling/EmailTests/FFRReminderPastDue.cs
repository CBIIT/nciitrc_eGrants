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
    public class FFRReminderPastDueRequiredTests
    {
        private string _eGrantsDevEmail = "eGrantsDev@mail.nih.gov";
        private string _josniEmail = "jonesni@mail.nih.gov";

        // MLH : Note I haven't seen any emails with a subject that capture on this

        [TestMethod]
        public void FFRReminderPastDueRequiredSendToDevEmail()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "FFR Reminder FFR Past Due 5R01CA228019-05 - This subject is totally fabricated, find one from production";
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
        public void FFRReminderPastDueRequiredApplId()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "FFR Reminder FFR Past Due 5R01CA228019-05 - This subject is totally fabricated, find one from production";
            testEmail.Subject = Subject;
            var Body = " \r\n";
            testEmail.Body = Body;
            var testProcessor = new TestProcessor();

            // Act
            var sentResults = testProcessor.TestSingleEmail(testEmail);

            // Assert
            var subj = sentResults["subject"];
            Assert.IsTrue(subj.Contains("applid=10532149"));
        }

        [TestMethod]
        public void FFRReminderPastDueRequiredCheckedSubject()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "FFR Reminder FFR Past Due 5R01CA228019-05 - This subject is totally fabricated, find one from production";
            testEmail.Subject = Subject;
            var Body = " \r\n";
            testEmail.Body = Body;
            var testProcessor = new TestProcessor();

            // Act
            var sentResults = testProcessor.TestSingleEmail(testEmail);

            // Assert
            var subj = sentResults["subject"];
            Assert.IsTrue(subj.Contains("category=FFR, sub=Reminder, extract=1"));
        }


        [TestMethod]
        public void FFRReminderPastDueRequiredSameSubjectNegative()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "FFR Reminder FFR Pre Due 5R01CA228019-05 - This subject is totally fabricated, find one from production";       //      <----- an off subject (pre instead of post)
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
