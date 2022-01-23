
using System;
using egrants_new.Egrants_Admin.Models;
using egrants_new.Integration.EmailRulesEngine.Models;

namespace egrants_new.Integration.EmailRulesEngine
{
   public  interface IEmailAction
    {
        string ActionData { get; set; }
        EmailRule EmailRule { get; set; }
        EmailRuleActionResult DoAction(EmailMsg msg = null);
        void DelegatedAction(EmailMsg msg);

    }
}
