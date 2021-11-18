using System;

namespace egrants_new.Integration.EmailRulesEngine.Models

{
    public class EmailRuleMatchedMessages
    {
        public int EmailRuleId { get; set; }
        public int EmailMessageId { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool ActionsCompleted { get; set; }
        public bool Matched { get; set; }
        public EmailRule Rule { get; set; }
        public EmailMsg Message { get; set; }
    }
}