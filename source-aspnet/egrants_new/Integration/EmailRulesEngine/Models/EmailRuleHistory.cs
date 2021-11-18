using System;

namespace egrants_new.Integration.EmailRulesEngine.Models

{
    public class EmailRuleHistory
    {
        public int Id { get; set; }
        public int EmailRuleId { get; set; }
        public DateTime RunDate { get; set; }
        public int LastModifiedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }
}