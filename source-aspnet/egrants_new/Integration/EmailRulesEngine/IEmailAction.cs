using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using egrants_new.Integration.EmailRulesEngine;

namespace egrants_new.Integration.EmailRulesEngine
{
   public  interface IEmailAction
    {

        EmailRule EmailRule { get; set; }
        EmailRuleActionResult DoAction(EmailMessage msg = null);

    }
}
