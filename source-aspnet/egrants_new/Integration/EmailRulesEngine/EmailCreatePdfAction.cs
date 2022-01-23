using egrants_new.Integration.EmailRulesEngine.Models;

namespace egrants_new.Integration.EmailRulesEngine
{
    public class EmailCreatePdfAction :BaseEmailAction
    {

        public EmailCreatePdfAction(EmailRule rule, EmailRuleAction action) : base(rule, action)
        {

        }


        public override void DelegatedAction(EmailMsg msg)
        {
            throw new System.NotImplementedException();
        }
    }
}