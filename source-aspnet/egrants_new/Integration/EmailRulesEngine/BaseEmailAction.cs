using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Http.ModelBinding;
using System.Windows.Forms;
using static egrants_new.Integration.Models.IntegrationEnums;
using egrants_new.Integration.EmailRulesEngine.Models;
using Hangfire.Annotations;
using Newtonsoft.Json.Linq;

namespace egrants_new.Integration.EmailRulesEngine
{
    public abstract class BaseEmailAction : IEmailAction
    {
        public EmailRule EmailRule { get; set; }
        public EmailRuleAction Action { get; set; }
        public string ActionData { get; set; }
        public delegate void RunAction(EmailMsg msg);
        public readonly EmailIntegrationRepository EmailRepo;

        protected BaseEmailAction(EmailRule rule, EmailRuleAction action)
        {
            EmailRepo = new EmailIntegrationRepository();
            EmailRule = rule;
            Action = action;
        }


        public EmailRuleActionResult DoAction(EmailMsg msg)
        {
            var result = new EmailRuleActionResult
            {
                ActionStarted = true,
                ActionId = Action.Id,
                MessageId = msg.Id,
                RuleId = EmailRule.Id,
                Successful = false,
                CreatedDate = DateTimeOffset.Now
            };

            RunAction run = DelegatedAction;

            //If this is the first Action then extract the message details

            try
            {
                if (Action.Order == 1)
                {
                   // ExtractMessageDetails(msg);
                }
                run(msg);
                result.ActionCompleted = true;
                result.Successful = true;
            }
            catch (Exception ex)
            {
                result.ActionCompleted = false;
                result.ErrorException = ex;
                result.ExceptionText = ex.Message;
                result.Successful = false;
            }

            return result;
        }

        public abstract void DelegatedAction(EmailMsg msg);


        public void ExtractMessageDetails(EmailMsg msg)
        {


            //caeranotifications

            const int profileid = 1;
            msg.EgrantsMetaData.Add("profileid",
                new EmailMsgMetadata() { metadata = profileid, Name = "profileid", type = MetadataType.Integer });

            var pa = GetPa(RemoveSpecialCharacters(msg.Subject));
            msg.EgrantsMetaData.Add("pa",
                new EmailMsgMetadata() { metadata = pa, Name = "pa", type = MetadataType.String });

            var fgn = ExtractValue(msg.Subject, "grantnumber=");
            msg.EgrantsMetaData.Add("fgn",
                new EmailMsgMetadata() { metadata = fgn, Name = "fgn", type = MetadataType.String });

            int applid = int.Parse(ExtractValue(msg.Subject, "applid=", true));
            msg.EgrantsMetaData.Add("applid",
                new EmailMsgMetadata() { metadata = applid, Name = "applid", type = MetadataType.Integer });

            var tempapplid = EmailRepo.GetTempApplId((ExtractValue(msg.Body, "Notification Id=")));
            msg.EgrantsMetaData.Add("tempapplid",
                new EmailMsgMetadata() { metadata = tempapplid, Name = "tempapplid", type = MetadataType.String });

            var v_SenderID = msg.Sender;
            msg.EgrantsMetaData.Add("v_SenderID",
                new EmailMsgMetadata() { metadata = v_SenderID, Name = "v_SenderID", type = MetadataType.String });

            var v_SubLine = msg.Subject;
            msg.EgrantsMetaData.Add("v_SubLine",
                new EmailMsgMetadata() { metadata = v_SubLine, Name = "v_SubLine", type = MetadataType.String });

            var notification_filetype = "txt";
            msg.EgrantsMetaData.Add("pa",
                new EmailMsgMetadata() { metadata = notification_filetype, Name = "notification_filetype", type = MetadataType.String });

            var catname = "eRA Notification"; //TODO: Get Data for this
            msg.EgrantsMetaData.Add("catname",
                new EmailMsgMetadata() { metadata = catname, Name = "catname", type = MetadataType.String });

            var subcatname = "Supplement Requested"; //TODO: Get Data for this
            msg.EgrantsMetaData.Add("pa",
                new EmailMsgMetadata() { metadata = subcatname, Name = "subcatname", type = MetadataType.String });

            var emd = new ExtractedMessageDetails()
            {
                Parentapplid = applid,
                Pa = pa,
                Rcvd_dt = msg.ReceivedDateTime,
                Catname = catname,
                Filetype = notification_filetype,
                Subcatname = subcatname,
                Sub = msg.Subject,
                Body = msg.Body
            };

            var filenumbername = EmailRepo.GetPlaceHolder(emd);
            msg.EgrantsMetaData.Add("pa",
                new EmailMsgMetadata() { metadata = filenumbername, Name = "filenumbername", type = MetadataType.String });

            var Alias = string.Concat(filenumbername, ".txt");
            msg.EgrantsMetaData.Add("pa",
                new EmailMsgMetadata() { metadata = Alias, Name = "Alias", type = MetadataType.String });
        }


        public string GetTempApplId(string notificationId)
        {
            string output = "";

            output = EmailRepo.GetTempApplId(notificationId);

            return output;
        }


        public string GetSenderId(EmailMsg msg)
        {

            if (msg.Sender.StartsWith("EX"))
            {

            }

            return "";
        }


        public string ExtractValue(string text, string search, bool digitsOnly = false)
        {
            string extracted = string.Empty;
            string captureGroupType = digitsOnly ? "d*" : "w*";
            string expression = $"{search}(?<result>\\{captureGroupType})";
            //Regex regex = new Regex(expression);

            var matches = Regex.Matches(text, expression, RegexOptions.IgnoreCase);

            if (matches.Count == 1)
            {
                extracted = matches[0].Groups["result"].Value;
            }
            else
            {
                throw new Exception("There were multiple matches for the category");
            }

            return extracted;
        }


        public string RemoveSpecialCharacters(string input)
        {
            string result = "";

            result = result.Replace("\n", "\r\n");
            result = result.Replace("&", "and");

            List<string> specCharsList = ":,/,\\,&,;,<,>,<<,^,%,@,'".Split(',').ToList();

            foreach (var spec in specCharsList)
            {
                if (result.Contains(spec))
                {
                    result = result.Replace(spec, " ");
                }
            }

            result = result.Replace(" ", "");
            result = result.Trim();

            return result;
        }

        public string GetPa(string input)
        {
            string result = "";
            result = EmailRepo.GetPa(RemoveSpecialCharacters(input));
            return result;
        }

        public string ExtractBody(EmailMsg msg)
        {
            var bodyObj = JObject.Parse(msg.Body);
            string body = (string)bodyObj["content"];
            return body;
        }


    }
}