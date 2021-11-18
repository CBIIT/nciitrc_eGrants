using System;


namespace egrants_new.Integration.EmailRulesEngine.Models

{
    public class EmailRuleActionResult
    {
        public int MessageId { get; set; }
        public int ActionId { get; set; }
        public int RuleId { get; set; }
        public bool Successful { get; set; }
        public bool ActionStarted { get; set; }
        public bool ActionCompleted { get; set; }
        public string ActionMessage { get; set; }
        public Exception ErrorException { get; set; }
        public string ExceptionText { get; set; }
        public string ActionDataPassed { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}