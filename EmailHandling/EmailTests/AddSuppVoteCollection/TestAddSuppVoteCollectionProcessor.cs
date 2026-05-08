using System;
using System.Collections.Generic;
using AddSuppVoteCollection;
using CommonUtilties;

namespace EmailTests.AddSuppVoteCollection
{
    internal class TestAddSuppVoteCollectionProcessor : Processor
    {
        public List<TestVoteRecord> VotesProcessedThisSession { get; } = new List<TestVoteRecord>();
        public List<TestForwardedEmail> ForwardedEmailsThisSession { get; } = new List<TestForwardedEmail>();
        public int ProcessedCount { get; private set; }
        public bool ErrorOccurred { get; private set; }
        public string LastErrorMessage { get; private set; }
        public List<SimulatedVoteEmail> SimulatedVoteEmails { get; set; } = new List<SimulatedVoteEmail>();

        public TestVoteRecord TestProcessSingleVote(string subject, string senderName,
       string senderEmail = "voter@nih.gov", string body = "", string verbose = "n")
        {
            try
            {
                bool isAccepted = subject.Contains("Accepted:");
                bool isRejected = subject.Contains("Rejected:");

                if (!isAccepted && !isRejected) return null;

                ProcessedCount++;

                var voteRecord = new TestVoteRecord
       {
          Subject = subject,
          SenderName = senderName,
          SenderEmail = senderEmail,
          Body = body,
        VoteType = isAccepted ? "Accepted" : "Rejected",
        ProcessedTime = DateTime.Now,
     WasMovedToVoteFolder = true
 };

VotesProcessedThisSession.Add(voteRecord);

       var forwardedEmail = new TestForwardedEmail
           {
          OriginalSubject = subject,
         ForwardedSubject = "DO NOT REPLY : Forwarding Response [" + subject + "]",
      Recipients = new List<string> { "emily.driskell@nih.gov", "jonesni@mail.nih.gov" },
             ForwardedTime = DateTime.Now
    };

           ForwardedEmailsThisSession.Add(forwardedEmail);
   return voteRecord;
            }
   catch (Exception ex)
            {
     ErrorOccurred = true;
         LastErrorMessage = ex.Message;
  return null;
        }
     }

        public int TestProcessSimulatedVotes(string verbose = "n")
        {
       try
      {
                int votesProcessed = 0;
      foreach (var email in SimulatedVoteEmails)
          {
             var result = TestProcessSingleVote(email.Subject, email.SenderName, email.SenderEmail, email.Body, verbose);
           if (result != null) votesProcessed++;
       }
      return votesProcessed;
    }
        catch (Exception ex)
            {
  ErrorOccurred = true;
    LastErrorMessage = ex.Message;
        return ProcessedCount;
    }
   }

        public bool IsVoteEmail(string subject)
        {
            return subject.Contains("Accepted:") || subject.Contains("Rejected:");
     }

public string GetVoteType(string subject)
        {
   if (subject.Contains("Accepted:")) return "Accepted";
     if (subject.Contains("Rejected:")) return "Rejected";
   return null;
        }

public void AddSimulatedVoteEmail(string subject, string senderName, string senderEmail = "voter@nih.gov", string body = "")
     {
     SimulatedVoteEmails.Add(new SimulatedVoteEmail
            {
      Subject = subject,
    SenderName = senderName,
          SenderEmail = senderEmail,
              Body = body,
              ReceivedTime = DateTime.Now.AddMinutes(-10)
     });
        }

        public void Reset()
        {
            VotesProcessedThisSession.Clear();
            ForwardedEmailsThisSession.Clear();
            ProcessedCount = 0;
     ErrorOccurred = false;
      LastErrorMessage = null;
     SimulatedVoteEmails.Clear();
        }
    }

    public class TestVoteRecord
    {
        public string Subject { get; set; }
        public string SenderName { get; set; }
public string SenderEmail { get; set; }
public string Body { get; set; }
        public string VoteType { get; set; }
        public DateTime ProcessedTime { get; set; }
    public bool WasMovedToVoteFolder { get; set; }
    }

    public class TestForwardedEmail
    {
 public string OriginalSubject { get; set; }
        public string ForwardedSubject { get; set; }
        public List<string> Recipients { get; set; }
        public DateTime ForwardedTime { get; set; }
 }

    public class SimulatedVoteEmail
    {
        public string Subject { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
    public string Body { get; set; }
public DateTime ReceivedTime { get; set; }
    }
}
