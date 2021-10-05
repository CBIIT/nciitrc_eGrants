using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
            foreach (var result in rule.Actions.Select(action => action.DoAction()))
            {
                _emailRepo.SaveActionResult(result);

                if (!result.Successful)
                {
                    return;
                }
            }

            return;
        }


    }
}