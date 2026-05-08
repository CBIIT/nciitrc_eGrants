using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using AddSuppProd;
using CommonUtilties;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace EmailTests.AddSuppProd
{
/// <summary>
    /// Test processor that extends AddSuppProd.Processor to intercept email processing
    /// and capture processing details for test verification.
    /// 
    /// This allows testing the processor logic without requiring:
    /// - Active Outlook connection
    /// - Real email folders
    /// - Database connections
    /// </summary>
    internal class TestAddSuppProdProcessor : Processor
    {
        /// <summary>
        /// Tracks all items that would have been processed during the test session.
   /// </summary>
   public List<TestProcessedItem> ItemsProcessedThisSession { get; } = new List<TestProcessedItem>();

        /// <summary>
        /// Count of items processed during the test.
        /// </summary>
  public int ProcessedCount { get; private set; } = 0;

  /// <summary>
        /// Indicates if an error occurred during processing.
        /// </summary>
   public bool ErrorOccurred { get; private set; } = false;

        /// <summary>
        /// Error message if an error occurred.
        /// </summary>
        public string LastErrorMessage { get; private set; } = null;

        /// <summary>
        /// Simulated items to process (for testing without Outlook).
        /// </summary>
        public List<SimulatedMailItem> SimulatedMailItems { get; set; } = new List<SimulatedMailItem>();

    /// <summary>
        /// Simulated directory path for testing.
        /// </summary>
        public string SimulatedDirPath { get; set; } = @"Test\Folder\Path";

  /// <summary>
    /// Simulated output directory for testing.
        /// </summary>
        public string SimulatedOutDir { get; set; } = @"C:\Test\Output";

 /// <summary>
        /// Test method to process simulated items without Outlook/database access.
        /// </summary>
        /// <param name="verbose">Verbose mode flag</param>
        /// <returns>Number of items processed</returns>
     public int TestProcessSimulatedItems(string verbose = "n")
        {
   try
            {
      ProcessedCount = 0;

      foreach (var item in SimulatedMailItems)
              {
          ProcessedCount++;

              var processedItem = new TestProcessedItem
          {
   Subject = item.Subject,
             Body = item.Body,
         SenderEmail = item.SenderEmail,
      ReceivedTime = item.ReceivedTime,
            ProcessedTime = DateTime.Now,
      WasMovedToOld = true
    };

   ItemsProcessedThisSession.Add(processedItem);

          CommonUtilities.ShowDiagnosticIfVerbose(
              $"TEST: Processed item with subject: {item.Subject}", verbose);
         }

    return ProcessedCount;
            }
   catch (Exception ex)
            {
      ErrorOccurred = true;
            LastErrorMessage = ex.Message;
           return ProcessedCount;
            }
        }

        /// <summary>
  /// Test method to simulate processing a single mail item.
     /// </summary>
        /// <param name="subject">Email subject</param>
        /// <param name="body">Email body</param>
        /// <param name="senderEmail">Sender email address</param>
        /// <param name="verbose">Verbose mode flag</param>
        /// <returns>The processed item record</returns>
      public TestProcessedItem TestProcessSingleItem(string subject, string body,
            string senderEmail = "test@nih.gov", string verbose = "n")
        {
  try
            {
     ProcessedCount++;

          var processedItem = new TestProcessedItem
   {
    Subject = subject,
          Body = body,
 SenderEmail = senderEmail,
      ReceivedTime = DateTime.Now.AddMinutes(-5),
          ProcessedTime = DateTime.Now,
WasMovedToOld = true
     };

   ItemsProcessedThisSession.Add(processedItem);

   CommonUtilities.ShowDiagnosticIfVerbose(
      $"TEST: Processed single item with subject: {subject}", verbose);

          return processedItem;
       }
            catch (Exception ex)
            {
      ErrorOccurred = true;
           LastErrorMessage = ex.Message;
   return null;
      }
        }

     /// <summary>
     /// Clears all recorded items and resets counters.
        /// </summary>
      public void Reset()
   {
            ItemsProcessedThisSession.Clear();
          ProcessedCount = 0;
          ErrorOccurred = false;
            LastErrorMessage = null;
          SimulatedMailItems.Clear();
   }

        /// <summary>
        /// Adds a simulated mail item for testing.
        /// </summary>
  public void AddSimulatedMailItem(string subject, string body,
  string senderEmail = "test@nih.gov", DateTime? receivedTime = null)
        {
     SimulatedMailItems.Add(new SimulatedMailItem
    {
              Subject = subject,
    Body = body,
  SenderEmail = senderEmail,
          ReceivedTime = receivedTime ?? DateTime.Now.AddMinutes(-10)
   });
        }
}

    /// <summary>
    /// Record class to store processed item details during testing.
    /// </summary>
    public class TestProcessedItem
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public string SenderEmail { get; set; }
        public DateTime ReceivedTime { get; set; }
   public DateTime ProcessedTime { get; set; }
        public bool WasMovedToOld { get; set; }
    }

    /// <summary>
    /// Simulated mail item for testing without Outlook.
    /// </summary>
    public class SimulatedMailItem
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public string SenderEmail { get; set; }
     public DateTime ReceivedTime { get; set; }
    }
}
