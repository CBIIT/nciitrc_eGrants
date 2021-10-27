using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using egrants_new.Integration.EmailRulesEngine;
using egrants_new.Integration.Models;
using static egrants_new.Integration.Models.IntegrationEnums;

namespace egrants_new.Integration.EmailRulesEngine
{

    public class EmailMonitoredMailbox
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string MailboxName { get; set; }
        public string MailboxUseName { get; set; }
        public string OAuthToken { get; set; }
        public bool Enabled { get; set; }
        public int CreatedByPersonId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int LastModifiedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }

    public class EmailMessage
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
        public string From { get; set; }
        public string ToRecipients { get; set; }
        public string CcRecipients { get; set; }
    }

    public class EmailRule
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public bool CriteriaAny { get; set; }
        public bool Enabled { get; set; }
        public DateTime NextRun { get; set; }
        public DateTime LastRun { get; set; }
        public int Interval { get; set; }
        public int CreatedByPersonId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int LastModifiedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public List<EmailRuleCriteria> Criteria { get; set; }
        public List<EmailRuleAction> Actions { get; set; }
    }


    public class EmailRuleHistory
    {
        public int Id { get; set; }
        public int EmailRuleId { get; set; }
        public DateTime RunDate { get; set; }
        public int LastModifiedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }

    public class EmailRuleCriteria
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public int EmailRulesId { get; set; }
        public CriteriaType CriteriaType { get; set; }
        public int FieldToEval { get; set; }
        public EvalType EvalType { get; set; }
        public string EvalValue { get; set; }
        public int CreatedByPersonId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int LastModifiedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }

    public class EmailFields
    {
        public int Id { get; set; }
        public string FieldName { get; set; }
        public int DataTypeId { get; set; }
    }

    public class EmailFieldDataTypes
    {
        public int Id { get; set; }
        public string Type { get; set; }
    }


    public class EmailRuleAction
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public string Description { get; set; }
        public IntegrationEnums.EmailActionType ActionType { get; set; }
        public string TargetValue { get; set; }
        public int CreatedByPersonId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int LastModifiedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public int EmailTemplateId { get; set; }
    }

    public class EmailRuleMatchedMessages
    {
        public int EmailRuleId { get; set; }
        public int EmailMessageId { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool ActionsCompleted { get; set; }
        public bool Matched { get; set; }
        public EmailRule Rule { get; set; }
        public EmailMessage Message { get; set; }
    }

}