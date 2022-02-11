using System;
using System.Collections.Generic;
using egrants_new.Integration.Shared;
using Newtonsoft.Json.Linq;

namespace egrants_new.Integration.EmailRulesEngine.Models
{
    public class EmailMsg
    {
        public int Id { get; set; }
        public int EmailMonitoredMailboxId { get; set; }
        public string GraphId { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime LastModifiedDateTime { get; set; }
        public DateTime ReceivedDateTime { get; set; }
        public DateTime SentDateTime { get; set; }
        public bool HasAttachments { get; set; }
        public string Subject { get; set; }
        public string BodyPreview { get; set; }
        public string Importance { get; set; }
        public string ParentFolderId { get; set; }
        public bool IsRead { get; set; }
        public string Body { get; set; }
        public string Sender { get; set; }
        public string EmailFrom { get; set; }
        public string ToRecipients { get; set; }
        public string CcRecipients { get; set; }
        public Dictionary<string, EmailMsgMetadata> EgrantsMetaData { get; set; }

        //private string _msgBody { get; set; }
        public string MessageBody
        {
            get
            {
                string output = string.Empty;
                if (Body != null)
                {
                    JObject body = JObject.Parse(Body);
                    string htmlBody = (string) body["content"];
                    output = TextHelper.BustHtml(htmlBody);
                }
                return output;
            }
        }


        public EmailMsg()
        {
            EgrantsMetaData = new Dictionary<string, EmailMsgMetadata>();

        }

    }
}