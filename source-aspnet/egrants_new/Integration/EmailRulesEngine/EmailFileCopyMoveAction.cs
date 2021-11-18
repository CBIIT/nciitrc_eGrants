using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Threading.Tasks;
using System.Configuration;
using egrants_new.Integration.EmailRulesEngine.Models;


namespace egrants_new.Integration.EmailRulesEngine
{
    public class EmailFileCopyMoveAction:IEmailAction
    {
        public EmailRule EmailRule { get; set; }
        public EmailRuleAction Action { get; set; }
        public EmailMsg Message { get; set; }
        public string ActionData { get; set; }

        public EmailFileCopyMoveAction(EmailRule rule, EmailRuleAction action)
        {
            EmailRule = rule;
            Action = action;
        }

        public EmailRuleActionResult DoAction(EmailMsg msg)
        {
            Message = msg;
            string tmpActionMsg = "Action Initialized";
            var result = new EmailRuleActionResult()
            {
                ActionId = Action.Id,
                RuleId = EmailRule.Id,
                Successful = false
            };
           //SaveAttachmentAndFileMoveCopy
           var destinationPath = Action.TargetValue;

           try
           {
               //Create the file
               tmpActionMsg = "Creating TXT file layout";
               string txtFileContents = $@"From:	{msg.EmailFrom}
Sent:	{msg.SentDateTime}
To:	{msg.ToRecipients}
Subject:	{msg.Subject} 



{msg.Body}";
               tmpActionMsg = "Getting eGrants Document Placeholder Filename";
                //string fileName = GetPlaceHolderFileName();
               string fileName = string.Join(".", Guid.NewGuid().ToString(), "txt");
               string localPath = ConfigurationManager.AppSettings["EmailAttachmentTempFolder"];
               string localFile = Path.Combine(localPath, fileName);
               string destinationFile = Path.Combine(destinationPath, fileName);

               tmpActionMsg = "Writing File to Disk";
               File.WriteAllText(localFile, txtFileContents);

               //Move File to Remote Dir
               tmpActionMsg = "Copying File to Remote directory";
               File.Copy(localFile, destinationFile);

               tmpActionMsg = "Action Completed";

           }
            catch (Exception ex)
            {
                result.ErrorException = ex;

            }

            result.ActionMessage = tmpActionMsg;
            return result;
        }



        private string GetPlaceHolderFileName()
        {
            var repo = new EmailIntegrationRepository();

            var msgDetails = EmailActionModule.ExtractMessageDetails(Message, EmailRule);

            string filename = repo.GetPlaceHolder(msgDetails);
            filename = string.Join(".", filename, msgDetails.Filetype);

            return filename;
        }

        private string GetSubcat()
        {
            string subcatname = "Unknown";

            if(Message.Subject.ToLower().Contains("Change in Status".ToLower()))
            {
                subcatname = "Supplement Status Change";
            }
            else if(Message.Subject.ToLower().Contains("Admin Supplement ".ToLower()))
            {
                subcatname = "Admin Supplement";
            }
            else if (Message.Subject.ToLower().Contains("Response Required".ToLower()))
            {
                subcatname = "Supplement Response Required";
            }
            else if (Message.Subject.ToLower().Contains("Diversity Supplement ".ToLower()))
            {
                subcatname = "Diversity Supplement";
            }

            return subcatname;
        }


        private string ExtractNotificationIDElement(string body)
        {
            string output = string.Empty;
            //var array = body.Split('Notification Id=');




            return output;
        }


    }
}