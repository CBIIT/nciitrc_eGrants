using System;
using System.Collections.Generic;
using DocManEmail;
using CommonUtilties;

namespace EmailHandlingTests.Unit.DocManEmail
{
    internal class TestDocManEmailProcessor : Processor
    {
        public List<TestDocumentRecord> DocumentsProcessedThisSession { get; } = new List<TestDocumentRecord>();
        public int ProcessedCount { get; private set; }
        public bool ErrorOccurred { get; private set; }
        public string LastErrorMessage { get; private set; }
        public List<SimulatedDocManEmail> SimulatedEmails { get; set; } = new List<SimulatedDocManEmail>();

        public TestDocumentRecord TestProcessSingleDocument(string subject, string senderEmail = "user@nih.gov",
  int attachmentCount = 1, string verbose = "n")
        {
  try
 {
          string cpiid = ExtractValuePublic(ExtractElementPublic(subject, 1), "cpiid");
     string docid = ExtractValuePublic(ExtractElementPublic(subject, 1), "docid");

       if (string.IsNullOrWhiteSpace(cpiid) && string.IsNullOrWhiteSpace(docid))
          {
        return null;
       }

    ProcessedCount++;

   var record = new TestDocumentRecord
     {
         Subject = subject,
     SenderEmail = senderEmail,
   CpiId = cpiid,
  DocId = docid,
         CategoryId = ExtractValuePublic(ExtractElementPublic(subject, 2), "catid"),
    SequenceNumber = ExtractValuePublic(ExtractElementPublic(subject, 3), "num"),
              DocumentDate = ExtractValuePublic(ExtractElementPublic(subject, 4), "date"),
    Reason = ExtractValuePublic(ExtractElementPublic(subject, 3), "reason"),
         AttachmentCount = attachmentCount,
ProcessedTime = DateTime.Now,
 WasMovedToOld = true
      };

          DocumentsProcessedThisSession.Add(record);
    return record;
   }
catch (Exception ex)
     {
         ErrorOccurred = true;
     LastErrorMessage = ex.Message;
     return null;
       }
     }

        public int TestProcessSimulatedEmails(string verbose = "n")
        {
try
            {
            int processed = 0;
   foreach (var email in SimulatedEmails)
      {
      var result = TestProcessSingleDocument(email.Subject, email.SenderEmail, email.AttachmentCount, verbose);
       if (result != null) processed++;
   }
      return processed;
    }
     catch (Exception ex)
       {
       ErrorOccurred = true;
 LastErrorMessage = ex.Message;
           return ProcessedCount;
    }
 }

        public string ExtractElementPublic(string str, int n)
        {
  string[] parts = str.Split(',');
return (n > 0 && n <= parts.Length) ? parts[n - 1].Trim() : "";
    }

     public string ExtractValuePublic(string p, string name)
        {
     if (string.IsNullOrEmpty(p)) return null;
   string[] parts = p.Split('=');
            return (parts.Length == 2 && parts[0].Trim().ToLower().Contains(name)) ? parts[1].Trim() : null;
 }

   public bool IsValidDocManSubject(string subject)
        {
   string cpiid = ExtractValuePublic(ExtractElementPublic(subject, 1), "cpiid");
            string docid = ExtractValuePublic(ExtractElementPublic(subject, 1), "docid");
 return !string.IsNullOrWhiteSpace(cpiid) || !string.IsNullOrWhiteSpace(docid);
 }

     public void AddSimulatedEmail(string subject, string senderEmail = "user@nih.gov", int attachmentCount = 1)
     {
   SimulatedEmails.Add(new SimulatedDocManEmail
            {
       Subject = subject,
   SenderEmail = senderEmail,
           AttachmentCount = attachmentCount,
              ReceivedTime = DateTime.Now.AddMinutes(-10)
            });
}

        public void Reset()
        {
DocumentsProcessedThisSession.Clear();
    ProcessedCount = 0;
            ErrorOccurred = false;
   LastErrorMessage = null;
   SimulatedEmails.Clear();
        }
    }

    public class TestDocumentRecord
    {
public string Subject { get; set; }
        public string SenderEmail { get; set; }
    public string CpiId { get; set; }
        public string DocId { get; set; }
     public string CategoryId { get; set; }
        public string SequenceNumber { get; set; }
        public string DocumentDate { get; set; }
        public string Reason { get; set; }
    public int AttachmentCount { get; set; }
        public DateTime ProcessedTime { get; set; }
        public bool WasMovedToOld { get; set; }
    }

    public class SimulatedDocManEmail
{
        public string Subject { get; set; }
        public string SenderEmail { get; set; }
        public int AttachmentCount { get; set; }
        public DateTime ReceivedTime { get; set; }
    }
}
