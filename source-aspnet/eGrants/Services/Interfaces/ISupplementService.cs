using eGrants.Models;

namespace eGrants.Services.Interfaces
{
    /// <summary>
    /// Interface for Supplement service operations
    /// </summary>
    public interface ISupplementService
    {
        /// <summary>
        /// Load notifications based on search criteria
        /// </summary>
        /// <param name="act">The action type</param>
        /// <param name="pa">The PA code</param>
        /// <param name="detail">Additional details</param>
        /// <param name="id">The notification ID</param>
        /// <param name="ic">The IC code</param>
        /// <param name="userid">The user ID</param>
        /// <returns>List of notifications</returns>
        List<Notifications> LoadNotifications(string act, string pa, string detail, int id, string ic, string userid);

        /// <summary>
        /// Review notification status
        /// </summary>
        /// <param name="act">The action type</param>
        /// <param name="pa">The PA code</param>
        /// <param name="detail">Additional details</param>
        /// <param name="id">The notification ID</param>
        /// <param name="ic">The IC code</param>
        /// <param name="userid">The user ID</param>
        /// <returns>List of notification status</returns>
        List<NotificationStatus> ReviewNotifications(string act, string pa, string detail, int id, string ic, string userid);

        /// <summary>
        /// Review email status for a notification
        /// </summary>
        /// <param name="id">The notification ID</param>
        /// <returns>List of email status</returns>
        List<EmailStatus> ReviewEmailStatus(int id);

        /// <summary>
        /// Load email position list
        /// </summary>
        /// <returns>List of email positions</returns>
        List<EmailPositions> LoadEmailPositionList();

        /// <summary>
        /// Get notice/return message from operations
        /// </summary>
        /// <param name="act">The action type</param>
        /// <param name="pa">The PA code</param>
        /// <param name="detail">Additional details</param>
        /// <param name="id">The notification ID</param>
        /// <param name="name">Template name</param>
        /// <param name="subject">Email subject</param>
        /// <param name="ic">The IC code</param>
        /// <param name="userid">The user ID</param>
        /// <returns>Return notice message</returns>
        string GetNotice(string act, string pa, string detail, int id, string name, string subject, string ic, string userid);

        /// <summary>
        /// Load email templates
        /// </summary>
        /// <returns>List of email templates</returns>
        List<EmailTemplates> LoadEmailTemplates();

        /// <summary>
        /// Load email rules list
        /// </summary>
        /// <returns>List of email rules</returns>
        List<EmailRules> LoadEmailRulesList();

        /// <summary>
        /// Load specific email rule
        /// </summary>
        /// <param name="act">The action type</param>
        /// <param name="pa">The PA code</param>
        /// <param name="detail">Additional details</param>
        /// <param name="id">The rule ID</param>
        /// <param name="ic">The IC code</param>
        /// <param name="userid">The user ID</param>
        /// <returns>List of email rule details</returns>
        List<EmailRule> LoadEmailRule(string act, string pa, string detail, int id, string ic, string userid);
    }
}