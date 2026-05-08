using System;
using System.Collections.Generic;
using ExchangeFixed;
using CommonUtilties;

namespace EmailTests.ExchangeFixed
{
    internal class TestExchangeFixedProcessor : Processor
    {
        public List<TestExchangeEmailRecord> EmailsProcessedThisSession { get; } = new List<TestExchangeEmailRecord>();
        public List<TestFileSaveOperation> FileSaveOperations { get; } = new List<TestFileSaveOperation>();
        public int ProcessedCount { get; private set; }
     public bool ErrorOccurred { get; private set; }
        public string LastErrorMessage { get; private set; }
        public List<SimulatedExchangeEmail> SimulatedEmails { get; set; } = new List<SimulatedExchangeEmail>();

        public TestExchangeEmailRecord TestProcessSingleEmail(string subject, string body = "",
            string senderEmail = "user@nih.gov", int attachmentCount = 0, string verbose = "n")
     {
   try
            {
  var parsedParams = ParseSubjectLinePublic(subject);

     if (string.IsNullOrEmpty(parsedParams.GrantNumber) && string.IsNullOrEmpty(parsedParams.ApplId))
     {
            return null;
 }

        ProcessedCount++;

       var record = new TestExchangeEmailRecord
   {
            Subject = subject,
      Body = body,
      SenderEmail = senderEmail,
       GrantNumber = parsedParams.GrantNumber,
           Category = parsedParams.Category ?? "Correspondence",
ApplId = parsedParams.ApplId,
             SubCategory = parsedParams.SubCategory,
   Extract = parsedParams.Extract ?? "1",
    AttachmentCount = attachmentCount,
     ProcessedTime = DateTime.Now
     };

              EmailsProcessedThisSession.Add(record);

     // Simulate file save operations based on extract type
         string extract = parsedParams.Extract ?? "1";
  if (extract == "1" || extract == "3")
   {
     FileSaveOperations.Add(new TestFileSaveOperation
      {
        FileName = "placeholder.txt",
       FileType = "txt",
            SaveType = "EmailBody"
           });
       }
           if ((extract == "2" || extract == "3") && attachmentCount > 0)
          {
   FileSaveOperations.Add(new TestFileSaveOperation
   {
   FileName = "placeholder.pdf",
   FileType = "pdf",
 SaveType = "Attachment"
        });
         }

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
         var result = TestProcessSingleEmail(email.Subject, email.Body, email.SenderEmail, email.AttachmentCount, verbose);
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

        public TestSubjectParams ParseSubjectLinePublic(string subject)
        {
  var p = new TestSubjectParams();
    if (string.IsNullOrEmpty(subject)) return p;

            foreach (var part in subject.Split(','))
     {
            string lp = part.Trim().ToLower();
        if (lp.Contains("grantnumber")) p.GrantNumber = ExtractValuePublic(part, "grantnumber");
          else if (lp.Contains("category")) p.Category = ExtractValuePublic(part, "category");
  else if (lp.Contains("applid")) p.ApplId = ExtractValuePublic(part, "applid");
         else if (lp.Contains("sub=")) p.SubCategory = ExtractValuePublic(part, "sub");
         else if (lp.Contains("extract")) p.Extract = ExtractValuePublic(part, "extract");
      }
            return p;
        }

   public string ExtractValuePublic(string p, string name)
   {
            if (string.IsNullOrEmpty(p)) return null;
 string[] parts = p.Split('=');
          return (parts.Length == 2 && parts[0].Trim().ToLower().Contains(name)) ? parts[1].Trim() : null;
        }

        public string GetFileTypePublic(string fileName)
   {
     if (string.IsNullOrEmpty(fileName)) return "txt";
            return fileName.Contains(".") ? fileName.Substring(fileName.LastIndexOf('.') + 1) : "txt";
    }

        public string RemoveSpecialCharsPublic(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
      return text.Replace(":", " ").Replace("/", " ").Replace("\\", " ").Replace(" ", "").Trim();
        }

        public bool IsValidEmailForProcessing(string subject)
        {
var p = ParseSubjectLinePublic(subject);
 return !string.IsNullOrEmpty(p.GrantNumber) || !string.IsNullOrEmpty(p.ApplId);
        }

        public void AddSimulatedEmail(string subject, string body = "", string senderEmail = "user@nih.gov", int attachmentCount = 0)
        {
            SimulatedEmails.Add(new SimulatedExchangeEmail
            {
 Subject = subject,
       Body = body,
        SenderEmail = senderEmail,
     AttachmentCount = attachmentCount,
            ReceivedTime = DateTime.Now.AddMinutes(-10)
        });
        }

        public void Reset()
        {
            EmailsProcessedThisSession.Clear();
            FileSaveOperations.Clear();
       ProcessedCount = 0;
     ErrorOccurred = false;
      LastErrorMessage = null;
    SimulatedEmails.Clear();
        }
    }

    public class TestSubjectParams
    {
public string GrantNumber { get; set; }
   public string Category { get; set; }
        public string ApplId { get; set; }
      public string SubCategory { get; set; }
        public string Extract { get; set; }
    }

    public class TestExchangeEmailRecord
    {
      public string Subject { get; set; }
        public string Body { get; set; }
        public string SenderEmail { get; set; }
        public string GrantNumber { get; set; }
        public string Category { get; set; }
   public string ApplId { get; set; }
        public string SubCategory { get; set; }
        public string Extract { get; set; }
   public int AttachmentCount { get; set; }
        public DateTime ProcessedTime { get; set; }
    }

    public class TestFileSaveOperation
    {
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string SaveType { get; set; }
    }

    public class SimulatedExchangeEmail
    {
        public string Subject { get; set; }
      public string Body { get; set; }
        public string SenderEmail { get; set; }
        public int AttachmentCount { get; set; }
    public DateTime ReceivedTime { get; set; }
    }
}
