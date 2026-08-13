using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using CommonUtilties;
using MailKit;
using Microsoft.Extensions.Configuration;

namespace Router
{
    public class Processor
    {
        public static string v_SenderID { get; private set; }

        // Used by tests
        public Dictionary<string, string> emailsSentThisSession { get; private set; }

        private IConfiguration _config;
        private ImapEmailService _imapService;

        public Processor(IConfiguration config = null)
        {
            emailsSentThisSession = new Dictionary<string, string>();
            _config = config;
        }

        public int Process(string dirPath, SqlConnection con, string verbose, string debug, int routingBreakDuration)
        {
            int itemsProcessedCount = 0;
            emailsSentThisSession.Clear();

            CommonUtilities.ShowDiagnosticIfVerbose("Here we go ...", verbose);

            // Connect to Exchange via IMAP (replaces Outlook COM)
            CommonUtilities.Logger?.Information("Processor: Creating ImapEmailService instance...");
            _imapService = new ImapEmailService(_config);
            CommonUtilities.Logger?.Information("Processor: ImapEmailService created. Calling Connect()...");
            _imapService.Connect();
            CommonUtilities.Logger?.Information("Processor: IMAP connection established successfully.");
            CommonUtilities.ShowDiagnosticIfVerbose("Connected to mail server via IMAP.", verbose);

            CommonUtilities.ShowDiagnosticIfVerbose($"Opening SQL connection ...", verbose);
            con.Open();
            CommonUtilities.ShowDiagnosticIfVerbose($"SQL connection opened.", verbose);

            CommonUtilities.ShowDiagnosticIfVerbose($"dirpath: {dirPath}", verbose);
            CommonUtilities.Logger?.Information("Processor: dirPath='{DirPath}'", dirPath);
            if (!string.IsNullOrWhiteSpace(dirPath))
            {
                CommonUtilities.Logger?.Information("Processor: Calling GetFolder for path '{DirPath}'...", dirPath);
                IMailFolder currentFolder = _imapService.GetFolder(dirPath);
                CommonUtilities.Logger?.Information("Processor: GetFolder returned folder '{FolderName}' with {Count} messages.", currentFolder.FullName, currentFolder.Count);
                CommonUtilities.ShowDiagnosticIfVerbose("Finished navigating to folder", verbose);

                CommonUtilities.Logger?.Information("Processor: Calling GetSubfolder for 'Old emails' under '{ParentFolder}'...", currentFolder.FullName);
                IMailFolder oldFolder = _imapService.GetSubfolder(currentFolder, "Old emails");
                CommonUtilities.Logger?.Information("Processor: Got 'Old emails' subfolder: '{FullName}'", oldFolder.FullName);
                CommonUtilities.ShowDiagnosticIfVerbose("went to Old emails", verbose);

                CommonUtilities.Logger?.Information("Processor: Calling GetEmails for folder '{FolderName}'...", currentFolder.FullName);
                List<RouterMailItem> eachEmailToProcess = _imapService.GetEmails(currentFolder);
                CommonUtilities.Logger?.Information("Processor: GetEmails returned {Count} email(s) to process.", eachEmailToProcess.Count);
                CommonUtilities.ShowDiagnosticIfVerbose($"Mail count={eachEmailToProcess.Count}", verbose);
                CommonUtilities.ShowDiagnosticIfVerbose($"staging email list count={eachEmailToProcess.Count}", verbose);

                CommonUtilities.ShowDiagnosticIfVerbose($"****************** starting ********************", verbose);

                foreach (var item in eachEmailToProcess)
                {
                    CommonUtilities.ShowDiagnosticIfVerbose($" ", verbose);
                    CommonUtilities.ShowDiagnosticIfVerbose($"Item : {item.Subject}", verbose);
                    CommonUtilities.Logger?.Information("Processor: Processing email UID={Uid}, Subject='{Subject}', From='{Sender}', ReceivedTime={ReceivedTime}",
                        item.UniqueId, item.Subject, item.SenderAddress, item.ReceivedTime);

                    var v_SubLine = item.Subject;
                    var v_Body = item.Body;

                    CommonUtilities.ShowDiagnosticIfVerbose($"Subject : {v_SubLine}", verbose);

                    bool failedToProcess = false;
                    try
                    {
                        HandleSingleEmail(item, v_SubLine, v_Body, verbose, con, debug);
                    }
                    catch (System.Exception ex)
                    {
                        failedToProcess = true;
                        var _logMessage = $"Error Occured! => EmailSender:{v_SenderID}; Subjectline : {v_SubLine}; Recieved Date: {item.ReceivedTime}";
                        var _errorMessage = $"Error Type : {ex.GetType().FullName}, Error Message: {ex.Message} , Error Stack: {ex.StackTrace}";
                        CommonUtilities.Logger?.Error(ex, "Processor: HandleSingleEmail failed for UID={Uid}, Subject='{Subject}', Sender='{Sender}'. Error: {ErrorMessage}",
                            item.UniqueId, v_SubLine, v_SenderID, ex.Message);
                        var _endTimeStamp = DateTime.Now;
                        int _forAppending = 8;
                        CommonUtilities.WriteLog(_forAppending, _logMessage, _errorMessage, _endTimeStamp);

                        RaiseErrorToAdmin(item, "Error Occured! PROD eMailRouter vbs", _errorMessage);
                    }

                    CommonUtilities.ShowDiagnosticIfVerbose("Incrementing count", verbose);
                    CommonUtilities.Logger?.Information("Processor: Moving message UID={Uid} from '{Source}' to 'Old emails'...", item.UniqueId, currentFolder.FullName);
                    try
                    {
                        _imapService.MoveMessage(currentFolder, item.UniqueId, oldFolder);
                        CommonUtilities.Logger?.Information("Processor: Message UID={Uid} moved successfully.", item.UniqueId);
                    }
                    catch (System.Exception ex)
                    {
                        string message = $"Failed to move an item at {DateTime.UtcNow} UTC. Here is some info : {ex.Message} \r\n {ex.ToString()}";
                        CommonUtilities.Logger?.Error(ex, "Processor: Failed to move message UID={Uid} to 'Old emails'. Error: {ErrorMessage}", item.UniqueId, ex.Message);
                        CommonUtilities.ShowDiagnosticIfVerbose(message, "y");

                        var errorRecipients = _config?["EmailRecipients:ErrorNotificationRecipients"] ?? "egrantsdevs@mail.nih.gov;leul.ayana@nih.gov";
                        CommonUtilities.Logger?.Information("Processor: Sending move-failure notification email to '{Recipients}'...", errorRecipients);
                        _imapService.SendEmail(errorRecipients, GetEnvironmentPrefix() + "Failed to move an item to old.", message);
                        CommonUtilities.Logger?.Information("Processor: Move-failure notification sent.");
                    }
                    Thread.Sleep(routingBreakDuration);

                    CommonUtilities.ShowDiagnosticIfVerbose("current Item moved", verbose);
                    CommonUtilities.ShowDiagnosticIfVerbose("**************************************************************************", verbose);

                    itemsProcessedCount++;

                    if (itemsProcessedCount >= 50)
                    {
                        var errorMessage1 = "Warning! PROD eMailRouter vbs has processed 50 mail items in one instance!";
                        var errorMessage2 = "Hello Admin, 50 items have been processed in one instance and the application is now exiting. Please check whether there is duplicate items processing.";
                        EmailMe(errorMessage1, errorMessage2);
                    }
                }
            }

            CommonUtilities.Logger?.Information("Processor: Finished processing. Total items processed: {Count}", itemsProcessedCount);
            return itemsProcessedCount;
        }

