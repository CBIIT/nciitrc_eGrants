using System;
using egrants_new.Integration.EmailRulesEngine.Models;
using Newtonsoft.Json.Linq;

namespace egrants_new.Integration.EmailRulesEngine
{
    public class EmailLookUpAction:IEmailAction
    {

            public EmailRule EmailRule { get; set; }
            public EmailRuleAction Action { get; set; }
            public string ActionData { get; set; }

            public EmailLookUpAction(EmailRule rule, EmailRuleAction action)
            {
                EmailRule = rule;
                Action = action;
            }

            public EmailRuleActionResult DoAction(EmailMsg msg)
            {
                var result = new EmailRuleActionResult();
                var tVal = this.Action.TargetValue;
                dynamic Instruction = JObject.Parse(tVal);


			/*
             *   		
             *
             */






			return result;
            }

        public void DelegatedAction(EmailMsg msg)
        {
            throw new NotImplementedException();
        }
    }
    }


