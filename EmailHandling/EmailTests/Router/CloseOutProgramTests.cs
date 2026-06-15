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
    public class CloseoutProgramActionRequiredTests
    {
        private string _eGrantsDevEmail = "eGrantsDev@mail.nih.gov";
        //private string _josniEmail = "jonesni@mail.nih.gov";

        [TestMethod]

        [TestCategory("Integration")]
        public void CloseoutProgramActionRequiredSendToDevEmail()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "Closeout Program Action Required: 5R01CA228019-05 - BUTCHER, EUGENE F-RPPR Acceptance Past Due";
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
        public void CloseoutProgramActionRequiredApplId()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "Closeout Program Action Required: 5R01CA228019-05 - BUTCHER, EUGENE F-RPPR Acceptance Past Due";
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

        [TestCategory("Integration")]
        public void CloseoutProgramActionRequiredCheckedSubject()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "Closeout Program Action Required: 5R01CA228019-05 - BUTCHER, EUGENE F-RPPR Acceptance Past Due";
            testEmail.Subject = Subject;
            var Body = " \r\n";
            testEmail.Body = Body;
            var testProcessor = new TestProcessor();

            // Act
            var sentResults = testProcessor.TestSingleEmail(testEmail);

            // Assert
            var subj = sentResults["subject"];
            Assert.IsTrue(subj.Contains("category=closeout, sub=F-RPPR Acceptance Past Due Reminder"));
        }


        [TestMethod]


        [TestCategory("Integration")]
        public void CloseoutProgramActionRequiredSameSubjectNegative()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "Closeout Simulation Action Required: 5R01CA228019-05 - BUTCHER, EUGENE F-RPPR Acceptance Past Due";       //      <----- an off subject (simulation)
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

