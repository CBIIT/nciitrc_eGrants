using System;
using System.Collections.Generic;
using System.Linq;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using CommonUtilties;

namespace Router
{
    /// <summary>
    /// Provides IMAP-based email reading, moving, and forwarding for the Router project.
    /// Replaces Outlook COM automation with MailKit.
    /// 
    /// Uses IMAP with OAuth 2.0 (Modern Authentication) for reading/moving emails
    /// and SMTP for sending/forwarding.
    /// 
    /// Configuration (appsettings.json):
    /// "IMAP": {
    ///     "Host": "outlook.office365.com",
    ///     "Port": 993,
    ///     "UseSsl": true,
    ///     "Username": "NCIOGAeGrantsDev@mail.nih.gov",
    ///     "TenantId": "your-azure-ad-tenant-id",
    ///     "ClientId": "your-azure-ad-app-client-id",
    ///     "ClientSecret": "your-azure-ad-app-client-secret"
    /// },
    /// "SMTP": {
    ///     "Host": "mail.nih.gov",
    ///     "Port": 25,
    ///     "FromAddress": "NCIOGAeGrantsDev@mail.nih.gov",
    ///     "FromName": "eGrants Router"
    /// }
    /// </summary>
    public class ImapEmailService : IDisposable
    {
        private readonly string _imapHost;
        private readonly int _imapPort;
        private readonly bool _imapUseSsl;
        private readonly string _imapUsername;
        private readonly string _tenantId;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _fromAddress;
        private readonly string _fromName;
        private ImapClient _imapClient;

        public ImapEmailService(IConfiguration config)
        {
            _imapHost = config["IMAP:Host"] ?? "outlook.office365.com";
            _imapPort = int.TryParse(config["IMAP:Port"], out var imapPort) ? imapPort : 993;
            _imapUseSsl = !string.Equals(config["IMAP:UseSsl"], "false", StringComparison.OrdinalIgnoreCase);
            _imapUsername = config["IMAP:Username"] ?? "";
            _tenantId = config["IMAP:TenantId"] ?? "";
            _clientId = config["IMAP:ClientId"] ?? "";
            _clientSecret = config["IMAP:ClientSecret"] ?? "";
            _smtpHost = config["SMTP:Host"] ?? "mail.nih.gov";
            _smtpPort = int.TryParse(config["SMTP:Port"], out var smtpPort) ? smtpPort : 25;
            _fromAddress = config["SMTP:FromAddress"] ?? "NCIOGAeGrantsDev@mail.nih.gov";
            _fromName = config["SMTP:FromName"] ?? "eGrants Router";

            CommonUtilities.Logger?.Information("ImapEmailService initialized with IMAP Host={ImapHost}, Port={ImapPort}, SSL={UseSsl}, Username={Username}",
                _imapHost, _imapPort, _imapUseSsl, _imapUsername);
            CommonUtilities.Logger?.Information("ImapEmailService OAuth config: TenantId={TenantId}, ClientId={ClientId}, ClientSecret={(SecretPresent)}",
                _tenantId, _clientId, !string.IsNullOrEmpty(_clientSecret) ? "***present***" : "***MISSING***");
            CommonUtilities.Logger?.Information("ImapEmailService SMTP config: Host={SmtpHost}, Port={SmtpPort}, From={FromAddress} ({FromName})",
                _smtpHost, _smtpPort, _fromAddress, _fromName);

            if (string.IsNullOrWhiteSpace(_tenantId))
                CommonUtilities.Logger?.Warning("IMAP: TenantId is empty or missing in configuration. OAuth authentication will fail.");
            if (string.IsNullOrWhiteSpace(_clientId))
                CommonUtilities.Logger?.Warning("IMAP: ClientId is empty or missing in configuration. OAuth authentication will fail.");
            if (string.IsNullOrWhiteSpace(_clientSecret))
                CommonUtilities.Logger?.Warning("IMAP: ClientSecret is empty or missing in configuration. OAuth authentication will fail.");
            if (string.IsNullOrWhiteSpace(_imapUsername))
                CommonUtilities.Logger?.Warning("IMAP: Username is empty or missing in configuration. IMAP authentication will fail.");
        }

