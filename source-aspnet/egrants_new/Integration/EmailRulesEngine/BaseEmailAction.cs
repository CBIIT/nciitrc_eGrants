using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static egrants_new.Integration.Models.IntegrationEnums;
using egrants_new.Integration.EmailRulesEngine.Models;
using Newtonsoft.Json.Linq;
using egrants_new.Integration.Models;
using System.Configuration;
using System.IO;

namespace egrants_new.Integration.EmailRulesEngine
{
    public abstract class BaseEmailAction : IEmailAction
    {
        public EmailRule EmailRule { get; set; }
        public EmailRuleAction Action { get; set; }
        public string ActionData { get; set; }
        public string ActionMessage { get; set; }
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
                  //  ExtractMessageDetails(msg);
                }
                run(msg);
                result.ActionMessage = ActionMessage;
                result.ActionCompleted = true;
                result.Successful = true;
            }
            catch (Exception ex)
            {
                result.ActionMessage = ActionMessage;
                result.ActionCompleted = false;
                result.ErrorException = ex;
                result.ExceptionText = ex.Message;
                result.Successful = false;
            }

            return result;
        }

        public abstract void DelegatedAction(EmailMsg msg);


        public virtual void  ExtractMessageDetails(EmailMsg msg)
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

            var tmpApplid = ExtractValue(msg.Subject, "applid=", true);

            int applid = 0;
            if (!Int32.TryParse(tmpApplid, out applid))
            {
                msg.EgrantsMetaData.Add("applid",
                    new EmailMsgMetadata() { metadata = 0, Name = "applid", type = MetadataType.Integer });
            }
            else
            {
                msg.EgrantsMetaData.Add("applid",
                    new EmailMsgMetadata() { metadata = applid.ToString(), Name = "applid", type = MetadataType.Integer });
            }

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
            msg.EgrantsMetaData.Add("notification_filetype",
                new EmailMsgMetadata() { metadata = notification_filetype, Name = "notification_filetype", type = MetadataType.String });

            var catname = "eRA Notification"; //TODO: Get Data for this
            msg.EgrantsMetaData.Add("catname",
                new EmailMsgMetadata() { metadata = catname, Name = "catname", type = MetadataType.String });

            var subcatname = "subcatname"; //TODO: Get Data for this
            msg.EgrantsMetaData.Add("subcatname",
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
            msg.EgrantsMetaData.Add("filenumbername",
                new EmailMsgMetadata() { metadata = filenumbername, Name = "filenumbername", type = MetadataType.String });

            var Alias = string.Concat(filenumbername, ".txt");
            msg.EgrantsMetaData.Add("Alias",
                new EmailMsgMetadata() { metadata = Alias, Name = "Alias", type = MetadataType.String });
        }


        public int GetTempApplId(string notificationId)
        {
            int output = 0;

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
            string captureGroupType = digitsOnly ? "\\d*\\s" : ".+\\b";

            string expression = $"{search}(?<result>{captureGroupType})";
            //Regex regex = new Regex(expression);

            var matches = Regex.Matches(text, expression, RegexOptions.IgnoreCase);

            if (matches.Count == 1)
            {
                extracted = matches[0].Groups["result"].Value;
            }
            else
            {
                //throw new Exception("There were multiple matches for the category");
            }

            return extracted;
        }


        public string RemoveSpecialCharacters(string input)
        {
            string result = input;

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

        public string GetPlaceholder(ExtractedMessageDetails emd)
        {
            return EmailRepo.GetPlaceHolder(emd);
        }

        public int GetApplId(string searchtext)
        {
            return EmailRepo.GetApplId(RemoveSpecialCharacters(searchtext));

        }

        public bool CheckIsReply(string notificationId, string senderId)
        {
            return EmailRepo.ChecklIsReply(notificationId, senderId);
        }

        public void SaveMessage(EmailMsg msg, string filename, string destinationPath, Integration.Models.IntegrationEnums.MessageSaveType saveType)
        {
            string tmpActionMsg = "Action Initialized";
            switch (saveType)
            {
                case MessageSaveType.Text:
                    //SaveAttachmentAndFileMoveCopy
                    //var destinationPath = Action.TargetValue;

                    //Create the file
                    tmpActionMsg = "Creating TXT file layout";
                    string txtFileContents = $@"From:       {msg.EmailFrom}
Sent:       {msg.SentDateTime}
To:         {msg.ToRecipients}
Subject:	{msg.Subject} 



{msg.MessageBody}";

                    //string fileName = GetPlaceHolderFileName();
                    //string fileName = string.Join(".", Guid.NewGuid().ToString(), "txt");
                    string localPath = ConfigurationManager.AppSettings["EmailAttachmentTempFolder"];
                    string localFile = Path.Combine(localPath, filename);
                    string destinationFile = Path.Combine(destinationPath, filename);

                    tmpActionMsg = "Writing File to Disk";
                    File.WriteAllText(localFile, txtFileContents);

                    //Move File to Remote Dir
                    tmpActionMsg = "Copying File to Remote directory";
                    File.Copy(localFile, destinationFile);
                    tmpActionMsg = "Action Completed";

                    break;  

                case MessageSaveType.Html:
                    throw new NotImplementedException("Saving Message Type HTML not Implemented Yet");

                    break;
                case MessageSaveType.Pdf:
                    throw new NotImplementedException("Saving Message Type PDF Not Implemented");
                    break;
                default:
                    break;

            }



        }
    }
}