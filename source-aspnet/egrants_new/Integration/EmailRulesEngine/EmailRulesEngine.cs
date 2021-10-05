using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Web;
using System.Reflection;
using System.Web.Services.Description;
using egrants_new.Integration.EmailRulesEngine;

namespace egrants_new.Integration.EmailRulesEngine
{
    public class EmailRulesEngine
    {
        private readonly EmailActionModule _actionModule;
        private readonly EmailIntegrationRepository _repo;

        public EmailRulesEngine()
        {

            _actionModule = new EmailActionModule();
            _repo = new EmailIntegrationRepository();

        }

        public void ProcessRules()
        {
            var rules = LoadRules();
            var messages = LoadMessages();

            foreach (var msg  in messages)
            {
                foreach (var rule in rules)
                {
                    bool trueFlag = false;
                    foreach (var criteria in rule.Criteria)
                    {
                        trueFlag = EvaluateCriteria(msg, criteria);

                        if (rule.CriteriaAny && trueFlag || !rule.CriteriaAny && !trueFlag)
                        {
                            break;
                        }
                    }
                    //Write the match to the match table
                    if (trueFlag)
                    {
                       _repo.SaveRuleMatch(msg,rule);
                    }
                }
            }
        }



        public List<EmailRule> LoadRules()
        {




            List<EmailRule> output = new List<EmailRule>();
            return output;
        }


        public List<EmailMessage> LoadMessages()
        {
            List<EmailMessage> output = new List<EmailMessage>(); 


            return output;
        }



        public bool EvaluateCriteria(EmailMessage msg, EmailRuleCriteria criteria)
        {
            PropertyInfo[] properties = msg.GetType().GetProperties();
            PropertyInfo fieldToEval = properties[criteria.FieldToEval];
            string fieldVal = (string)fieldToEval.GetValue(msg);

            bool result = false;

            return result;
        }

        public bool TestOperation(  )
        {
            bool test = false;


            return test;
        }


        public void ExecuteActions()
        {



        }


    }
}