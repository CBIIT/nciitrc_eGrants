using System;
using System.Collections.Generic;

namespace egrants_new.Integration.EmailRulesEngine.Models

{
    public class EmailRule
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public bool CriteriaAny { get; set; }
        public bool Enabled { get; set; }
        public DateTime NextRun { get; set; }
        public DateTime LastRun { get; set; }
        public int Interval { get; set; }
        public int CreatedByPersonId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int LastModifiedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public List<EmailRuleCriteria> Criteria { get; set; }
        public List<EmailRuleAction> Actions { get; set; }
    }
}