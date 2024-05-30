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
    public class SBIRTests
    {
        [TestMethod]
        public void TestSBIRNotCleared()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem) oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "SBIR/STTR Foreign Risk Management: Not Cleared for Funding";
            testEmail.Subject = Subject;
            var Body = "1R44CA288032-01A1 (10931950) has undergone SBIR/STTR risk management assessment in accordance with the SBIR and STTR Extension Act of 2022 on 04/23/2024 11:12 AM. \r\nIC has the discretion to fund 1R44CA288032-01A1 in accordance with the IC’s normal operating procedures and the criteria defined in the SBIR and STTR Extension Act of 2022. If any material changes (e.g., change in personnel, performance sites, or company ownership) are submitted after the assessment date/time of 04/23/2024 11:12 AM, an award cannot be released until a re-assessment has been completed (The IC will be notified via a follow-up notification). This is for internal use only. \r\nSee the Staff Guidance for the NIH SBIR/STTR Foreign Risk Management found on the NIH Extramural Intranet. \r\n";
            testEmail.Body = Body;
            var testProcessor = new TestProcessor();

            // Act
            var sentResults = testProcessor.TestSingleEmail(testEmail);

            // Assert
            Assert.IsTrue(sentResults["subject"].Contains("Not Cleared"));
        }

        [TestMethod]
        public void TestSBIRNotClearedNegative()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            // Note the altered subject (an actual subject from an actual email)
            var Subject = "SBIR/STTR Foreign Risk Management: Cleared for Funding";
            testEmail.Subject = Subject;
            var Body = "1R44CA288032-01A1 (10931950) has undergone SBIR/STTR risk management assessment in accordance with the SBIR and STTR Extension Act of 2022 on 04/23/2024 11:12 AM. \r\nIC has the discretion to fund 1R44CA288032-01A1 in accordance with the IC’s normal operating procedures and the criteria defined in the SBIR and STTR Extension Act of 2022. If any material changes (e.g., change in personnel, performance sites, or company ownership) are submitted after the assessment date/time of 04/23/2024 11:12 AM, an award cannot be released until a re-assessment has been completed (The IC will be notified via a follow-up notification). This is for internal use only. \r\nSee the Staff Guidance for the NIH SBIR/STTR Foreign Risk Management found on the NIH Extramural Intranet. \r\n";
            testEmail.Body = Body;
            var testProcessor = new TestProcessor();
            // Act
            var sentResults = testProcessor.TestSingleEmail(testEmail);

            // Assert
            Assert.IsFalse(sentResults["subject"].Contains("Not Cleared"));
        }

        [TestMethod]
        public void TestSBIRApplIdExtraction()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "SBIR/STTR Foreign Risk Management: Not Cleared for Funding";
            testEmail.Subject = Subject;
            var Body = "1R44CA288032-01A1 (10931950) has undergone SBIR/STTR risk management assessment in accordance with the SBIR and STTR Extension Act of 2022 on 04/23/2024 11:12 AM. \r\nIC has the discretion to fund 1R44CA288032-01A1 in accordance with the IC’s normal operating procedures and the criteria defined in the SBIR and STTR Extension Act of 2022. If any material changes (e.g., change in personnel, performance sites, or company ownership) are submitted after the assessment date/time of 04/23/2024 11:12 AM, an award cannot be released until a re-assessment has been completed (The IC will be notified via a follow-up notification). This is for internal use only. \r\nSee the Staff Guidance for the NIH SBIR/STTR Foreign Risk Management found on the NIH Extramural Intranet. \r\n";
            testEmail.Body = Body;
            var testProcessor = new TestProcessor();

            // Act
            var sentResults = testProcessor.TestSingleEmail(testEmail);

            // Assert
            Assert.IsTrue(sentResults["subject"].Contains("applid=10931950"));
        }

        [TestMethod]
        public void TestSBIRApplIdExtractionNegative()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            var Subject = "SBIR/STTR Foreign Risk Management: Not Cleared for Funding";
            testEmail.Subject = Subject;
            // NOTE : fishy appl id on the next line
            var Body = "1R44CA288032-01A1 (111111) has undergone SBIR/STTR risk management assessment in accordance with the SBIR and STTR Extension Act of 2022 on 04/23/2024 11:12 AM. \r\nIC has the discretion to fund 1R44CA288032-01A1 in accordance with the IC’s normal operating procedures and the criteria defined in the SBIR and STTR Extension Act of 2022. If any material changes (e.g., change in personnel, performance sites, or company ownership) are submitted after the assessment date/time of 04/23/2024 11:12 AM, an award cannot be released until a re-assessment has been completed (The IC will be notified via a follow-up notification). This is for internal use only. \r\nSee the Staff Guidance for the NIH SBIR/STTR Foreign Risk Management found on the NIH Extramural Intranet. \r\n";
            testEmail.Body = Body;
            var testProcessor = new TestProcessor();

            // Act
            var sentResults = testProcessor.TestSingleEmail(testEmail);

            // Assert
            Assert.IsFalse(sentResults["subject"].Contains("applid=10931950"));
        }

    }
}