        /// <summary>
        /// Acquires an OAuth 2.0 access token using MSAL client credentials flow.
        /// Requires an Azure AD app registration with the IMAP.AccessAsApp permission.
        /// </summary>
        private string AcquireOAuthToken()
        {
            var authority = $"https://login.microsoftonline.com/{_tenantId}";
            CommonUtilities.Logger?.Information("OAuth: Building ConfidentialClientApplication. Authority={Authority}, ClientId={ClientId}", authority, _clientId);

            IConfidentialClientApplication app;
            try
            {
                app = ConfidentialClientApplicationBuilder
                    .Create(_clientId)
                    .WithClientSecret(_clientSecret)
                    .WithAuthority(new Uri(authority))
                    .Build();
                CommonUtilities.Logger?.Information("OAuth: ConfidentialClientApplication built successfully.");
            }
            catch (Exception ex)
            {
                CommonUtilities.Logger?.Error(ex, "OAuth: Failed to build ConfidentialClientApplication. Error: {ErrorMessage}", ex.Message);
                throw;
            }

            // The scope for Exchange Online IMAP access via client credentials
            var scopes = new[] { "https://outlook.office365.com/.default" };
            CommonUtilities.Logger?.Information("OAuth: Requesting token with scopes: [{Scopes}]", string.Join(", ", scopes));

            AuthenticationResult result;
            try
            {
                result = app.AcquireTokenForClient(scopes).ExecuteAsync().GetAwaiter().GetResult();
                CommonUtilities.Logger?.Information("OAuth: Token acquired successfully. ExpiresOn={ExpiresOn}, TokenType={TokenType}, Scopes=[{Scopes}]",
                    result.ExpiresOn, "Bearer", string.Join(", ", scopes));
                CommonUtilities.Logger?.Debug("OAuth: AccessToken length={Length} chars", result.AccessToken?.Length ?? 0);
            }
            catch (MsalServiceException ex)
            {
                CommonUtilities.Logger?.Error(ex, "OAuth: MSAL service error acquiring token. ErrorCode={ErrorCode}, StatusCode={StatusCode}, Error: {ErrorMessage}",
                    ex.ErrorCode, ex.StatusCode, ex.Message);
                throw;
            }
            catch (MsalClientException ex)
            {
                CommonUtilities.Logger?.Error(ex, "OAuth: MSAL client error acquiring token. ErrorCode={ErrorCode}, Error: {ErrorMessage}",
                    ex.ErrorCode, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                CommonUtilities.Logger?.Error(ex, "OAuth: Unexpected error acquiring token. Error: {ErrorMessage}", ex.Message);
                throw;
            }

            return result.AccessToken;
        }

        /// <summary>
        /// Connects to the IMAP server using OAuth 2.0 (Modern Authentication).
        /// Uses MSAL client credentials flow to obtain an access token, then
        /// authenticates to IMAP using the XOAUTH2 SASL mechanism.
        /// </summary>
        public void Connect()
        {
            CommonUtilities.Logger?.Information("IMAP: Creating ImapClient...");
            _imapClient = new ImapClient();

            var sslOptions = _imapUseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.None;
            CommonUtilities.Logger?.Information("IMAP: Connecting to {Host}:{Port} with SSL={SslOptions}...", _imapHost, _imapPort, sslOptions);

            try
            {
                _imapClient.Connect(_imapHost, _imapPort, sslOptions);
                CommonUtilities.Logger?.Information("IMAP: TCP connection established successfully to {Host}:{Port}", _imapHost, _imapPort);
            }
            catch (Exception ex)
            {
                CommonUtilities.Logger?.Error(ex, "IMAP: Failed to connect to {Host}:{Port}. Error: {ErrorMessage}", _imapHost, _imapPort, ex.Message);
                throw;
            }

            // Log IMAP server capabilities before auth
            CommonUtilities.Logger?.Information("IMAP: Server capabilities before auth: {Capabilities}", _imapClient.Capabilities);
            if (_imapClient.AuthenticationMechanisms != null)
            {
                CommonUtilities.Logger?.Information("IMAP: Available authentication mechanisms: [{Mechanisms}]",
                    string.Join(", ", _imapClient.AuthenticationMechanisms));
            }

            // Acquire OAuth 2.0 token
            CommonUtilities.Logger?.Information("IMAP: Acquiring OAuth 2.0 token for user '{Username}'...", _imapUsername);
            string accessToken;
            try
            {
                accessToken = AcquireOAuthToken();
                CommonUtilities.Logger?.Information("IMAP: OAuth token acquired. Proceeding to IMAP XOAUTH2 authentication...");
            }
            catch (Exception ex)
            {
                CommonUtilities.Logger?.Error(ex, "IMAP: Failed to acquire OAuth token. Cannot authenticate. Error: {ErrorMessage}", ex.Message);
                throw;
            }

            // Authenticate using XOAUTH2 SASL mechanism
            CommonUtilities.Logger?.Information("IMAP: Authenticating via XOAUTH2 as '{Username}'...", _imapUsername);
            try
            {
                var oauth2 = new SaslMechanismOAuth2(_imapUsername, accessToken);
                _imapClient.Authenticate(oauth2);
                CommonUtilities.Logger?.Information("IMAP: OAuth2 XOAUTH2 authentication successful for '{Username}'", _imapUsername);
            }
            catch (AuthenticationException ex)
            {
                CommonUtilities.Logger?.Error(ex, "IMAP: OAuth2 authentication FAILED for '{Username}'. Error: {ErrorMessage}", _imapUsername, ex.Message);
                CommonUtilities.Logger?.Error("IMAP: Ensure the Azure AD app registration has 'IMAP.AccessAsApp' permission and admin consent has been granted.");
                CommonUtilities.Logger?.Error("IMAP: Ensure a service principal is registered for the mailbox and full_access_as_app is granted via Exchange Online PowerShell.");
                throw;
            }
            catch (Exception ex)
            {
                CommonUtilities.Logger?.Error(ex, "IMAP: Unexpected error during OAuth2 authentication for '{Username}'. Error: {ErrorMessage}", _imapUsername, ex.Message);
                throw;
            }

            // Log capabilities after auth (may differ)
            CommonUtilities.Logger?.Information("IMAP: Server capabilities after auth: {Capabilities}", _imapClient.Capabilities);

            // Log available namespaces for debugging folder path issues
            CommonUtilities.Logger?.Information("IMAP: Connected and authenticated. Listing namespaces...");
            if (_imapClient.PersonalNamespaces != null)
            {
                foreach (var ns in _imapClient.PersonalNamespaces)
                    CommonUtilities.Logger?.Information("IMAP: Personal namespace: Path='{Path}', Separator='{Separator}'", ns.Path, ns.DirectorySeparator);
            }
            if (_imapClient.SharedNamespaces != null)
            {
                foreach (var ns in _imapClient.SharedNamespaces)
                    CommonUtilities.Logger?.Information("IMAP: Shared namespace: Path='{Path}', Separator='{Separator}'", ns.Path, ns.DirectorySeparator);
            }
            if (_imapClient.OtherNamespaces != null)
            {
                foreach (var ns in _imapClient.OtherNamespaces)
                    CommonUtilities.Logger?.Information("IMAP: Other namespace: Path='{Path}', Separator='{Separator}'", ns.Path, ns.DirectorySeparator);
            }
        }

        /// <summary>
        /// Opens a mailbox folder by path. The path uses '/' as separator.
        /// The dirPath from config uses backslashes and includes the Outlook display prefix
        /// (e.g. "Public Folders - user@nih.gov\All Public Folders\NCI\...").
        /// This method converts from the Outlook-style path to IMAP path.
        /// </summary>
        public IMailFolder GetFolder(string outlookStylePath)
        {
            CommonUtilities.Logger?.Information("IMAP: GetFolder called with Outlook-style path: '{OutlookPath}'", outlookStylePath);

            // Convert Outlook-style path to IMAP folder path
            string imapPath = ConvertOutlookPathToImap(outlookStylePath);
            CommonUtilities.Logger?.Information("IMAP: Converted to IMAP path: '{ImapPath}'", imapPath);

            try
            {
                var folder = _imapClient.GetFolder(imapPath);
                CommonUtilities.Logger?.Information("IMAP: Found folder '{ImapPath}'. Opening with ReadWrite access...", imapPath);
                folder.Open(FolderAccess.ReadWrite);
                CommonUtilities.Logger?.Information("IMAP: Folder '{ImapPath}' opened successfully. Message count: {Count}", imapPath, folder.Count);
                return folder;
            }
            catch (FolderNotFoundException ex)
            {
                CommonUtilities.Logger?.Error(ex, "IMAP: Folder NOT FOUND at path '{ImapPath}'. Original Outlook path was '{OutlookPath}'", imapPath, outlookStylePath);

                // Log available top-level folders to help debug
                CommonUtilities.Logger?.Information("IMAP: Listing available top-level folders for debugging...");
                try
                {
                    var personal = _imapClient.GetFolder(_imapClient.PersonalNamespaces[0]);
                    foreach (var sub in personal.GetSubfolders(false))
                        CommonUtilities.Logger?.Information("IMAP:   Top-level folder: '{FolderName}' (FullName: '{FullName}')", sub.Name, sub.FullName);
                }
                catch (Exception listEx)
                {
                    CommonUtilities.Logger?.Warning(listEx, "IMAP: Could not list top-level folders: {ErrorMessage}", listEx.Message);
                }

                throw;
            }
            catch (Exception ex)
            {
                CommonUtilities.Logger?.Error(ex, "IMAP: Error getting folder at path '{ImapPath}'. Error: {ErrorMessage}", imapPath, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Gets a subfolder by name from a parent folder.
        /// </summary>
        public IMailFolder GetSubfolder(IMailFolder parent, string subfolderName)
        {
            CommonUtilities.Logger?.Information("IMAP: Getting subfolder '{SubfolderName}' from parent '{ParentFolder}'", subfolderName, parent.FullName);

            try
            {
                var subfolder = parent.GetSubfolder(subfolderName);
                CommonUtilities.Logger?.Information("IMAP: Found subfolder '{SubfolderName}' (FullName: '{FullName}')", subfolderName, subfolder.FullName);
                return subfolder;
            }
            catch (FolderNotFoundException ex)
            {
                CommonUtilities.Logger?.Error(ex, "IMAP: Subfolder '{SubfolderName}' NOT FOUND under '{ParentFolder}'", subfolderName, parent.FullName);

                // Log available subfolders to help debug
                CommonUtilities.Logger?.Information("IMAP: Listing available subfolders of '{ParentFolder}'...", parent.FullName);
                try
                {
                    foreach (var sub in parent.GetSubfolders(false))
                        CommonUtilities.Logger?.Information("IMAP:   Subfolder: '{FolderName}' (FullName: '{FullName}')", sub.Name, sub.FullName);
                }
                catch (Exception listEx)
                {
                    CommonUtilities.Logger?.Warning(listEx, "IMAP: Could not list subfolders: {ErrorMessage}", listEx.Message);
                }

                throw;
            }
            catch (Exception ex)
            {
                CommonUtilities.Logger?.Error(ex, "IMAP: Error getting subfolder '{SubfolderName}' from '{ParentFolder}'. Error: {ErrorMessage}",
                    subfolderName, parent.FullName, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Gets all mail messages from a folder.
        /// Returns a list of RouterMailItem wrappers.
        /// </summary>
        public List<RouterMailItem> GetEmails(IMailFolder folder)
        {
            CommonUtilities.Logger?.Information("IMAP: Searching for all emails in folder '{FolderName}'...", folder.FullName);

            var items = new List<RouterMailItem>();
            IList<UniqueId> uids;

            try
            {
                uids = folder.Search(MailKit.Search.SearchQuery.All);
                CommonUtilities.Logger?.Information("IMAP: Search returned {Count} message UID(s) in '{FolderName}'", uids.Count, folder.FullName);
            }
            catch (Exception ex)
            {
                CommonUtilities.Logger?.Error(ex, "IMAP: Failed to search folder '{FolderName}'. Error: {ErrorMessage}", folder.FullName, ex.Message);
                throw;
            }

            foreach (var uid in uids)
            {
                try
                {
                    CommonUtilities.Logger?.Debug("IMAP: Fetching message UID={Uid} from '{FolderName}'...", uid, folder.FullName);
                    var message = folder.GetMessage(uid);

                    var senderAddress = message.From?.Mailboxes?.FirstOrDefault()?.Address ?? "";
                    var senderName = message.From?.Mailboxes?.FirstOrDefault()?.Name ?? "";

                    CommonUtilities.Logger?.Information("IMAP: Loaded message UID={Uid}: Subject='{Subject}', From='{SenderName}' <{SenderAddress}>, Date={Date}",
                        uid, message.Subject ?? "(no subject)", senderName, senderAddress, message.Date);

                    items.Add(new RouterMailItem
                    {
                        UniqueId = uid,
                        Subject = message.Subject ?? "",
                        Body = message.TextBody ?? "",
                        HtmlBody = message.HtmlBody ?? "",
                        SenderAddress = senderAddress,
                        SenderName = senderName,
                        ReceivedTime = message.Date.LocalDateTime,
                        MimeMessage = message
                    });
                }
                catch (Exception ex)
                {
                    CommonUtilities.Logger?.Error(ex, "IMAP: Failed to fetch message UID={Uid} from '{FolderName}'. Error: {ErrorMessage}. Skipping this message.",
                        uid, folder.FullName, ex.Message);
                }
            }

            CommonUtilities.Logger?.Information("IMAP: Successfully loaded {Count} email(s) from '{FolderName}'", items.Count, folder.FullName);
            return items;
        }

        /// <summary>
        /// Moves a message to the specified destination folder.
        /// </summary>
        public void MoveMessage(IMailFolder sourceFolder, UniqueId uid, IMailFolder destinationFolder)
        {
            CommonUtilities.Logger?.Information("IMAP: Moving message UID={Uid} from '{Source}' to '{Destination}'...",
                uid, sourceFolder.FullName, destinationFolder.FullName);

            try
            {
                var newUid = sourceFolder.MoveTo(uid, destinationFolder);
                CommonUtilities.Logger?.Information("IMAP: Message UID={Uid} moved successfully from '{Source}' to '{Destination}'. New UID={NewUid}",
                    uid, sourceFolder.FullName, destinationFolder.FullName, newUid);
            }
            catch (Exception ex)
            {
                CommonUtilities.Logger?.Error(ex, "IMAP: Failed to move message UID={Uid} from '{Source}' to '{Destination}'. Error: {ErrorMessage}",
                    uid, sourceFolder.FullName, destinationFolder.FullName, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Creates a forward of the given message.
        /// Returns a RouterOutgoingMail that can have recipients added and be sent.
        /// </summary>
        public RouterOutgoingMail Forward(RouterMailItem originalItem)
        {
            CommonUtilities.Logger?.Information("IMAP: Creating forward of message: Subject='{Subject}', UID={Uid}",
                originalItem.Subject, originalItem.UniqueId);

            var forward = new RouterOutgoingMail
            {
                Subject = "FW: " + originalItem.Subject,
                OriginalMessage = originalItem.MimeMessage
            };

            CommonUtilities.Logger?.Debug("IMAP: Forward created with subject: '{Subject}'", forward.Subject);
            return forward;
        }

        /// <summary>
        /// Sends an outgoing mail (forward or new message) via SMTP.
        /// </summary>
        public void Send(RouterOutgoingMail outgoing)
        {
            CommonUtilities.Logger?.Information("SMTP: Preparing to send email. Subject='{Subject}', Recipients={RecipientCount}, IsForward={IsForward}",
                outgoing.Subject, outgoing.Recipients.Count, outgoing.OriginalMessage != null);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_fromName, _fromAddress));

            foreach (var recipient in outgoing.Recipients)
            {
                CommonUtilities.Logger?.Debug("SMTP: Adding recipient: {Recipient}", recipient);
                message.To.Add(MailboxAddress.Parse(recipient));
            }

            message.Subject = outgoing.Subject;
            CommonUtilities.Logger?.Debug("SMTP: From={From}, Subject='{Subject}'", _fromAddress, outgoing.Subject);

            if (outgoing.OriginalMessage != null)
            {
                // Build a forwarded message body
                var builder = new BodyBuilder();
                var originalHtml = outgoing.OriginalMessage.HtmlBody;
                var originalText = outgoing.OriginalMessage.TextBody;

                CommonUtilities.Logger?.Debug("SMTP: Building forwarded message body. OriginalHtml={HasHtml}, OriginalText={HasText}, Attachments={AttachmentCount}",
                    !string.IsNullOrEmpty(originalHtml), !string.IsNullOrEmpty(originalText), outgoing.OriginalMessage.Attachments.Count());

                if (!string.IsNullOrEmpty(outgoing.HtmlBody))
                {
                    builder.HtmlBody = outgoing.HtmlBody + "<br/><hr/><b>--- Forwarded message ---</b><br/>" + (originalHtml ?? originalText ?? "");
                }
                else
                {
                    builder.HtmlBody = "<b>--- Forwarded message ---</b><br/>" + (originalHtml ?? originalText ?? "");
                }

                // Carry over attachments from original
                int attachmentCount = 0;
                foreach (var attachment in outgoing.OriginalMessage.Attachments)
                {
                    builder.Attachments.Add(attachment);
                    attachmentCount++;
                }
                if (attachmentCount > 0)
                    CommonUtilities.Logger?.Information("SMTP: Carried over {Count} attachment(s) from original message", attachmentCount);

                message.Body = builder.ToMessageBody();
            }
            else
            {
                // New message (not a forward)
                CommonUtilities.Logger?.Debug("SMTP: Building new message body (not a forward)");
                var builder = new BodyBuilder();
                builder.HtmlBody = outgoing.HtmlBody ?? outgoing.TextBody ?? "";
                message.Body = builder.ToMessageBody();
            }

            CommonUtilities.Logger?.Information("SMTP: Connecting to {Host}:{Port}...", _smtpHost, _smtpPort);
            using (var smtpClient = new MailKit.Net.Smtp.SmtpClient())
            {
                try
                {
                    smtpClient.Connect(_smtpHost, _smtpPort, SecureSocketOptions.None);
                    CommonUtilities.Logger?.Information("SMTP: Connected to {Host}:{Port} successfully", _smtpHost, _smtpPort);
                }
                catch (Exception ex)
                {
                    CommonUtilities.Logger?.Error(ex, "SMTP: Failed to connect to {Host}:{Port}. Error: {ErrorMessage}", _smtpHost, _smtpPort, ex.Message);
                    throw;
                }

                try
                {
                    smtpClient.Send(message);
                    CommonUtilities.Logger?.Information("SMTP: Email sent successfully. Subject='{Subject}', To={Recipients}",
                        outgoing.Subject, string.Join("; ", outgoing.Recipients));
                }
                catch (Exception ex)
                {
                    CommonUtilities.Logger?.Error(ex, "SMTP: Failed to send email. Subject='{Subject}', To={Recipients}. Error: {ErrorMessage}",
                        outgoing.Subject, string.Join("; ", outgoing.Recipients), ex.Message);
                    throw;
                }

                smtpClient.Disconnect(true);
                CommonUtilities.Logger?.Debug("SMTP: Disconnected from {Host}:{Port}", _smtpHost, _smtpPort);
            }
        }

        /// <summary>
        /// Sends a simple HTML email via SMTP.
        /// </summary>
        public void SendEmail(string to, string subject, string htmlBody)
        {
            CommonUtilities.Logger?.Information("SMTP: SendEmail called. To='{To}', Subject='{Subject}'", to, subject);

            var outgoing = new RouterOutgoingMail
            {
                Subject = subject,
                HtmlBody = htmlBody
            };
            foreach (var addr in to.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                outgoing.Recipients.Add(addr.Trim());
            }
            Send(outgoing);
        }

        /// <summary>
        /// Converts an Outlook-style public folder path to an IMAP folder path.
        /// 
        /// Outlook format: "Public Folders - user@nih.gov\All Public Folders\NCI\GAB\eGrantsDev\emailRouterTestRB\"
        /// IMAP format:    "Public Folders/NCI/GAB/eGrantsDev/emailRouterTestRB"
        /// 
        /// The conversion:
        /// 1. Strips the "Public Folders - xxx\" prefix
        /// 2. Strips the "All Public Folders\" prefix
        /// 3. Replaces backslashes with the IMAP separator
        /// 4. Trims trailing separators
        /// </summary>
        private string ConvertOutlookPathToImap(string outlookPath)
        {
            if (string.IsNullOrWhiteSpace(outlookPath))
                throw new ArgumentException("Folder path cannot be empty", nameof(outlookPath));

            string path = outlookPath.Trim();
            CommonUtilities.Logger?.Debug("IMAP: ConvertOutlookPathToImap input: '{InputPath}'", path);

            // Detect if this is a legacy Outlook Public Folder path
            bool isPublicFolderPath = path.IndexOf("Public Folders", StringComparison.OrdinalIgnoreCase) >= 0
                                   || path.IndexOf("All Public Folders", StringComparison.OrdinalIgnoreCase) >= 0;

            if (isPublicFolderPath)
            {
                CommonUtilities.Logger?.Warning("IMAP: Public Folder path detected. IMAP in Exchange Online does NOT support Public Folders. " +
                    "Update 'dirpathRouter' in appsettings to use a mailbox folder path (e.g., 'INBOX' or 'DevTest'). " +
                    "Original path: '{OutlookPath}'", path);

                // Strip "Public Folders - xxx\" prefix and "All Public Folders\" prefix
                int allPublicIdx = path.IndexOf("All Public Folders", StringComparison.OrdinalIgnoreCase);
                if (allPublicIdx >= 0)
                {
                    path = path.Substring(allPublicIdx + "All Public Folders".Length);
                    CommonUtilities.Logger?.Debug("IMAP: After stripping 'All Public Folders' prefix: '{Path}'", path);
                }
                else
                {
                    int firstSep = path.IndexOf('\\');
                    if (firstSep >= 0)
                    {
                        path = path.Substring(firstSep);
                        CommonUtilities.Logger?.Debug("IMAP: After stripping first segment: '{Path}'", path);
                    }
                }
            }

            // Get the IMAP namespace separator (usually '/')
            char separator = _imapClient?.PersonalNamespaces?.FirstOrDefault()?.DirectorySeparator ?? '/';
            CommonUtilities.Logger?.Debug("IMAP: Using separator: '{Separator}'", separator);

            // Replace backslashes with IMAP separator and trim
            path = path.Replace('\\', separator);
            path = path.Trim(separator);
            CommonUtilities.Logger?.Debug("IMAP: After replacing separators and trimming: '{Path}'", path);

            CommonUtilities.Logger?.Information("IMAP: Final IMAP folder path: '{FinalPath}'", path);
            return path;
        }

        public void Dispose()
        {
            CommonUtilities.Logger?.Information("IMAP: Disposing ImapEmailService...");
            if (_imapClient != null)
            {
                if (_imapClient.IsConnected)
                {
                    CommonUtilities.Logger?.Information("IMAP: Disconnecting from {Host}...", _imapHost);
                    try
                    {
                        _imapClient.Disconnect(true);
                        CommonUtilities.Logger?.Information("IMAP: Disconnected successfully");
                    }
                    catch (Exception ex)
                    {
                        CommonUtilities.Logger?.Warning(ex, "IMAP: Error during disconnect: {ErrorMessage}", ex.Message);
                    }
                }
                _imapClient.Dispose();
                _imapClient = null;
                CommonUtilities.Logger?.Information("IMAP: ImapClient disposed");
            }
        }
    }

    /// <summary>
    /// Represents an email read from the mailbox via IMAP.
    /// Replaces the dynamic Outlook COM MailItem.
    /// </summary>
    public class RouterMailItem
    {
        public UniqueId UniqueId { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string HtmlBody { get; set; }
        public string SenderAddress { get; set; }
        public string SenderName { get; set; }
        public DateTime ReceivedTime { get; set; }
        public MimeMessage MimeMessage { get; set; }
    }

    /// <summary>
    /// Represents an outgoing email to be sent via SMTP.
    /// Replaces the dynamic Outlook COM MailItem for outbound messages.
    /// </summary>
    public class RouterOutgoingMail
    {
        public string Subject { get; set; }
        public string TextBody { get; set; }
        public string HtmlBody { get; set; }
        public List<string> Recipients { get; set; } = new List<string>();
        public MimeMessage OriginalMessage { get; set; }

        public void AddRecipient(string email)
        {
            if (!string.IsNullOrWhiteSpace(email))
                Recipients.Add(email.Trim());
        }
    }
}
