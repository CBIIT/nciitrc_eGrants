using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace egrants_new.Integration.EmailRulesEngine.Models
{
    public class ViewEmailActionResults
    {

        public int EmailRuleId { get; set; }
        public int EmailMessageId { get; set; }
        public string RuleDescription { get; set; }
        public bool ActionsCompleted { get; set; }
        public bool ActionCompleted { get; set; }
        public string ActionName { get; set; }
        public string ActionMessage { get; set; }
        public string ExceptionText { get; set; }

    }
}