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

        public void ProcessMail()
        {
            var rules = LoadRules();

            foreach (var rule in rules)
            {
                var messages = LoadMessages(rule);
                foreach (var msg in messages)
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
                    _repo.SaveRuleMatch(msg, rule, trueFlag);
                }
            }
        }


        public void ProcessPendingActions()
        {
            var rules = LoadRules();

            foreach (var rule in rules)
            {
                ExecuteActions(rule);
            }
        }



        private List<EmailRule> LoadRules()
        {
            return _repo.GetEmailRules();
        }


        private List<EmailMessage> LoadMessages(EmailRule rule)
        {
            return _repo.GetEmailMessages(rule);
        }


        private bool EvaluateCriteria(EmailMessage msg, EmailRuleCriteria criteria)
        {
            PropertyInfo[] properties = msg.GetType().GetProperties();
            PropertyInfo fieldToEval = properties.FirstOrDefault(p => p.Name.ToLower() == criteria.FieldToEval.ToLower());
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

        private void ExecuteActions(EmailRule rule)
        {

            var matches = _repo.GetEmailRuleMatches(rule.Id);

            foreach (var match in matches)
            {
                var msg = _repo.GetEmailMessage(match.EmailMessageId);
                bool completedActions = _actionModule.PerformActions(msg,rule);

            }

        }


    }
}