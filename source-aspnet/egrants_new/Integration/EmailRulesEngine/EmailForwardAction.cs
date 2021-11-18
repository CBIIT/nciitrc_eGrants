using System;
using egrants_new.Integration.EmailRulesEngine.Models;

namespace egrants_new.Integration.EmailRulesEngine
{
    public class EmailForwardAction : IEmailAction
    {
        public EmailRule EmailRule { get; set; }
        public EmailRuleAction Action { get; set; }
        public string ActionData { get; set ; }

        public EmailForwardAction(EmailRule rule, EmailRuleAction action)
        {
            EmailRule = rule;
            Action = action;
        }

        public EmailRuleActionResult DoAction(EmailMsg msg)
        {
            var result = new EmailRuleActionResult();
            //SaveAttachmentAndFileMoveCopy
            var filePath = Action.TargetValue;


            return result;
        }
    }
}