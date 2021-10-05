using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace egrants_new.Integration.EmailRulesEngine
{
    public class EmailFileCopyMoveAction:IEmailAction
    {
        public EmailRule EmailRule { get; set; }
        public EmailRuleAction Action { get; set; }

        public EmailRuleActionResult DoAction()
        {
            var result = new EmailRuleActionResult();
           //SaveAttachmentAndFileMoveCopy
           var filePath = Action.TargetValue;


           return result;
        }
    }

    public class EmailForwardAction : IEmailAction
    {
        public EmailRule EmailRule { get; set; }
        public EmailRuleAction Action { get; set; }

        public EmailRuleActionResult DoAction()
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

        public EmailRuleActionResult DoAction()
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

        public EmailRuleActionResult DoAction()
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

        public EmailRuleActionResult DoAction()
        {
            var result = new EmailRuleActionResult();
            //SaveAttachmentAndFileMoveCopy
            var filePath = Action.TargetValue;


            return result;
        }
    }

}