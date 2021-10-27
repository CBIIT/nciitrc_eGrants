using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using egrants_new.Integration.Models;
using Hangfire.Server;

namespace egrants_new.Integration.EmailRulesEngine
{
    public class EmailActionModule
    {

        private readonly EmailIntegrationRepository _emailRepo;


        public EmailActionModule()
        {
            _emailRepo = new EmailIntegrationRepository();
        }


        public void PerformActions(EmailMessage msg, EmailRule rule)
        {
            foreach (var action in rule.Actions)
            {
                var emailAction = MakEmailAction(action, rule);

                var result = emailAction.DoAction();
                _emailRepo.SaveActionResult(result);

                if (!result.Successful)
                {
                    return;
                }
            }

            return;
        }


        public IEmailAction MakEmailAction(EmailRuleAction action, EmailRule rule)
        {
            IEmailAction outAction;

            switch (action.ActionType)
            {
                case IntegrationEnums.EmailActionType.EmailForwardAction:
                    outAction = new EmailForwardAction(rule, action);

                    break;
                case IntegrationEnums.EmailActionType.EmailCreatePdfAction:
                    outAction = new EmailCreatePdfAction(rule, action);
                    break;

                case IntegrationEnums.EmailActionType.EmailCreateSendNewEmailAction:
                    outAction = new EmailCreateSendNewEmailAction(rule, action);
                    break;

                case IntegrationEnums.EmailActionType.EmailCreateTextFileAction:
                    outAction = new EmailCreateTextFileAction(rule, action);

                    break;

                case IntegrationEnums.EmailActionType.EmailFileCopyMoveAction:
                    outAction = new EmailFileCopyMoveAction(rule, action);
                    break;

                default:
                    throw new Exception("Action Type not defined or determined");
                    break;

            }

            return outAction;

        }

    }
}