        /// <summary>
        /// Override this method for testing.
        /// </summary>
        protected virtual Dictionary<string, string> Send(RouterOutgoingMail mailItem)
        {
            var prefix = GetEnvironmentPrefix();
            if (!string.IsNullOrEmpty(prefix))
            {
                mailItem.Subject = prefix + mailItem.Subject;
            }

            CommonUtilities.Logger?.Information("Processor.Send: Sending email via SMTP. Subject='{Subject}', Recipients=[{Recipients}], IsForward={IsForward}",
                mailItem.Subject, string.Join("; ", mailItem.Recipients), mailItem.OriginalMessage != null);
            _imapService.Send(mailItem);
            CommonUtilities.Logger?.Information("Processor.Send: Email sent successfully. Subject='{Subject}'", mailItem.Subject);

            return null;
        }

        /// <summary>
        /// Returns the environment name in parentheses (e.g. "(Development) ") if not Production.
        /// Returns empty string for Production or if DOTNET_ENVIRONMENT is not set.
        /// </summary>
        private static string GetEnvironmentPrefix()
        {
            var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            if (string.IsNullOrWhiteSpace(env) || env.Equals("Production", StringComparison.OrdinalIgnoreCase))
                return string.Empty;
            return $"({env}) ";
        }

        private bool EmailMe(string subject, string bodyMessage)
        {
            CommonUtilities.ShowDiagnosticIfVerbose("Issuing email to admin ...", "y");
            var legacyRecipient = _config?["EmailRecipients:LegacyErrorRecipient"] ?? "leul.ayana@nih.gov";
            CommonUtilities.Logger?.Information("Processor.EmailMe: Sending admin email via SMTP. To='{Recipient}', Subject='{Subject}'", legacyRecipient, subject);
            _imapService.SendEmail(legacyRecipient, GetEnvironmentPrefix() + subject, bodyMessage);
            CommonUtilities.Logger?.Information("Processor.EmailMe: Admin email sent successfully.");
            return true;
        }

        public static string StartService(string svcName)
        {
            string objPath = string.Format("Win32_Service.Name='{0}'", svcName);
            throw new NotSupportedException("ManagementObject is not available in this project. StartService functionality is not supported on this platform.");
        }

        private string RaiseErrorToAdmin(RouterMailItem currentItem, string errorMessage1, string errorMessage2)
        {
            CommonUtilities.Logger?.Information("Processor.RaiseErrorToAdmin: Forwarding error email for UID={Uid}, Subject='{Subject}'", currentItem.UniqueId, currentItem.Subject);
            var outmail = _imapService.Forward(currentItem);
            var legacyRecipient = _config?["EmailRecipients:LegacyErrorRecipient"] ?? "leul.ayana@nih.gov";
            outmail.AddRecipient(legacyRecipient);
            outmail.AddRecipient(legacyRecipient);   // NB : original system had this duplicated [sic]
            outmail.Subject = $"{errorMessage1}  >>(Subj: {currentItem.Subject} )";
            CommonUtilities.Logger?.Information("Processor.RaiseErrorToAdmin: Sending forward to '{Recipient}', Subject='{Subject}'", legacyRecipient, outmail.Subject);
            Send(outmail);
            CommonUtilities.Logger?.Information("Processor.RaiseErrorToAdmin: Error notification sent.");
            return "done";
        }

        /// <summary>
        /// Overload for test objects that cannot create real mail items.
        /// </summary>
        public void HandleSingleEmail(string from, string v_SubLine, string v_Body, string verbose, SqlConnection con, string debug)
        {
            // Create a synthetic RouterMailItem for testing
            var testItem = new RouterMailItem
            {
                Subject = v_SubLine,
                Body = v_Body,
                SenderAddress = from,
                SenderName = from,
                ReceivedTime = DateTime.Now
            };
            HandleSingleEmailInternal(testItem, from, v_SubLine, v_Body, verbose, con, debug);
        }

        public void HandleSingleEmail(RouterMailItem currentItem, string v_SubLine, string v_Body, string verbose, SqlConnection con, string debug)
        {
            HandleSingleEmailInternal(currentItem, null, v_SubLine, v_Body, verbose, con, debug);
        }

