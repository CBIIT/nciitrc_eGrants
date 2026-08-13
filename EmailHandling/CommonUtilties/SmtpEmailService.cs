using System;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace CommonUtilties
{
    /// <summary>
    /// Sends emails via SMTP relay. Replaces Outlook COM automation for projects
    /// that only need to send email (no mailbox reading).
    /// 
    /// Uses the internal SMTP relay (port 25) which does not require credentials
    /// when the server IP is trusted by the relay.
    /// 
    /// Configuration (appsettings.json):
    /// "SMTP": {
    ///     "Host": "mail.nih.gov",
    ///     "Port": 25,
    ///     "FromAddress": "egrants@mail.nih.gov",
    ///     "FromName": "eGrants System"
    /// }
    /// </summary>
    public class SmtpEmailService
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _fromAddress;
        private readonly string _fromName;

        public SmtpEmailService(IConfiguration config)
        {
            _host = config["SMTP:Host"] ?? "mail.nih.gov";
            _port = int.TryParse(config["SMTP:Port"], out var port) ? port : 25;
            _fromAddress = config["SMTP:FromAddress"] ?? "egrants@mail.nih.gov";
            _fromName = config["SMTP:FromName"] ?? "eGrants System";
        }

        public SmtpEmailService(string host, int port, string fromAddress, string fromName = "eGrants System")
        {
            _host = host;
            _port = port;
            _fromAddress = fromAddress;
            _fromName = fromName;
        }

        /// <summary>
        /// Sends an HTML email. Recipients are semicolon-separated.
        /// </summary>
        public void SendEmail(string to, string subject, string htmlBody, string cc = null,
            MailPriority priority = MailPriority.Normal)
        {
            using (var message = new MailMessage())
            {
                message.From = new MailAddress(_fromAddress, _fromName);
                message.Subject = subject;
                message.Body = htmlBody;
                message.IsBodyHtml = true;
                message.Priority = priority;

                foreach (var addr in to.Split(';', StringSplitOptions.RemoveEmptyEntries))
                {
                    var trimmed = addr.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmed))
                        message.To.Add(trimmed);
                }

                if (!string.IsNullOrWhiteSpace(cc))
                {
                    foreach (var addr in cc.Split(';', StringSplitOptions.RemoveEmptyEntries))
                    {
                        var trimmed = addr.Trim();
                        if (!string.IsNullOrWhiteSpace(trimmed))
                            message.CC.Add(trimmed);
                    }
                }

                using (var client = new SmtpClient(_host, _port))
                {
                    client.UseDefaultCredentials = false;
                    client.Credentials = CredentialCache.DefaultNetworkCredentials;
                    client.EnableSsl = false; // Internal relay, no TLS needed on port 25
                    client.Send(message);
                }
            }
        }

        /// <summary>
        /// Sends an HTML email with voting options header.
        /// Note: SMTP voting options use the X-Microsoft-Outlook-VotingOptions header,
        /// which is recognized by Outlook clients but not guaranteed on all mail clients.
        /// </summary>
        public void SendEmailWithVoting(string to, string subject, string htmlBody,
            string votingOptions, string cc = null, MailPriority priority = MailPriority.Normal)
        {
            using (var message = new MailMessage())
            {
                message.From = new MailAddress(_fromAddress, _fromName);
                message.Subject = subject;
                message.Body = htmlBody;
                message.IsBodyHtml = true;
                message.Priority = priority;

                // Add voting options header (Outlook clients will show voting buttons)
                message.Headers.Add("X-Microsoft-Outlook-VotingOptions", votingOptions);

                foreach (var addr in to.Split(';', StringSplitOptions.RemoveEmptyEntries))
                {
                    var trimmed = addr.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmed))
                        message.To.Add(trimmed);
                }

                if (!string.IsNullOrWhiteSpace(cc))
                {
                    foreach (var addr in cc.Split(';', StringSplitOptions.RemoveEmptyEntries))
                    {
                        var trimmed = addr.Trim();
                        if (!string.IsNullOrWhiteSpace(trimmed))
                            message.CC.Add(trimmed);
                    }
                }

                using (var client = new SmtpClient(_host, _port))
                {
                    client.UseDefaultCredentials = false;
                    client.Credentials = CredentialCache.DefaultNetworkCredentials;
                    client.EnableSsl = false;
                    client.Send(message);
                }
            }
        }
    }
}
