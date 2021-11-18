using System;
using egrants_new.Integration.Models;

namespace egrants_new.Integration.EmailRulesEngine.Models

{
    public class EmailRuleCriteria
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public int EmailRulesId { get; set; }
        public IntegrationEnums.CriteriaType CriteriaType { get; set; }
        public string FieldToEval { get; set; }
        public IntegrationEnums.EvalType EvalType { get; set; }
        public string EvalValue { get; set; }
        public int CreatedByPersonId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int LastModifiedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }
}