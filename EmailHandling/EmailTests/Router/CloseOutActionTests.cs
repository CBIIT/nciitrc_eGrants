using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Outlook = Microsoft.Office.Interop.Outlook;

namespace EmailTests
{
    /// <summary>
    /// Tests for "Closeout Action Required" emails.
    /// 
    /// NOTE: The Router currently handles these subject patterns for closeout:
    /// - "urgent: closeout reports overdue" (case insensitive)
 /// - "closeout program action required" (case insensitive)
    /// 
    /// The subject "Closeout Action Required:" (without "Program" or "urgent") 
  /// is NOT currently handled by the Router. These tests verify that behavior.
    /// 
    /// If this email type should be processed in the future, the Router's 
    /// HandleSingleEmail method needs to be updated to include this pattern.
    /// </summary>
    [TestClass]
    public class CloseoutActionRequiredTests
  {
        private string _eGrantsDevEmail = "eGrantsDev@mail.nih.gov";
        private string _josniEmail = "jonesni@mail.nih.gov";

        /// <summary>
        /// Tests that "Closeout Action Required:" emails (without "Program" keyword)
        /// are NOT currently processed by the Router.
        /// This is expected behavior based on current Router implementation.
        /// </summary>
        [TestMethod]
  public void CloseoutActionRequired_NotCurrentlyHandled()
        {
    // Arrange
 Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
 // Note: This subject does NOT contain "Program" or "urgent", so it won't be processed
       var Subject = "Closeout Action Required: 1R44CA256984-01A1 - Pasche, Valerie Past Due Closeout Documents";
            testEmail.Subject = Subject;
       var Body = " \r\n";
 testEmail.Body = Body;
       var testProcessor = new TestProcessor();

            // Act
     var sentResults = testProcessor.TestSingleEmail(testEmail);

  // Assert - This email type is NOT currently handled by the Router
     // so no subject or recipients should be set
            Assert.IsFalse(sentResults.ContainsKey("subject"), 
         "Closeout Action Required (without 'Program') should NOT be processed by current Router");
        }

        /// <summary>
        /// Tests that the "urgent: closeout reports overdue" pattern IS handled.
        /// </summary>
  [TestMethod]
        public void UrgentCloseoutReportsOverdue_SendToDevEmail()
        {
            // Arrange
            Outlook.Application oApp = new Outlook.Application();
        var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
       // This pattern IS handled by the Router
    var Subject = "URGENT: Closeout Reports Overdue for 1R44CA256984-01A1";
      testEmail.Subject = Subject;
  var Body = " \r\n";
            testEmail.Body = Body;
          var testProcessor = new TestProcessor();

            // Act
var sentResults = testProcessor.TestSingleEmail(testEmail);

   // Assert
            Assert.IsTrue(sentResults.ContainsKey("recipients"), "Should have recipients");
       Assert.IsTrue(sentResults["recipients"].Contains(_eGrantsDevEmail));
        }

        /// <summary>
 /// Tests that "urgent: closeout reports overdue" emails have correct applid.
        /// </summary>
     [TestMethod]
        public void UrgentCloseoutReportsOverdue_CheckApplId()
        {
            // Arrange
  Outlook.Application oApp = new Outlook.Application();
            var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
      var Subject = "URGENT: Closeout Reports Overdue for 1R44CA256984-01A1";
       testEmail.Subject = Subject;
         var Body = " \r\n";
            testEmail.Body = Body;
 var testProcessor = new TestProcessor();

            // Act
         var sentResults = testProcessor.TestSingleEmail(testEmail);

  // Assert
 Assert.IsTrue(sentResults.ContainsKey("subject"), "Should have subject");
 var subj = sentResults["subject"];
        Assert.IsTrue(subj.Contains("applid="), "Subject should contain applid");
        }

/// <summary>
        /// Tests that "urgent: closeout reports overdue" emails have correct category.
        /// </summary>
        [TestMethod]
        public void UrgentCloseoutReportsOverdue_CheckCategory()
  {
            // Arrange
          Outlook.Application oApp = new Outlook.Application();
         var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
   var Subject = "URGENT: Closeout Reports Overdue for 1R44CA256984-01A1";
   testEmail.Subject = Subject;
            var Body = " \r\n";
            testEmail.Body = Body;
            var testProcessor = new TestProcessor();

         // Act
  var sentResults = testProcessor.TestSingleEmail(testEmail);

            // Assert
   Assert.IsTrue(sentResults.ContainsKey("subject"), "Should have subject");
   var subj = sentResults["subject"];
         Assert.IsTrue(subj.Contains("category=closeout, sub=Past Due Documents Reminder"),
       "Subject should contain correct category");
        }

    /// <summary>
        /// Negative test - altered subject should not be processed.
        /// </summary>
    [TestMethod]
     public void CloseoutActionRequiredSameSubjectNegative()
        {
            // Arrange
     Outlook.Application oApp = new Outlook.Application();
        var testEmail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
  var Subject = "Closeout Action Passiveness: 1R44CA256984-01A1 - Pasche, Valerie Past Due Closeout Documents";
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
