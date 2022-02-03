using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using egrants_new.Integration.EmailRulesEngine.Models;

namespace egrants_new.Egrants.Models
{
    public class MailIntegrationPage
    {

        public string Result { get; set; }

        public List<EmailMsg> Messages { get; set; }
        public List<EmailRuleActionResult> Results { get; set; }

        public int PageType;


    }
}