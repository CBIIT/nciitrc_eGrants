using System;
using egrants_new.Integration.Models;

namespace egrants_new.Integration.EmailRulesEngine.Models

{
    public class EmailRuleAction
    {
        public int Id { get; set; }
        public int EmailRulesId { get; set; }
        public int Order { get; set; }
        public string Description { get; set; }
        public IntegrationEnums.EmailActionType ActionType { get; set; }
        public string TargetValue { get; set; }
        public int CreatedByPersonId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int LastModifiedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public int EmailTemplateId { get; set; }
    }
}