        private void HandleSingleEmailInternal(RouterMailItem currentItem, string fromOverride, string v_SubLine, string v_Body, string verbose, SqlConnection con, string debug)
        {
            // Helper to create a forward
            RouterOutgoingMail ForwardCurrentItem()
            {
                if (currentItem?.MimeMessage != null)
                {
                    CommonUtilities.Logger?.Information("Processor.ForwardCurrentItem: Creating IMAP forward for UID={Uid}, Subject='{Subject}'", currentItem.UniqueId, currentItem.Subject);
                    var result = _imapService.Forward(currentItem);
                    CommonUtilities.Logger?.Information("Processor.ForwardCurrentItem: Forward created with subject '{Subject}'", result.Subject);
                    return result;
                }
                // Test path: create a new mail with the forwarded content
                CommonUtilities.Logger?.Debug("Processor.ForwardCurrentItem: No MimeMessage available (test path). Creating synthetic forward.");
                var fwd = new RouterOutgoingMail
                {
                    Subject = "FW: " + v_SubLine,
                    HtmlBody = v_Body
                };
                return fwd;
            }

            // Load email recipients from configuration, with fallback to legacy hardcoded values if config not available
            var _dBugEmail = _config?["EmailRecipients:DebugEmail"] ?? "leul.ayana@nih.gov";
            var _eGrantsDevEmail = _config?["EmailRecipients:EGrantsDevEmail"] ?? "eGrantsDev@mail.nih.gov";
            var _eGrantsTestEmail = _config?["EmailRecipients:EGrantsTestEmail"] ?? "eGrantsTest1@mail.nih.gov";
            var _eGrantsStageEmail = _config?["EmailRecipients:EGrantsStageEmail"] ?? "eGrantsStage@mail.nih.gov";
            var _eFileEmail = _config?["EmailRecipients:EFileEmail"] ?? "efile@mail.nih.gov";
            var _nciGrantsPostAwardEmail = _config?["EmailRecipients:NCIGrantsPostAwardEmail"] ?? "NCIGrantsPostAward@nih.gov";

            if (!v_SubLine.ToLower().Contains("undeliverable: "))
            {
                v_SenderID = GetSenderId(currentItem);
                CommonUtilities.Logger?.Information("Processor: SenderID resolved to '{SenderID}' for Subject='{Subject}'", v_SenderID, v_SubLine);
                CommonUtilities.ShowDiagnosticIfVerbose($"Sender : {v_SenderID}", verbose);

                if (v_SubLine.Contains("eSNAP Received at NIH") || v_SubLine.Contains("eRA Commons: RPPR for Grant"))
                {
                    if (v_SubLine.Contains("submitted to NIH with a Non-Compliance"))
                    {
                        //(1) load into eGrants
                        //---- IMP: STRIP SPACES FROM CATEGORY NAME "ERA NOTIFICATION"
                        var replysubj = $"category=eRANotification, sub=RPPR Non-Compliance, extract=1,{v_SubLine}";
                        CommonUtilities.ShowDiagnosticIfVerbose($"Found : {v_SubLine}", verbose);
                        CommonUtilities.ShowDiagnosticIfVerbose($"replysubj : {replysubj}", verbose);
                        var outmail = ForwardCurrentItem();
                        if (debug == "n")
                        {
                            outmail.AddRecipient(_eFileEmail);
                            outmail.AddRecipient(_eGrantsDevEmail);
                            outmail.AddRecipient(_eGrantsTestEmail);
                            outmail.AddRecipient(_eGrantsStageEmail);
                            outmail.Subject = replysubj;
                            Send(outmail);
                        }
                        else
                        {
                            outmail.AddRecipient(_dBugEmail);
                            outmail.AddRecipient(_eGrantsDevEmail);
                            outmail.Subject = replysubj;
                            Send(outmail);
                        }

                    } // end submitted to NIH with a Non-Compliance

                    //(2) forward to Bryan and Nicole
                    var outmail2 = ForwardCurrentItem();
                    if (debug == "n")
                    {
                        var publicAccessRecipients = (_config?["EmailRecipients:PublicAccessComplianceRecipients"] ?? "jonesni@mail.nih.gov;bakerb@mail.nih.gov;edward.mikulich@nih.gov").Split(';');
                        foreach (var recipient in publicAccessRecipients)
                        {
                            outmail2.AddRecipient(recipient.Trim());
                        }
                        Send(outmail2);
                    }
                    else
                    {
                        outmail2.AddRecipient(_dBugEmail);
                        outmail2.AddRecipient(_eGrantsDevEmail);
                        Send(outmail2);
                    }
                }
                else if (v_SubLine.Contains("IC ACTION REQUIRED - Relinquishing Statement"))
                {
                    var outmail2 = ForwardCurrentItem();
                    if (debug == "n")
                    {
                        var relinquishingRecipients = (_config?["EmailRecipients:RelinquishingStatementRecipients"] ?? "emily.driskell@nih.gov;dvellaj@mail.nih.gov;edward.mikulich@nih.gov").Split(';');
                        foreach (var recipient in relinquishingRecipients)
                        {
                            outmail2.AddRecipient(recipient.Trim());
                        }
                        Send(outmail2);
                    }
                    else
                    {
                        CommonUtilities.ShowDiagnosticIfVerbose($"Found subject : {v_SubLine}", verbose);
                        outmail2.AddRecipient(_dBugEmail);
                        outmail2.AddRecipient(_eGrantsDevEmail);
                        Send(outmail2);
                    }
                }
                else if (v_SubLine.Contains(" Supplement Requested through "))
                {
                    CommonUtilities.ShowDiagnosticIfVerbose($"Found subject : {v_SubLine}", verbose);
                    var outmail2 = ForwardCurrentItem();
                    if (debug == "n")
                    {
                        var supplementsEmail = _config?["EmailRecipients:NCIOGASupplementsEmail"] ?? "NCIOGASupplements@mail.nih.gov";
                        outmail2.AddRecipient(supplementsEmail);
                        Send(outmail2);
                    }
                    else
                    {
                        CommonUtilities.ShowDiagnosticIfVerbose($"Found subject : {v_SubLine}", verbose);
                        outmail2.AddRecipient(_dBugEmail);
                        outmail2.AddRecipient(_eGrantsDevEmail);
                        Send(outmail2);
                    }
                }
                else if (v_SubLine.Contains(" FCOI ") && !v_SubLine.StartsWith("Automatic reply:"))
                {
                    string applId = string.Empty;
                    if (!string.IsNullOrWhiteSpace(v_SubLine))
                    {
                        applId = GetApplId(CommonUtilities.RemoveSpaceCharacters(v_SubLine), con);
                        CommonUtilities.ShowDiagnosticIfVerbose($"FCOI => applid : {applId}", verbose);
                    }
                    string p_SpecEmail = String.Empty;
                    string b_SpecEmail = String.Empty;
                    if (!string.IsNullOrWhiteSpace(applId))
                    {
                        var queryText = $"sp_getOfficersEmailForGrantNum";
                        using (SqlCommand command = new SqlCommand(queryText, con))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.Add(new SqlParameter("@APPLID", applId));
                            command.Parameters.Add(new SqlParameter("@OffCode", "SPEC"));
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    p_SpecEmail = $"{reader["Email_address_p"]}";
                                    b_SpecEmail = $"{reader["Email_address_b"]}";
                                    CommonUtilities.ShowDiagnosticIfVerbose($"Return from poroc (SPEC EMAIL)=>{p_SpecEmail}", verbose);
                                    CommonUtilities.ShowDiagnosticIfVerbose($"Return from poroc (BACKUP_SPEC EMAIL)=>{b_SpecEmail}", verbose);
                                }
                            }
                        }
                    }
                    var outmail2 = ForwardCurrentItem();

