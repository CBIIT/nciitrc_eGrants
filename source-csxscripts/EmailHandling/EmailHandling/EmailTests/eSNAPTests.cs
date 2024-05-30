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
    public class eSNAPTests
    {
        // NB : an extra email gets sent out if the subject contains "submitted to NIH with a Non-Compliance"

        private string _eGrantsDevEmail = "eGrantsDev@mail.nih.gov";
        private string _josniEmail = "jonesni@mail.nih.gov";

        [TestMethod]
        public void TestReceivedByAgencySendToDevEmail()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "eRA Commons: RPPR for Grant R01CA274978-02 Received by Agency";
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
        public void TestReceivedByAgencySendNoNonComplianceEmails()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "eRA Commons: RPPR for Grant R01CA274978-02 Received by Agency";
            testEmail.Subject = Subject;
            var Body = " \r\n";
            testEmail.Body = Body;
            var testProcessor = new TestProcessor();
            ;
            // Act
            var sentResults = testProcessor.TestSingleEmail(testEmail);

            // Assert
            Assert.IsFalse(sentResults["subject"].Contains("category=eRANotification, sub=RPPR Non-Compliance"));
        }

        [TestMethod]
        public void TestNonCompliance()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "eRA Commons: RPPR for Grant R01CA278703-02 submitted to NIH with a Non-Compliance warning";
            testEmail.Subject = Subject;
            var Body = " \r\n";
            testEmail.Body = Body;
            var testProcessor = new TestProcessor();
            ;
            // Act
            var sentResults = testProcessor.TestSingleEmail(testEmail);

            // Assert
            Assert.IsTrue(sentResults["subject"].Contains("category=eRANotification, sub=RPPR Non-Compliance"));
        }

    }
}
