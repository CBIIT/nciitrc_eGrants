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
    public class ICActionRequiredTests
    {
        private string _eGrantsDevEmail = "eGrantsDev@mail.nih.gov";
        private string _josniEmail = "jonesni@mail.nih.gov";

        [TestMethod]
        public void TestICActionRequiredSendToDevEmail()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "IC ACTION REQUIRED - Relinquishing Statement";
            testEmail.Subject = Subject;
            var Body = " \r\n";
            testEmail.Body = Body;
            var testProcessor = new TestProcessor();
            ;
            // Act
            var sentResults = testProcessor.TestSingleEmail(testEmail);

            // Assert
            Assert.IsTrue(sentResults["recipients"].Contains(_eGrantsDevEmail));
        }

        [TestMethod]
        public void TestICActionRequiredSameSubject()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "IC ACTION REQUIRED - Relinquishing Statement";
            testEmail.Subject = Subject;
            var Body = " \r\n";
            testEmail.Body = Body;
            var testProcessor = new TestProcessor();
            ;
            // Act
            var sentResults = testProcessor.TestSingleEmail(testEmail);

            // Assert
            Assert.IsTrue(sentResults["subject"].Contains(Subject));
        }


        [TestMethod]
        public void TestICActionRequiredSameSubjectNegative()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "IC ACTION - Relish Statement";       //      <----- an off subject
            testEmail.Subject = Subject;
            var Body = " \r\n";
            testEmail.Body = Body;
            var testProcessor = new TestProcessor();
            ;
            // Act
            var sentResults = testProcessor.TestSingleEmail(testEmail);

            // Assert
            Assert.IsFalse(sentResults.ContainsKey("subject"));
        }

    }
}