                    if (debug == "n")
                    {
                        var bobTeamEmail = _config?["EmailRecipients:NCIOGABOBTeamEmail"] ?? "nciogabobteam1@mail.nih.gov";
                        outmail2.AddRecipient(bobTeamEmail);
                        // if they're not equal, send to both
                        if (!string.IsNullOrWhiteSpace(p_SpecEmail) && !string.IsNullOrWhiteSpace(b_SpecEmail)
                            && !p_SpecEmail.Equals(b_SpecEmail, StringComparison.CurrentCultureIgnoreCase))
                        {
                            outmail2.AddRecipient(p_SpecEmail);
                            outmail2.AddRecipient(b_SpecEmail);
                        }
                        // they do equal, just send to p specEmail
                        else if (!string.IsNullOrWhiteSpace(p_SpecEmail) && !string.IsNullOrWhiteSpace(b_SpecEmail)
                            && p_SpecEmail.Equals(b_SpecEmail, StringComparison.CurrentCultureIgnoreCase))
                        {
                            outmail2.AddRecipient(p_SpecEmail);
                        }
                        else if (!string.IsNullOrWhiteSpace(p_SpecEmail) && string.IsNullOrWhiteSpace(b_SpecEmail))
                        {
                            outmail2.AddRecipient(p_SpecEmail);
                        }
                        else if (!string.IsNullOrWhiteSpace(b_SpecEmail) && string.IsNullOrWhiteSpace(p_SpecEmail))
                        {
                            outmail2.AddRecipient(b_SpecEmail);
                        }
                        Send(outmail2);
                    }
                    else
                    {
                        outmail2.AddRecipient(_dBugEmail);
                        outmail2.AddRecipient(_eGrantsDevEmail);
                        var replysubj = string.Empty;
                        // if they're not equal, send to both
                        if (!string.IsNullOrWhiteSpace(p_SpecEmail) && !string.IsNullOrWhiteSpace(b_SpecEmail)
                            && !p_SpecEmail.Equals(b_SpecEmail, StringComparison.CurrentCultureIgnoreCase))
                        {
                            CommonUtilities.ShowDiagnosticIfVerbose($"P={p_SpecEmail}B={b_SpecEmail}", verbose);
                        }
                        // they do equal, just send to p specEmail
                        else if (!string.IsNullOrWhiteSpace(p_SpecEmail) && !string.IsNullOrWhiteSpace(b_SpecEmail)
                            && p_SpecEmail.Equals(b_SpecEmail, StringComparison.CurrentCultureIgnoreCase))
                        {
                            CommonUtilities.ShowDiagnosticIfVerbose($"P={p_SpecEmail}", verbose);
                        }
                        else if (!string.IsNullOrWhiteSpace(p_SpecEmail) && string.IsNullOrWhiteSpace(b_SpecEmail))
                        {
                            CommonUtilities.ShowDiagnosticIfVerbose($"P={p_SpecEmail}", verbose);
                        }
                        else if (!string.IsNullOrWhiteSpace(b_SpecEmail) && string.IsNullOrWhiteSpace(p_SpecEmail))
                        {
                            CommonUtilities.ShowDiagnosticIfVerbose($"B={b_SpecEmail}", verbose);
                        }
                        Send(outmail2);
                    }
                }
                //---- IMP: STRIP SPACES FROM CATEGORY NAME "ERA NOTIFICATION"	
                else if (v_SubLine.Contains("No Cost Extension Submitted"))
                {
                    var replysubj = $"category=eRANotification, sub=No Cost Extension, extract=1,{currentItem.Subject}";
                    CommonUtilities.ShowDiagnosticIfVerbose($"FOUND->{v_SubLine}", verbose);
                    CommonUtilities.ShowDiagnosticIfVerbose($"reply : {replysubj}", verbose);
                    var outmail = ForwardCurrentItem();
                    if (debug == "n")
                    {
                        outmail.AddRecipient(_eFileEmail);
                        outmail.AddRecipient(_eGrantsDevEmail);
                        outmail.AddRecipient(_eGrantsTestEmail);
                        outmail.AddRecipient(_eGrantsStageEmail);
                        outmail.Subject = replysubj;
                        Send(outmail);
                    }
                    else
                    {
                        CommonUtilities.ShowDiagnosticIfVerbose($"reply : {replysubj}", verbose);
                        outmail.AddRecipient(_eGrantsDevEmail);
                        outmail.Subject = replysubj;
                        Send(outmail);
                    }
                }
                else if (v_SubLine.Contains("Change of Institution request for Grant"))
                {
                    var replysubj = currentItem.Subject;
                    var outmail = ForwardCurrentItem();
                    if (debug == "n")
                    {
                        var changeOfInstitutionRecipients = (_config?["EmailRecipients:RelinquishingStatementRecipients"] ?? "emily.driskell@nih.gov;dvellaj@mail.nih.gov;edward.mikulich@nih.gov").Split(';');
                        foreach (var recipient in changeOfInstitutionRecipients)
                        {
                            outmail.AddRecipient(recipient.Trim());
                        }
                        outmail.Subject = replysubj;
                        Send(outmail);
                    }
                    else
                    {
                        CommonUtilities.ShowDiagnosticIfVerbose($"FOUND->{v_SubLine}", verbose);
                        outmail.AddRecipient(_dBugEmail);
                        outmail.AddRecipient(_eGrantsDevEmail);
                        outmail.Subject = replysubj;
                        Send(outmail);
                    }
                }
                else if (v_SenderID.ToLower().Contains("public"))
                {
                    CommonUtilities.ShowDiagnosticIfVerbose($"Found a public access email", verbose);
                    if (!string.IsNullOrWhiteSpace(v_SubLine))
                    {
                        CommonUtilities.ShowDiagnosticIfVerbose($"FOUND subject ->{v_SubLine}", verbose);
                        string[] tokens = v_SubLine.Split(new[] { ": " }, StringSplitOptions.None);
                        var secondPart = tokens[tokens.Length - 1].Trim();
                        CommonUtilities.ShowDiagnosticIfVerbose($"Second part : {secondPart}", verbose);

                        var subCat = string.Empty;
                        if (v_SubLine.ToLower().Contains("compliant"))
                        {
                            CommonUtilities.ShowDiagnosticIfVerbose($"Found compliant", verbose);
                            subCat = "Compliant";
                        }
                        else
                        {
                            CommonUtilities.ShowDiagnosticIfVerbose($"Found non compliant", verbose);
                        }

                        string[] tokens2 = secondPart.Split(new[] { " - " }, StringSplitOptions.None);
                        var middle = tokens2[0];
                        CommonUtilities.ShowDiagnosticIfVerbose($"Isolated middle part :{middle}", verbose);

                        var applId = GetApplId(RemoveSpCharacters(middle), con);
                        CommonUtilities.ShowDiagnosticIfVerbose($"appl id :{applId}", verbose);

                        var replySubj = $"category=PublicAccess, sub={subCat}, applid={applId}, extract=1, {currentItem.Subject}";

                        CommonUtilities.ShowDiagnosticIfVerbose($"dBugEmail :{_dBugEmail}", verbose);
                        CommonUtilities.ShowDiagnosticIfVerbose($"eGrantsDevEmail :{_eGrantsDevEmail}", verbose);
                        CommonUtilities.ShowDiagnosticIfVerbose($"replySubj :{replySubj}", verbose);

                        var outmail = ForwardCurrentItem();
                        if (debug == "n")
                        {
                            outmail.AddRecipient(_eFileEmail);
                            outmail.AddRecipient(_eGrantsDevEmail);
                            outmail.AddRecipient(_eGrantsTestEmail);
                            outmail.AddRecipient(_eGrantsStageEmail);
                            outmail.Subject = replySubj;
                            Send(outmail);
                        }
                        else
                        {
                            CommonUtilities.ShowDiagnosticIfVerbose($"reply : {replySubj}", verbose);
                            outmail.AddRecipient(_dBugEmail);
                            outmail.AddRecipient(_eGrantsDevEmail);
                            outmail.Subject = replySubj;
                            Send(outmail);
                        }
                        CommonUtilities.ShowDiagnosticIfVerbose($"done w / handling a public email", verbose);
                    }
                }
                else if (v_SubLine.Contains("JIT Request for Grant"))
                {
                    var replySubj = $"category=JIT Info, sub=Reminder, extract=1, {currentItem.Subject}";
                    var outmail = ForwardCurrentItem();
                    if (debug == "n")
                    {
                        outmail.AddRecipient(_eFileEmail);
                        outmail.AddRecipient(_eGrantsDevEmail);
                        outmail.AddRecipient(_eGrantsTestEmail);
                        outmail.AddRecipient(_eGrantsStageEmail);
                        outmail.Subject = replySubj;
                        Send(outmail);
                    }
                    else
                    {
                        CommonUtilities.ShowDiagnosticIfVerbose($"DON'T WANT THIS {v_SubLine}", verbose);
                        outmail.AddRecipient(_dBugEmail);
                        outmail.AddRecipient(_eGrantsDevEmail);
                        outmail.Subject = replySubj;
                        Send(outmail);
                    }
                }
                else if (v_SubLine.Contains("JIT Documents Have Been Submitted for Grant"))
                {
                    var replySubj = $"category=eRA Notification, sub=JIT Submitted, extract=1, {currentItem.Subject}";
                    var outmail = ForwardCurrentItem();
                    if (debug == "n")
                    {
                        outmail.AddRecipient(_eFileEmail);
                        outmail.AddRecipient(_eGrantsDevEmail);
                        outmail.AddRecipient(_eGrantsTestEmail);
                        outmail.AddRecipient(_eGrantsStageEmail);
                        outmail.Subject = replySubj;
                        Send(outmail);
                    }
                    else
                    {
                        CommonUtilities.ShowDiagnosticIfVerbose($"DON'T WANT THIS {v_SubLine}", verbose);
                        outmail.AddRecipient(_dBugEmail);
                        outmail.AddRecipient(_eGrantsDevEmail);
                        outmail.Subject = replySubj;
                        Send(outmail);
                    }
                }
                else if (v_SubLine.Contains("NIH Automated Email: ACTION REQUIRED - Overdue Progress Report for Grant"))
                {
                    // July 2024: Per OGA (Lisa Vytlacil) no changes are needed. Leave upload criteria as is.
                    CommonUtilities.ShowDiagnosticIfVerbose("Very old email detected and tagged as NIH Automated Email: ACTION REQUIRED - Overdue Progress Report for Grant", verbose);

                    var replySubj = string.Empty;
                    if (v_SubLine.Contains(" R15 "))
                        replySubj = $"category=eRANotification, sub=Late Progress Report, extract=1, {currentItem.Subject}";
                    CommonUtilities.ShowDiagnosticIfVerbose($"Current subject : {currentItem.Subject}", verbose);
                    CommonUtilities.ShowDiagnosticIfVerbose($"Reply subject :  {replySubj}", verbose);
                    var outmail = ForwardCurrentItem();
                    if (debug == "n")
                    {
                        outmail.AddRecipient(_eFileEmail);
                        outmail.AddRecipient(_eGrantsDevEmail);
                        outmail.AddRecipient(_eGrantsTestEmail);
                        outmail.AddRecipient(_eGrantsStageEmail);
                        outmail.Subject = replySubj;
                        Send(outmail);
                    }
                    else
                    {
                        CommonUtilities.ShowDiagnosticIfVerbose($"DON'T WANT THIS {v_SubLine}", verbose);
                        outmail.AddRecipient(_dBugEmail);
                        outmail.AddRecipient(_eGrantsDevEmail);
                        outmail.Subject = replySubj;
                        Send(outmail);
                    }
                }
                else if (v_SubLine.Contains("Expiring Funds") || v_SubLine.Contains("EXPIRING FUNDS-"))
                {
                    //Only attached document has to be extracted so many make Body=""
                    var replySubj = $"category=Closeout, extract=2, {currentItem.Subject}";
                    CommonUtilities.ShowDiagnosticIfVerbose($"Reply subject : {replySubj}", verbose);
                    var outmail = ForwardCurrentItem();
                    if (debug == "n")
                    {
                        outmail.AddRecipient(_eFileEmail);
                        outmail.AddRecipient(_eGrantsDevEmail);
                        outmail.AddRecipient(_eGrantsTestEmail);
                        outmail.AddRecipient(_eGrantsStageEmail);
                        outmail.Subject = replySubj;
                        Send(outmail);
                    }
                    else
                    {
                        CommonUtilities.ShowDiagnosticIfVerbose($"DON'T WANT THIS {v_SubLine}", verbose);
                        outmail.AddRecipient(_dBugEmail);
                        outmail.AddRecipient(_eGrantsDevEmail);
                        outmail.Subject = replySubj;
                        Send(outmail);
                    }
                }
                else if (v_SubLine.Contains("Prior Approval: "))
                {
                    var outmail = ForwardCurrentItem();
                    if (debug == "n")
                    {
                        outmail.AddRecipient(_nciGrantsPostAwardEmail);
                        Send(outmail);
                    }
                    else
                    {
                        outmail.AddRecipient(_dBugEmail);
                        outmail.AddRecipient(_eGrantsDevEmail);
                        Send(outmail);
                    }
                }
                else if (v_SubLine.Contains("FFR NOTIFICATION : REJECTED"))
                {
                    if (v_SubLine.ToLower().Contains("re: ffr notification") || v_SubLine.ToLower().Contains("fw: ffr notification"))
                    {
                        CommonUtilities.ShowDiagnosticIfVerbose($"DON'T WANT THIS {v_SubLine}", verbose);
                    }
                    else
                    {
                        var replySubj = $"category=Notification, sub=FFR Rejection, extract=1, {currentItem.Subject}";
                        var outmail = ForwardCurrentItem();
                        if (debug == "n")
                        {
                            outmail.AddRecipient(_eFileEmail);
                            outmail.AddRecipient(_eGrantsDevEmail);
                            outmail.AddRecipient(_eGrantsTestEmail);
                            outmail.AddRecipient(_eGrantsStageEmail);
                            outmail.Subject = replySubj;
                            Send(outmail);
                        }
                        else
                        {
                            outmail.AddRecipient(_dBugEmail);
                            outmail.AddRecipient(_eGrantsDevEmail);
                            outmail.Subject = replySubj;
                            Send(outmail);
                        }
                    }
                }
                else if (v_SubLine.Contains("eRA Commons: The Final RPPR - Additional Materials for Award"))
                {
                    if (v_SubLine.Contains("re: eRA Commons: The Final RPPR ") || v_SubLine.Contains("fw: eRA Commons: The Final RPPR "))
                    {
                        CommonUtilities.ShowDiagnosticIfVerbose($"DON'T WANT THIS {v_SubLine}", verbose);
                    }
                    else
                    {
                        var replySubject = $"category=FRAM: Request, sub=The Final RPPR, extract=1, {currentItem.Subject}";
                        var outmail = ForwardCurrentItem();
                        if (debug == "n")
                        {
                            outmail.AddRecipient(_eFileEmail);
                            outmail.AddRecipient(_eGrantsDevEmail);
                            outmail.AddRecipient(_eGrantsTestEmail);
                            outmail.AddRecipient(_eGrantsStageEmail);
                            outmail.Subject = replySubject;
                            Send(outmail);
                        }
                        else
                        {
                            CommonUtilities.ShowDiagnosticIfVerbose($"DON'T WANT THIS {v_SubLine}", verbose);
                            outmail.AddRecipient(_dBugEmail);
                            outmail.AddRecipient(_eGrantsDevEmail);
                            outmail.Subject = replySubject;
                            Send(outmail);
                        }
                    }
                }
                else if (v_SubLine.Contains("RPPR Unobligated Balance: Additional Information Needed"))
                {
                    CommonUtilities.ShowDiagnosticIfVerbose($"Handlin RPPR for unobligated balance w/ this subject :  {v_SubLine}", verbose);
                    var applid = string.Empty;
                    if (!string.IsNullOrWhiteSpace(v_SubLine))
                    {
                        var grantid = v_SubLine.Trim().Split(' ')[7];
                        CommonUtilities.ShowDiagnosticIfVerbose($"Grant Id step 1 result : {grantid}", verbose);
                        if (!string.IsNullOrWhiteSpace(grantid))
                        {
                            applid = GetApplId(RemoveSpCharacters(grantid), con);
                            CommonUtilities.ShowDiagnosticIfVerbose($"Appl Id step 2 result : {applid}", verbose);
                        }
                    }

                    var replySubject = $"applid={applid}, category=Correspondence, sub=RPPR Unobligated Balance, extract=1, {currentItem.Subject}";
                    var outmail = ForwardCurrentItem();
                    if (debug == "n")
                    {
                        outmail.AddRecipient(_eFileEmail);
                        outmail.AddRecipient(_eGrantsDevEmail);
                        outmail.AddRecipient(_eGrantsTestEmail);
                        outmail.AddRecipient(_eGrantsStageEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }
                    else
                    {
                        CommonUtilities.ShowDiagnosticIfVerbose($"DON'T WANT THIS {v_SubLine}", verbose);
                        outmail.AddRecipient(_dBugEmail);
                        outmail.AddRecipient(_eGrantsDevEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }
                }
                else if (v_SubLine.Contains("eRA Commons: PRAM for Grant"))
                {
                    if (v_SubLine.Contains("re: eRA Commons: PRAM for Grant") || v_SubLine.Contains("fw: eRA Commons: PRAM for Grant"))
                    {
                        CommonUtilities.ShowDiagnosticIfVerbose($"DON'T WANT THIS {v_SubLine}", verbose);
                    }
                    else
                    {
                        var replySubject = $"category=PRAM: Requested, sub=PRAM for Grant, extract=1, {currentItem.Subject}";
                        var outmail = ForwardCurrentItem();
                        if (debug == "n")
                        {
                            outmail.AddRecipient(_eGrantsDevEmail);
                            outmail.AddRecipient(_eGrantsTestEmail);
                            outmail.AddRecipient(_eGrantsStageEmail);
                            outmail.Subject = replySubject;
                            Send(outmail);
                        }
                        else
                        {
                            CommonUtilities.ShowDiagnosticIfVerbose($"DON'T WANT THIS {v_SubLine}", verbose);
                            outmail.AddRecipient(_dBugEmail);
                            outmail.AddRecipient(_eGrantsDevEmail);
                            outmail.Subject = replySubject;
                            Send(outmail);
                        }
                    }
                }
                else if (v_SubLine.Contains("FRAM Requested") || v_SubLine.Contains("PRAM Requested"))
                {
                    var replySubject = string.Empty;
                    var applId = string.Empty;

                    if (v_SubLine.Contains("FRAM Requested"))
                    {
                        if (!string.IsNullOrWhiteSpace(v_SubLine))
                        {
                            applId = GetApplId(RemoveSpCharacters(v_SubLine), con);
                        }
                        replySubject = $"applid={applId}, category=FRAM, sub=Request, extract=1, {currentItem.Subject}";
                    }
                    else if (v_SubLine.Contains("PRAM Requested"))
                    {
                        if (!string.IsNullOrWhiteSpace(v_SubLine))
                        {
                            applId = GetApplId(RemoveSpCharacters(v_SubLine), con);
                        }
                        replySubject = $"applid={applId}, category=PRAM, sub=Request, extract=1, {currentItem.Subject}";
                    }

                    var outmail = ForwardCurrentItem();
                    if (debug == "n")
                    {
                        outmail.AddRecipient(_eFileEmail);
                        outmail.AddRecipient(_eGrantsDevEmail);
                        outmail.AddRecipient(_eGrantsTestEmail);
                        outmail.AddRecipient(_eGrantsStageEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }
                    else
                    {
                        CommonUtilities.ShowDiagnosticIfVerbose($"DON'T WANT THIS {v_SubLine}", verbose);
                        outmail.AddRecipient(_dBugEmail);
                        outmail.AddRecipient(_eGrantsDevEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }

                }
                else if (v_SubLine.Contains("CHANGE_NOTICE_FOR") && v_SubLine.Contains("Application is withdrawn request"))
                {
                    var replySubject = string.Empty;
                    var applId = string.Empty;

                    if (!string.IsNullOrWhiteSpace(v_SubLine))
                    {
                        applId = GetApplId(RemoveSpCharacters(v_SubLine), con);
                    }
                    replySubject = $"applid={applId}, category=eRA Notification, sub=Application Withdrawn, extract=1, {currentItem.Subject}";

                    var outmail = ForwardCurrentItem();
                    if (debug == "n")
                    {
                        outmail.AddRecipient(_eFileEmail);
                        outmail.AddRecipient(_eGrantsDevEmail);
                        outmail.AddRecipient(_eGrantsTestEmail);
                        outmail.AddRecipient(_eGrantsStageEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }
                    else
                    {
                        outmail.AddRecipient(_dBugEmail);
                        outmail.AddRecipient(_eGrantsDevEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }

                }
                else if (v_SubLine.StartsWith("RPPR Reminder"))
                {
                    CommonUtilities.ShowDiagnosticIfVerbose($"Handlin RPPR for Reminder w/ this subject :  {v_SubLine}", verbose);
                    var replySubject = string.Empty;
                    var applId = string.Empty;

                    if (!string.IsNullOrWhiteSpace(v_SubLine))
                    {
                        applId = GetApplId(RemoveSpCharacters(v_SubLine), con);
                    }
                    CommonUtilities.ShowDiagnosticIfVerbose($"applId :  {applId}", verbose);
                    CommonUtilities.ShowDiagnosticIfVerbose($"replySubject :  {replySubject}", verbose);
                    replySubject = $"applid={applId}, category=RPPR, sub=Reminder, extract=1, {currentItem.Subject}";

                    var outmail = ForwardCurrentItem();
                    if (debug == "n")
                    {
                        outmail.AddRecipient(_eFileEmail);
                        outmail.AddRecipient(_eGrantsDevEmail);
                        outmail.AddRecipient(_eGrantsTestEmail);
                        outmail.AddRecipient(_eGrantsStageEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }
                    else
                    {
                        outmail.AddRecipient(_dBugEmail);
                        outmail.AddRecipient(_eGrantsDevEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }
                }
                else if (v_SubLine.Contains("IRPPR Reminder"))
                {
                    var replySubject = string.Empty;
                    var applId = string.Empty;

                    if (!string.IsNullOrWhiteSpace(v_SubLine))
                    {
                        applId = GetApplId(RemoveSpCharacters(v_SubLine), con);
                    }
                    replySubject = $"applid={applId}, category=IRPPR, sub=Reminder, extract=1, {currentItem.Subject}";

                    var outmail = ForwardCurrentItem();
                    if (debug == "n")
                    {
                        outmail.AddRecipient(_eFileEmail);
                        outmail.AddRecipient(_eGrantsDevEmail);
                        outmail.AddRecipient(_eGrantsTestEmail);
                        outmail.AddRecipient(_eGrantsStageEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }
                    else
                    {
                        outmail.AddRecipient(_dBugEmail);
                        outmail.AddRecipient(_eGrantsDevEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }
                }
                else if (v_SubLine.ToLower().Contains("urgent: closeout reports overdue"))
                {
                    CommonUtilities.ShowDiagnosticIfVerbose($"Hello you are closing out a thing ...", verbose);
                    var applId = string.Empty;

                    if (!string.IsNullOrWhiteSpace(v_SubLine))
                    {
                        var isolated = GetNthWord(v_SubLine, 6);
                        CommonUtilities.ShowDiagnosticIfVerbose($"Isolated : {isolated}", verbose);
                        applId = GetApplId(RemoveSpCharacters(isolated), con);
                        CommonUtilities.ShowDiagnosticIfVerbose($"Appl Id : {applId}", verbose);
                    }

                    var replySubject = $"category=closeout, sub=Past Due Documents Reminder, applid={applId}, extract=1, {currentItem.Subject}";

                    var outmail = ForwardCurrentItem();
                    if (debug == "n")
                    {
                        outmail.AddRecipient(_eFileEmail);
                        outmail.AddRecipient(_eGrantsDevEmail);
                        outmail.AddRecipient(_eGrantsTestEmail);
                        outmail.AddRecipient(_eGrantsStageEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }
                    else
                    {
                        CommonUtilities.ShowDiagnosticIfVerbose($"FOUND -> {v_SubLine}", verbose);
                        outmail.AddRecipient(_dBugEmail);
                        outmail.AddRecipient(_eGrantsDevEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }
                    CommonUtilities.ShowDiagnosticIfVerbose($"Hello you closed out a thing", verbose);
                }
                else if (v_SubLine.ToLower().Contains("closeout program action required"))
                {
                    CommonUtilities.ShowDiagnosticIfVerbose($"Hello you are closing out a PROGRAM thing ...", verbose);
                    var applId = string.Empty;

                    if (!string.IsNullOrWhiteSpace(v_SubLine))
                    {
                        var isolated = GetNthWord(v_SubLine.Trim(), 5);
                        CommonUtilities.ShowDiagnosticIfVerbose($"Isolated : {isolated}", verbose);
                        applId = GetApplId(RemoveSpCharacters(isolated), con);
                        CommonUtilities.ShowDiagnosticIfVerbose($"Appl Id : {applId}", verbose);
                    }

                    var replySubject = $"category=closeout, sub=F-RPPR Acceptance Past Due Reminder, applid={applId}, extract=1, {currentItem.Subject}";

                    var outmail = ForwardCurrentItem();
                    if (debug == "n")
                    {
                        outmail.AddRecipient(_eFileEmail);
                        outmail.AddRecipient(_eGrantsDevEmail);
                        outmail.AddRecipient(_eGrantsTestEmail);
                        outmail.AddRecipient(_eGrantsStageEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }
                    else
                    {
                        CommonUtilities.ShowDiagnosticIfVerbose($"FOUND -> {v_SubLine}", verbose);
                        outmail.AddRecipient(_dBugEmail);
                        outmail.AddRecipient(_eGrantsDevEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }
                    CommonUtilities.ShowDiagnosticIfVerbose($"Hello you closed out a PROGRAM thing", verbose);
                }
                else if (v_SubLine.Contains("FFR Reminder") && v_SubLine.Contains("FFR Past Due"))
                {
                    var replySubject = string.Empty;
                    var applId = string.Empty;

                    if (!string.IsNullOrWhiteSpace(v_SubLine))
                    {
                        applId = GetApplId(RemoveSpCharacters(v_SubLine), con);
                    }
                    replySubject = $"applid={applId}, category=FFR, sub=Reminder, extract=1, {currentItem.Subject}";

                    var outmail = ForwardCurrentItem();
                    if (debug == "n")
                    {
                        outmail.AddRecipient(_eFileEmail);
                        outmail.AddRecipient(_eGrantsDevEmail);
                        outmail.AddRecipient(_eGrantsTestEmail);
                        outmail.AddRecipient(_eGrantsStageEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }
                    else
                    {
                        outmail.AddRecipient(_dBugEmail);
                        outmail.AddRecipient(_eGrantsDevEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }
                }
                else if (v_SubLine.Contains("ClinicalTrials.gov Results Reporting for Grant"))
                {
                    var replySubject = string.Empty;
                    var applId = string.Empty;

                    var lastFourCharacters = string.Empty;
                    if (!string.IsNullOrWhiteSpace(v_SubLine))
                    {
                        var lastWordInSubject = GetLastWord(v_SubLine);
                        lastFourCharacters = lastWordInSubject.Substring(Math.Max(0, lastWordInSubject.Length - 4));
                        applId = GetApplId(RemoveSpCharacters(v_SubLine), con);
                    }
                    replySubject = $"applid={applId}, category=CT.gov, sub=Results Reporting Reminder NCT{lastFourCharacters} , extract=1, {currentItem.Subject}";

                    var outmail = ForwardCurrentItem();
                    if (debug == "n")
                    {
                        outmail.AddRecipient(_eFileEmail);
                        outmail.AddRecipient(_eGrantsDevEmail);
                        outmail.AddRecipient(_eGrantsTestEmail);
                        outmail.AddRecipient(_eGrantsStageEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }
                    else
                    {
                        outmail.AddRecipient(_dBugEmail);
                        outmail.AddRecipient(_eGrantsDevEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }
                }
                else if (v_SubLine.Contains("SBIR/STTR Foreign Risk Management"))
                {
                    CommonUtilities.ShowDiagnosticIfVerbose("handling SBIR/STTR", verbose);

                    var replySubject = string.Empty;
                    var applId = string.Empty;
                    var lastFourCharacters = string.Empty;
                    if (!string.IsNullOrWhiteSpace(v_SubLine))
                    {
                        applId = v_Body.Split(' ')[1];   // e.g. (10921643)
                        CommonUtilities.ShowDiagnosticIfVerbose($"SBIR/STTR extraction 1 : {applId}", verbose);
                        applId = applId.Replace("(", "");
                        applId = applId.Replace(")", "");
                        CommonUtilities.ShowDiagnosticIfVerbose($"SBIR/STTR extraction 2 : {applId}", verbose);
                    }
                    replySubject = $"applid={applId}, category=Funding, sub=DCI-InTh Cleared, extract=1, {currentItem.Subject}";
                    if (v_SubLine.Contains("Not Cleared"))
                        replySubject = $"applid={applId}, category=Funding, sub=DCI-InTh Not Cleared, extract=1, {currentItem.Subject}";
                    CommonUtilities.ShowDiagnosticIfVerbose($"replySubject : {replySubject}", verbose);

                    var outmail = ForwardCurrentItem();
                    if (debug == "n")
                    {
                        outmail.AddRecipient(_eFileEmail);
                        outmail.AddRecipient(_eGrantsDevEmail);
                        outmail.AddRecipient(_eGrantsTestEmail);
                        outmail.AddRecipient(_eGrantsStageEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }
                    else
                    {
                        outmail.AddRecipient(_dBugEmail);
                        outmail.AddRecipient(_eGrantsDevEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }
                    CommonUtilities.ShowDiagnosticIfVerbose("completed SBIR", verbose);
                }
                CommonUtilities.ShowDiagnosticIfVerbose("Finished handling the program type", verbose);
            }
            CommonUtilities.ShowDiagnosticIfVerbose("Done checking if it was undeliverable", verbose);
        }

        private static string GetLastWord(string inbound)
        {
            var wordz = inbound.Split(' ');
            var numberOfWords = wordz.Length;
            var lastWord = wordz[numberOfWords - 1];
            return lastWord;
        }

        private static string GetNthWord(string inbound, int number)
        {
            var wordz = inbound.Split(' ');
            var lastWord = wordz[number - 1];
            return lastWord;
        }

        private static string GetApplId(string str, SqlConnection con)
        {
            var queryText = "select dbo.Imm_fn_applid_match(  @LocalId ) as applid";
            try
            {
                using (SqlCommand command = new SqlCommand(queryText, con))
                {
                    command.Parameters.Add(new SqlParameter("@LocalId", str));
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                            {
                                int returnedVal = reader.GetInt32(0);
                                string applId = $"{returnedVal}";
                                return applId;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: Imm_fn_applid_match returned NULL for input '{str}'");
                                return string.Empty;
                            }
                        }
                    }
                }
                return string.Empty;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Query failed.");
                Console.WriteLine($"The string parameter for Imm_fn_applid_match was '{str}'");
                Console.WriteLine($"The query text (without inferred params) : '{queryText}'");
                throw new System.Exception($"Get Appl Id query failed. Input string : '{str}'\r\n Message: {ex.Message}");
            }
        }

        private static string RemoveSpCharacters(string text)
        {
            var result = text;
            result = result.Replace("vbLf", "vbCrLF");
            result = result.Replace(":", " ");
            result = result.Replace("/", " ");
            result = result.Replace("\\", " ");
            result = result.Replace("&", "and");
            result = result.Replace(";", " ");
            result = result.Replace("<", " ");
            result = result.Replace(">", " ");
            result = result.Replace("<<", " ");
            result = result.Replace(">>", " ");
            result = result.Replace("^", " ");
            result = result.Replace("%", " ");
            result = result.Replace("@", " ");
            result = result.Replace("'", " ");
            result = result.Replace(" ", "");
            return result;
        }

        /// <summary>
        /// Gets the sender identifier from a RouterMailItem.
        /// For IMAP, the sender address is already available as a property.
        /// Extracts the alias (part before @) to match legacy Outlook behavior.
        /// </summary>
        public virtual string GetSenderId(RouterMailItem currentItem)
        {
            if (currentItem == null)
                return string.Empty;

            string senderAddress = currentItem.SenderAddress;
            if (string.IsNullOrWhiteSpace(senderAddress))
                return string.Empty;

            // Extract the alias (part before @) to match legacy Exchange alias behavior
            int atIndex = senderAddress.IndexOf('@');
            if (atIndex > 0)
                return senderAddress.Substring(0, atIndex);

            return senderAddress;
        }
    }
}
