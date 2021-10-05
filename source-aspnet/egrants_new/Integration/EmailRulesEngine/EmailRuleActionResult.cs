using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace egrants_new.Integration.EmailRulesEngine
{
    public class EmailRuleActionResult
    {
        public int ActionId { get; set; }
        public int RuleId { get; set; }
        public bool Successful { get; set; }
        public bool ActionStarted { get; set; }
        public bool ActionCompleted { get; set; }
        public string ActionMessage { get; set; }
        public Exception ErrorException { get; set; }
    }
}