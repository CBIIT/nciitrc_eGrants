using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Runtime.Remoting.Messaging;
using System.Web;

namespace egrants_new.Integration.EmailRulesEngine
{
    public class EmailFileCopyMoveAction:IEmailAction
    {
        public EmailRule EmailRule { get; set; }
        public EmailRuleAction Action { get; set; }
        public EmailMessage Message { get; set; }

        public EmailFileCopyMoveAction(EmailRule rule, EmailRuleAction action)
        {
            EmailRule = rule;
            Action = action;
        }

        public EmailRuleActionResult DoAction(EmailMessage msg)
        {
            Message = msg;
            var result = new EmailRuleActionResult();
           //SaveAttachmentAndFileMoveCopy
           var filePath = Action.TargetValue;





           return result;
        }



        private string GetPlaceHolderFileName()
        {
            var repo = new EmailIntegrationRepository();

            var msgDetails = EmailActionModule.ExtractMessageDetails(Message, EmailRule);


            string filename = repo.GetPlaceHolder();



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
            var array = body.Split('Notification Id=');




            return output;
        }


    }

    public class EmailForwardAction : IEmailAction
    {
        public EmailRule EmailRule { get; set; }
        public EmailRuleAction Action { get; set; }

        public EmailForwardAction(EmailRule rule, EmailRuleAction action)
        {
            EmailRule = rule;
            Action = action;
        }

        public EmailRuleActionResult DoAction(EmailMessage msg)
        {
            var result = new EmailRuleActionResult();
            //SaveAttachmentAndFileMoveCopy
            var filePath = Action.TargetValue;


            return result;
        }
    }

    public class EmailCreateTextFileAction : IEmailAction
    {
        public EmailRule EmailRule { get; set; }
        public EmailRuleAction Action { get; set; }

        public EmailCreateTextFileAction(EmailRule rule, EmailRuleAction action)
        {
            EmailRule = rule;
            Action = action;
        }


        public EmailRuleActionResult DoAction(EmailMessage msg)
        {
            var result = new EmailRuleActionResult();
            //SaveAttachmentAndFileMoveCopy
            var filePath = Action.TargetValue;


            return result;
        }
    }

    public class EmailCreatePdfAction : IEmailAction
    {
        public EmailRule EmailRule { get; set; }
        public EmailRuleAction Action { get; set; }

        public EmailCreatePdfAction(EmailRule rule, EmailRuleAction action)
        {
            EmailRule = rule;
            Action = action;
        }


        public EmailRuleActionResult DoAction(EmailMessage msg)
        {
            var result = new EmailRuleActionResult();
            //SaveAttachmentAndFileMoveCopy
            var filePath = Action.TargetValue;


            return result;
        }
    }
    
    public class EmailCreateSendNewEmailAction : IEmailAction
    {
        public EmailRule EmailRule { get; set; }
        public EmailRuleAction Action { get; set; }

        public EmailCreateSendNewEmailAction(EmailRule rule, EmailRuleAction action)
        {
            EmailRule = rule;
            Action = action;
        }


        public EmailRuleActionResult DoAction(EmailMessage msg)
        {
            var result = new EmailRuleActionResult();
            //SaveAttachmentAndFileMoveCopy
            var filePath = Action.TargetValue;


            return result;
        }
    }

}