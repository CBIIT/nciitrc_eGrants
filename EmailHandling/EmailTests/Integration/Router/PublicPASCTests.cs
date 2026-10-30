using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmailHandlingTests.Shared;

using Outlook = Microsoft.Office.Interop.Outlook;
using EmailHandlingTests.Shared;

namespace EmailHandlingTests.Integration.Router
{
    [TestClass]
    public class PublicPASCTests
    {
        private string _eGrantsDevEmail = "eGrantsDev@mail.nih.gov";
        //private string _josniEmail = "jonesni@mail.nih.gov";

        // MLH : Note this was originally manually tested, probably because I didn't have any examples
        // We do have examples now though ... very conflicted examples

        // PASC: 5U24CA213274 - 08 - RUDIN, CHARLES M
        // Compliant PASC: 5R01CA258784 - 04 - SEN, TRIPARNA
        // Compliant: PASC: 5U01CA253915-06 - ETZIONI, RUTH D

        [TestMethod]

        [TestCategory("Integration")]
        public void PASCSendToDevEmail()
        {
            // not compliant at all

            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "PASC: 5U24CA213274 - 08 - RUDIN, CHARLES M";
            testEmail.Subject = Subject;
            var Body = " \r\n";
            testEmail.Body = Body;

            string criticalEmailAddress = "public@public.com";

            var testProcessor = new TestProcessor();

            // Act
            var sentResults = testProcessor.TestSingleEmail(testEmail, criticalEmailAddress);

            // Assert
            Assert.IsTrue(sentResults["recipients"].Contains(_eGrantsDevEmail));
        }

        [TestMethod]

        [TestCategory("Integration")]
        public void PASCSendAltFormatToDevEmail()
        {
            // compliant and one colon

            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "Compliant PASC: 5R01CA258784 - 04 - SEN, TRIPARNA";
            testEmail.Subject = Subject;
            var Body = " \r\n";
            testEmail.Body = Body;

            string criticalEmailAddress = "public@public.com";

            var testProcessor = new TestProcessor();

            // Act
            var sentResults = testProcessor.TestSingleEmail(testEmail, criticalEmailAddress);

            // Assert
            Assert.IsTrue(sentResults["recipients"].Contains(_eGrantsDevEmail));
        }

        [TestMethod]

        [TestCategory("Integration")]
        public void PASCSendAltAltFormatToDevEmail()
        {
            // compliant and two colons !!

            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "Compliant: PASC: 5U01CA253915-06 - ETZIONI, RUTH D";
            testEmail.Subject = Subject;
            var Body = " \r\n";
            testEmail.Body = Body;

            string criticalEmailAddress = "public@public.com";

            var testProcessor = new TestProcessor();

            // Act
            var sentResults = testProcessor.TestSingleEmail(testEmail, criticalEmailAddress);

            // Assert
            Assert.IsTrue(sentResults["recipients"].Contains(_eGrantsDevEmail));
        }

        [TestMethod]

        [TestCategory("Integration")]
        public void PASCSendToDevEmailNegative()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "This subject is wildly off and should NEVER succeed here (should have grant Id in here)";
            testEmail.Subject = Subject;
            var Body = " \r\n";
            testEmail.Body = Body;
            string criticalEmailAddress = "public@public.com";
            var testProcessor = new TestProcessor();

            // Act
            var sentResults = testProcessor.TestSingleEmail(testEmail, criticalEmailAddress);
            var subj = sentResults["subject"];

            // Assert
            // When no grant number is found in the subject, the database function returns NULL
            // and the applid should be empty, but the email should still be processed
            Assert.IsTrue(subj.Contains("applid="),
                $"Expected subject to contain 'applid=', but got: {subj}");
            Assert.IsTrue(subj.Contains("category=PublicAccess"),
                $"Expected subject to contain 'category=PublicAccess', but got: {subj}");
        }

    }
}

