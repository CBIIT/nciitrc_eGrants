using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Web;
using System.Reflection;
using System.Web.Services.Description;
using egrants_new.Integration.EmailRulesEngine;
using egrants_new.Integration.Models;

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

            //List<EmailRule> output = new List<EmailRule>();
            return _repo.GetEmailRules();
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

            switch (criteria.EvalType)
            {
                case IntegrationEnums.EvalType.Contains:
                   result = fieldVal.Contains(criteria.EvalValue);

                    break;
                case IntegrationEnums.EvalType.EndsWith:
                    result = fieldVal.EndsWith(criteria.EvalValue, StringComparison.CurrentCultureIgnoreCase);

                    break;
                case IntegrationEnums.EvalType.Equals:
                    result = fieldVal.Equals(criteria.EvalValue, StringComparison.CurrentCultureIgnoreCase);

                    break;
                case IntegrationEnums.EvalType.GreaterThan:
                    result = int.Parse(fieldVal) > int.Parse(criteria.EvalValue);

                    break;
                case IntegrationEnums.EvalType.LessThan:
                    result = int.Parse(fieldVal) < int.Parse(criteria.EvalValue);

                    break;
                case IntegrationEnums.EvalType.NotEqual:
                    result = int.Parse(fieldVal) != int.Parse(criteria.EvalValue);

                    break;
                case IntegrationEnums.EvalType.StartsWith:
                    result = fieldVal.StartsWith(criteria.EvalValue, StringComparison.CurrentCultureIgnoreCase);

                    break;

                default:
                    break;
            }


            return result;
        }

        public void ExecuteActions()
        {



        }


    }
}