using System;

namespace egrants_new.Integration.EmailRulesEngine.Models

{
    public class EmailMonitoredMailbox
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string MailboxName { get; set; }
        public string MailboxUseName { get; set; }
        public string OAuthToken { get; set; }
        public bool Enabled { get; set; }
        public int CreatedByPersonId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int LastModifiedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }
}