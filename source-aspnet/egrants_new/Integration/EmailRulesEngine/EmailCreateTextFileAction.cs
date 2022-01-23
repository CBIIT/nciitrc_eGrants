using egrants_new.Integration.EmailRulesEngine.Models;

namespace egrants_new.Integration.EmailRulesEngine
{
    public class EmailCreateTextFileAction : BaseEmailAction
    {

        public EmailCreateTextFileAction(EmailRule rule, EmailRuleAction action):base(rule, action)
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

        public override void DelegatedAction(EmailMsg msg)
        {
            throw new System.NotImplementedException();
        }
    }
}