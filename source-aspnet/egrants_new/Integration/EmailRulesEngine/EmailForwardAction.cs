using System;
using egrants_new.Integration.EmailRulesEngine.Models;

namespace egrants_new.Integration.EmailRulesEngine
{
    public class EmailForwardAction : BaseEmailAction
    {
        public EmailRule EmailRule { get; set; }
        public EmailRuleAction Action { get; set; }
        public string ActionData { get; set ; }

        public EmailForwardAction(EmailRule rule, EmailRuleAction action):base(rule, action)
        {

        }

        public override void DelegatedAction(EmailMsg msg)
        {
            //SaveAttachmentAndFileMoveCopy
            var filePath = Action.TargetValue;
        }
    }
}