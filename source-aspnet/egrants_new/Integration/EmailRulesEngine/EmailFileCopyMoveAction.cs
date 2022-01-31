using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Threading.Tasks;
using System.Configuration;
using System.EnterpriseServices.Internal;
using egrants_new.Integration.EmailRulesEngine.Models;
using Newtonsoft.Json.Linq;


namespace egrants_new.Integration.EmailRulesEngine
{
    public class EmailFileCopyMoveAction : BaseEmailAction
    {

        public EmailFileCopyMoveAction(EmailRule rule, EmailRuleAction action) : base(rule, action)
        {

        }

        public override void DelegatedAction(EmailMsg msg)
        {
            EmailMsg message = msg;

            string tmpActionMsg = "Action Initialized";

            //SaveAttachmentAndFileMoveCopy
            var destinationPath = Action.TargetValue;

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

    }
}