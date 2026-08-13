using System;
using System.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using CommonUtilties;

namespace AddSuppProd
{
    /// <summary>
    /// Processor class for Administrative Supplement Production.
    /// Migrated from add_supp_prod.vbs
    /// 
    /// Processes incoming emails from NCIOGASupplements public folder:
    /// - System notifications (nciogaegrantsprod)
    /// - eRA notifications (caeranotifications)  
    /// - Staff manual uploads (driskelleb, jonesni, omairi, woldezf)
    /// - PD/PI replies to supplement notifications
    /// 
    /// Extracts metadata, calls database stored procedures,
    /// saves emails/attachments to disk, and archives processed items.
    /// </summary>
    public class Processor
    {
        private const int MaxItemsPerRun = 30;
        private readonly List<string> AuthorizedStaff = new List<string> { "driskelleb", "jonesni", "omairi", "woldezf" };
        private const int OlTXT = 0;
        private const int OlMailItem = 0;
        private readonly string _adminEmailRecipients;  // Add this field
        private string _serverDstPath;

        // Add this constructor
        public Processor(string adminEmailRecipients)
        {
            _adminEmailRecipients = adminEmailRecipients ?? "leul.ayana@nih.gov;guillermo.choy-leon@nih.gov";
        }

        public int Process(SqlConnection con, string dirPath, string outDir, string serverDstPath, string verbose, string logDir)
        {
            int itemsProcessed = 0;
            _serverDstPath = serverDstPath;

            CommonUtilities.Logger?.Information("Starting supplement production processing");
            CommonUtilities.Logger?.Information("Folder path: {DirPath}", dirPath);
            CommonUtilities.Logger?.Information("Output directory: {OutDir}", outDir);

            // Create Outlook application via late binding
            Type outlookType = Type.GetTypeFromProgID("Outlook.Application");
            if (outlookType == null)
            {
                CommonUtilities.Logger?.Error("Outlook is not installed or not registered");
                throw new InvalidOperationException("Outlook.Application COM class not found");
            }

            dynamic outlookApp = GetRunningOutlook() ?? Activator.CreateInstance(outlookType);
            dynamic outlookNs = outlookApp.GetNamespace("MAPI");
            CommonUtilities.Logger?.Information("Outlook initialized");

            con.Open();
            CommonUtilities.Logger?.Information("Database connection opened");

            dynamic currentFolder = GetCurrentFolder(outlookNs, dirPath);
            dynamic oldFolder = currentFolder.Folders["old"];

            int itemToProcess = currentFolder.Items.Count;
            CommonUtilities.Logger?.Information("Found {ItemCount} items to process", itemToProcess);

            // Process items from last to first (avoid index shifting)
            while (itemToProcess > 0 && itemsProcessed < MaxItemsPerRun)
            {
                try
                {
                    dynamic currentItem = currentFolder.Items[itemToProcess];
                    if (currentItem != null)
                    {
                        string subject = currentItem.Subject ?? "";
                        string senderID = GetSenderID(currentItem);
                        DateTime receivedTime = currentItem.ReceivedTime;

                        CommonUtilities.Logger?.Information("Processing from {Sender}: {Subject}", senderID, subject);

                        // Process based on sender type
                        ProcessEmailBySender(con, outlookApp, currentItem, senderID, subject, receivedTime, outDir, logDir);

                        // Move to archive
                        MoveToOldFolder(currentItem, oldFolder);

                        itemsProcessed++;
                        CommonUtilities.Logger?.Information("Item {Count} processed", itemsProcessed);
                    }
                }
                catch (Exception ex)
                {
                    CommonUtilities.Logger?.Error(ex, "Error processing item {ItemNumber}", itemToProcess);
                    Program.WriteLog($"Error processing item {itemToProcess}", ex.Message, DateTime.Now, logDir);

                    try
                    {
                        dynamic currentItem = currentFolder.Items[itemToProcess];
                        ForwardErrorToAdmin(outlookApp, currentItem, "Error processing email", ex.Message);
                    }
                    catch { }
                }

                itemToProcess--;
            }

            if (itemsProcessed >= MaxItemsPerRun)
            {
                CommonUtilities.Logger?.Warning("Max items per run ({Max}) reached", MaxItemsPerRun);
                SendWarningEmail(outlookApp, "Warning! AddSuppProd processed 30 items", 
                    "30 items processed in one instance. Check for duplicates.");
            }

            con.Close();
            CommonUtilities.Logger?.Information("Complete. {Count} items processed", itemsProcessed);
            return itemsProcessed;
        }

        private void ProcessEmailBySender(SqlConnection con, dynamic outlookApp, dynamic currentItem,
            string senderID, string subject, DateTime receivedTime, string outDir, string logDir)
        {
            string senderLower = senderID.ToLower();

            if (senderLower == "nciogaegrantsprod")
            {
                ProcessSystemNotification(con, currentItem, subject, receivedTime, outDir, outlookApp);
            }
            else if (senderLower == "caeranotifications")
            {
                ProcessEraNotification(con, currentItem, subject, receivedTime, outDir, outlookApp);
            }
            else if (AuthorizedStaff.Contains(senderLower))
            {
                ProcessStaffUpload(con, currentItem, subject, receivedTime, outDir, outlookApp);
            }
            else
            {
                ProcessPossibleReply(con, currentItem, senderID, subject, receivedTime, outDir, outlookApp);
            }
        }

        #region Missing Core Methods

        /// <summary>
        /// Process system notifications from nciogaegrantsprod
        /// Handles: Status Change, Admin Supplement, Response Required, Diversity Supplement
        /// VBScript lines 54-110
        /// </summary>
        private void ProcessSystemNotification(SqlConnection con, dynamic currentItem, string subject,
            DateTime receivedTime, string outDir, dynamic outlookApp)
        {
            CommonUtilities.Logger?.Information("Processing system notification");

            string catname = "Correspondence";
            string subcatname = "Unknown";
            string body = currentItem.Body ?? "";

            // Determine subcategory from subject line (VBScript lines 56-66)
            if (subject.Contains("Change in Status"))
                subcatname = "Supplement Status Change";
            else if (subject.Contains("Admin Supplement"))
                subcatname = "Admin Supplement";
            else if (subject.Contains("Response Required"))
                subcatname = "Supplement Response Required";
            else if (subject.Contains("Diversity Supplement"))
                subcatname = "Diversity Supplement";

            CommonUtilities.Logger?.Debug("Category: {Category}, Subcategory: {Subcategory}", catname, subcatname);

            // Extract notification ID from email body (VBScript line 70)
            string notificationIdStr = ExtractNotificationID(body);
            if (string.IsNullOrWhiteSpace(notificationIdStr))
            {
                CommonUtilities.Logger?.Warning("Could not extract notification ID from system notification");
                ForwardErrorToAdmin(outlookApp, currentItem, 
                    "ERROR: Could not extract notification ID", 
                    "Could not find 'Notification Id=' pattern in email body");
                return;
            }

            // Get application ID from notification (VBScript line 71)
            string applId = GetApplIdFromNotification(con, notificationIdStr);
            if (string.IsNullOrWhiteSpace(applId))
            {
                CommonUtilities.Logger?.Warning("Could not get application ID for notification {NotifId}", notificationIdStr);
                ForwardErrorToAdmin(outlookApp, currentItem, 
                    "ERROR: Could not get application ID", 
                    $"Notification ID: {notificationIdStr}");
                return;
            }

            // Call stored procedure to create placeholder (VBScript lines 74-83)
            string fileNumber = CallGetPlaceHolderNew(con, applId, "", receivedTime, catname, "txt",
                subject, body, subcatname);

            if (string.IsNullOrWhiteSpace(fileNumber))
            {
                // VBScript lines 85-97
                ForwardErrorToAdmin(outlookApp, currentItem,
                    "ERROR: Could not create entry in WIP. Check DB proc: getPlaceHolder_new to load OGA_Notification",
                    $"ApplId: {applId}, Category: {catname}, Subcategory: {subcatname}");
                return;
            }

            // Save email body as text file (VBScript lines 99-106)
            string fileName = $"{fileNumber}.txt";
            SaveEmailAsText(currentItem, outDir, fileName);
            CommonUtilities.Logger?.Information("System notification saved as {FileName}", fileName);
        }

        /// <summary>
        /// Process eRA notifications from caeranotifications
        /// Handles: Supplement Requested notifications
        /// VBScript lines 111-175
        /// </summary>
        private void ProcessEraNotification(SqlConnection con, dynamic currentItem, string subject,
            DateTime receivedTime, string outDir, dynamic outlookApp)
        {
            CommonUtilities.Logger?.Information("Processing eRA notification");

            string catname = "eRA Notification";
            string subcatname = subject.Contains("Supplement Requested") ? "Supplement Requested" : "Unknown";
            string body = currentItem.Body ?? "";

            CommonUtilities.Logger?.Debug("Category: {Category}, Subcategory: {Subcategory}", catname, subcatname);

            // Try to get application ID from subject line (VBScript lines 123-125)
            string applId = GetApplIdFromText(con, RemoveSpecialCharacters(subject));

            // If not found, try email body (VBScript lines 128-130)
            if (string.IsNullOrWhiteSpace(applId))
            {
                applId = GetApplIdFromText(con, RemoveSpecialCharacters(body));
            }

            // If still blank, send to administrator (VBScript lines 133-144)
            if (string.IsNullOrWhiteSpace(applId))
            {
                CommonUtilities.Logger?.Warning("Could not identify application ID in eRA notification");
                ForwardErrorToAdmin(outlookApp, currentItem,
                    "ERROR: Supplement could not identified",
                    $"Subject: {subject}");
                return;
            }

            // Get PA code (VBScript line 146)
            string pa = GetPAFromText(con, RemoveSpecialCharacters(subject));

            // Call stored procedure (VBScript lines 151-160)
            string fileNumber = CallGetPlaceHolderNew(con, applId, pa, receivedTime, catname, "txt",
                subject, body, subcatname);

            if (string.IsNullOrWhiteSpace(fileNumber))
            {
                // VBScript lines 162-171
                ForwardErrorToAdmin(outlookApp, currentItem,
                    "ERROR: Could not create entry in WIP. Check DB proc: getPlaceHolder_new",
                    $"ApplId: {applId}, PA: {pa}");
                return;
            }

            // Save email body as text file (VBScript lines 173-175)
            string fileName = $"{fileNumber}.txt";
            SaveEmailAsText(currentItem, outDir, fileName);
            CommonUtilities.Logger?.Information("eRA notification saved as {FileName}", fileName);
        }

        /// <summary>
        /// Process manual uploads from authorized staff
        /// Handles: correspondence (email body) and application file (attachments)
        /// VBScript lines 176-295
        /// </summary>
        private void ProcessStaffUpload(SqlConnection con, dynamic currentItem, string subject,
            DateTime receivedTime, string outDir, dynamic outlookApp)
        {
            CommonUtilities.Logger?.Information("Processing staff upload");

            // Parse parameters from subject line (VBScript lines 180-199)
            var parameters = ParseSubjectParameters(subject);
            string category = parameters.ContainsKey("category") ? parameters["category"] : "correspondence";
            string subcategory = parameters.ContainsKey("sub") ? parameters["sub"] : "";
            string grantNumber = parameters.ContainsKey("grantnumber") ? parameters["grantnumber"] : "";
            string applId = parameters.ContainsKey("applid") ? parameters["applid"] : "";
            string body = currentItem.Body ?? "";

            dynamic attachments = currentItem.Attachments;
            int attachmentCount = attachments.Count;

            CommonUtilities.Logger?.Debug("Parsed - Category: {Cat}, Subcategory: {Sub}, GrantNumber: {Grant}, ApplId: {ApplId}, Attachments: {Count}",
                category, subcategory, grantNumber, applId, attachmentCount);

            // Determine file type (VBScript lines 200-209)
            string fileType = "txt";
            if (category.ToLower() == "application file" || category.ToLower() == "applicationfile")
            {
                if (attachmentCount > 0)
                {
                    string attachmentName = attachments[1].FileName;
                    fileType = GetFileExtension(attachmentName);
                    subcategory = ""; // No subcategory for application files
                    CommonUtilities.Logger?.Debug("Application file detected: {FileName}, Type: {FileType}", 
                        attachmentName, fileType);
                }
            }

            // Get application ID if not provided (VBScript lines 212-222)
            if (string.IsNullOrWhiteSpace(applId))
            {
                if (!string.IsNullOrWhiteSpace(grantNumber))
                {
                    applId = GetApplIdFromText(con, RemoveSpecialCharacters(grantNumber));
                }
                else
                {
                    applId = GetApplIdFromText(con, RemoveSpecialCharacters(subject));
                }

                if (string.IsNullOrWhiteSpace(applId))
                {
                    applId = GetApplIdFromText(con, RemoveSpecialCharacters(body));
                }
            }

            // If still blank, send to administrator (VBScript lines 225-235)
            if (string.IsNullOrWhiteSpace(applId))
            {
                ForwardErrorToAdmin(outlookApp, currentItem,
                    "ERROR: GRANT NUMBER OR APPL_ID COULD NOT BE IDENTIFIED EITHER IN SUBJECT OR EMAIL BODY",
                    "Grant number or application ID must be in subject line or email body");
                return;
            }

            // Validate subcategory for correspondence (VBScript lines 236-247)
            if (category.ToLower() == "correspondence" && string.IsNullOrWhiteSpace(subcategory))
            {
                string replyText = "Two parameters are important: 1) category 2) grantnumber. " +
                    "If category=correspondence, you must add third parameter called sub=<<subcategoryname>>. " +
                    "Example: category=correspondence,sub=admin supplement,grantnumber=1R01CA123456-01\n" +
                    "If category=application file, do not add third parameter sub=<<>>. " +
                    "Example: category=application file,grantnumber=1R01CA123456-01";

                ForwardErrorToAdmin(outlookApp, currentItem, "INVALID SUBJECT LINE", replyText);
                return;
            }

            // Call stored procedure (VBScript lines 249-260)
            // NOTE: Staff uploads pass EMPTY strings for subject and body parameters
            string fileNumber = CallGetPlaceHolderNew(con, applId, "", receivedTime, category, fileType,
                "", "", subcategory);

            if (string.IsNullOrWhiteSpace(fileNumber))
            {
                // VBScript lines 262-272
                string replyBody = $"appl_id={applId} Rec Time={receivedTime} category={category} " +
                                  $"Notification Type={fileType} subcat={subcategory}";
                ForwardErrorToAdmin(outlookApp, currentItem,
                    "ERROR: Could not create entry in WIP. Check DB proc: getPlaceHolder_new",
                    replyBody);
                return;
            }

            // Save based on category and attachments (VBScript lines 273-282)
            if (category.ToLower() == "correspondence" && attachmentCount == 0 && !string.IsNullOrWhiteSpace(body))
            {
                string fileName = $"{fileNumber}.txt";
                SaveEmailAsText(currentItem, outDir, fileName);
                CommonUtilities.Logger?.Information("Staff correspondence saved as {FileName}", fileName);
            }
            else if ((category.ToLower() == "application file" || category.ToLower() == "applicationfile") && attachmentCount > 0)
            {
                string fileName = $"{fileNumber}.{fileType}";
                SaveAttachment(attachments[1], outDir, fileName);
                CommonUtilities.Logger?.Information("Staff attachment saved as {FileName}", fileName);
            }
        }

        /// <summary>
        /// Process possible reply from PD/PI
        /// Handles: Replies to supplement notifications
        /// VBScript lines 296-347
        /// </summary>
        private void ProcessPossibleReply(SqlConnection con, dynamic currentItem, string senderID,
            string subject, DateTime receivedTime, string outDir, dynamic outlookApp)
        {
            string body = currentItem.Body ?? "";

            // Extract notification ID from email body (VBScript line 298)
            string notificationId = ExtractNotificationID(body);

            if (string.IsNullOrWhiteSpace(notificationId))
            {
                // Not a reply - unknown sender (VBScript lines 342-351)
                CommonUtilities.Logger?.Warning("Unidentified email from {Sender}", senderID);
                ForwardErrorToAdmin(outlookApp, currentItem,
                    "UN Identified email: NCIOGASupplements public folder",
                    $"From: {senderID}");
                return;
            }

            CommonUtilities.Logger?.Information("Processing reply from {Sender} for notification {NotifId}", 
                senderID, notificationId);

            // This appears to be a reply - update database (VBScript line 301)
            UpdateReplyReceived(con, notificationId, senderID);

            // Get PA code (VBScript line 302)
            string pa = GetPAFromText(con, RemoveSpecialCharacters(subject));

            // Get application ID (VBScript line 303)
            string applId = GetApplIdFromNotification(con, notificationId);
            if (string.IsNullOrWhiteSpace(applId))
            {
                ForwardErrorToAdmin(outlookApp, currentItem,
                    "ERROR: Could not get application ID for reply",
                    $"Notification ID: {notificationId}");
                return;
            }

            // Set category and subcategory (VBScript lines 304-306)
            string catname = "Correspondence";
            string subcat = "Supplement Response";

            // Call stored procedure (VBScript lines 308-317)
            string fileNumber = CallGetPlaceHolderNew(con, applId, pa, receivedTime, catname, "txt",
                subject, body, subcat);

            if (string.IsNullOrWhiteSpace(fileNumber))
            {
                // VBScript lines 319-329
                ForwardErrorToAdmin(outlookApp, currentItem,
                    "ERROR: Could not create entry in WIP. Check DB proc: getPlaceHolder_new",
                    $"Notification ID: {notificationId}");
                return;
            }

            // Save email body as text file (VBScript lines 331-338)
            string fileName = $"{fileNumber}.txt";
            SaveEmailAsText(currentItem, outDir, fileName);
            CommonUtilities.Logger?.Information("Reply saved as {FileName}", fileName);
        }

        #endregion

        #region Outlook Helper Methods

        private string GetSenderID(dynamic mailItem)
        {
            try
            {
                string senderEmailType = mailItem.SenderEmailType;

                if (senderEmailType == "EX")
                {
                    dynamic sender = mailItem.Sender;
                    if (sender != null)
                    {
                        dynamic exchUser = sender.GetExchangeUser();
                        if (exchUser != null)
                        {
                            string alias = exchUser.Alias;
                            if (!string.IsNullOrWhiteSpace(alias)) return alias;
                        }
                    }

                    string senderEmail = mailItem.SenderEmailAddress ?? "";
                    int lastEquals = senderEmail.LastIndexOf('=');
                    if (lastEquals >= 0) return senderEmail.Substring(lastEquals + 1);
                }
                else if (senderEmailType == "SMTP")
                {
                    return mailItem.SenderEmailAddress ?? "";
                }
            }
            catch (Exception ex)
            {
                CommonUtilities.Logger?.Debug(ex, "Error getting sender ID");
            }

            return "unknown";
        }

        private dynamic GetCurrentFolder(dynamic ns, string dirPath)
        {
            CommonUtilities.Logger?.Information("Navigating to folder: {Path}", dirPath);

            string[] dirs = dirPath.TrimEnd('\\').Split(new char[] { '\\' });
            dynamic folder = ns.Folders[dirs[0]];
            CommonUtilities.Logger?.Information("Entered root folder: {Folder}", dirs[0]);

            for (int i = 1; i < dirs.Length; i++)
            {
                if (!string.IsNullOrEmpty(dirs[i]))
                {
                    try
                    {
                        folder = folder.Folders[dirs[i]];
                        CommonUtilities.Logger?.Information("Entered subfolder: {Folder}", dirs[i]);
                    }
                    catch (Exception ex)
                    {
                        CommonUtilities.Logger?.Error(ex, "Failed to navigate to subfolder: {Folder}", dirs[i]);
                        throw;
                    }
                }
            }
            return folder;
        }

        private void MoveToOldFolder(dynamic mailItem, dynamic oldFolder)
        {
            try
            {
                mailItem.Move(oldFolder);
                CommonUtilities.Logger?.Debug("Item moved to archive");
            }
            catch (System.Runtime.InteropServices.COMException comEx) when (comEx.HResult == unchecked((int)0x80040119))
            {
                CommonUtilities.Logger?.Warning("Item copied but not deleted (insufficient permissions)");
            }
            catch (Exception ex)
            {
                CommonUtilities.Logger?.Error(ex, "Error moving item");
                throw;
            }
        }

        private void ForwardErrorToAdmin(dynamic outlookApp, dynamic originalItem, string errorSubject, string errorMessage)
        {
            try
            {
                dynamic forwardMail = originalItem.Forward();
                
                foreach (string email in _adminEmailRecipients.Split(';'))
                {
                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        forwardMail.Recipients.Add(email.Trim());
                    }
                }
                
                forwardMail.Subject = GetEnvironmentPrefix() + errorSubject;
                forwardMail.Body = errorMessage + "\n\n" + (originalItem.Body ?? "");
                forwardMail.Send();

                CommonUtilities.Logger?.Information("Error notification sent: {Subject}", errorSubject);
            }
            catch (Exception ex)
            {
                CommonUtilities.Logger?.Error(ex, "Failed to forward error email");
            }
        }

        private void SendWarningEmail(dynamic outlookApp, string subject, string body)
        {
            try
            {
                dynamic mail = outlookApp.CreateItem(OlMailItem);
                mail.To = _adminEmailRecipients;
                mail.Subject = GetEnvironmentPrefix() + subject;
                mail.BodyFormat = 2;
                mail.HTMLBody = body;
                mail.Send();

                CommonUtilities.Logger?.Information("Warning email sent: {Subject}", subject);
            }
            catch (Exception ex)
            {
                CommonUtilities.Logger?.Error(ex, "Failed to send warning email");
            }
        }

        private static dynamic GetRunningOutlook()
        {
            try
            {
                var clsid = new Guid("0006F03A-0000-0000-C000-000000000046");
                GetActiveObject(clsid, IntPtr.Zero, out object obj);
                return obj;
            }
            catch
            {
                return null;
            }
        }

        [System.Runtime.InteropServices.DllImport("oleaut32.dll", PreserveSig = false)]
        private static extern void GetActiveObject(
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPStruct)] Guid clsid,
            IntPtr reserved,
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.IUnknown)] out object obj);

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

        #endregion

        #region Text Processing Helper Methods

        /// <summary>
        /// Extracts notification ID from email body
        /// VBScript equivalent: ExtractNotificationIDElement
        /// </summary>
        public string ExtractNotificationID(string body)
        {
            try
            {
                var match = Regex.Match(body, @"Notification Id=(\d+)", RegexOptions.IgnoreCase);
                if (match.Success && match.Groups.Count > 1)
                {
                    string notifId = match.Groups[1].Value;
                    CommonUtilities.Logger?.Debug("Extracted notification ID: {NotifId}", notifId);
                    return notifId;
                }
            }
            catch (Exception ex)
            {
                CommonUtilities.Logger?.Debug(ex, "Error extracting notification ID");
            }
            return "";
        }

        /// <summary>
        /// Removes special characters for pattern matching
        /// VBScript equivalent: removespcharacters
        /// </summary>
        public string RemoveSpecialCharacters(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";

            text = text.Replace(":", " ")
                       .Replace("/", " ")
                       .Replace("\\", " ")
                       .Replace("&", "and")
                       .Replace(";", " ")
                       .Replace("<", " ")
                       .Replace(">", " ")
                       .Replace("^", " ")
                       .Replace("%", " ")
                       .Replace("@", " ")
                       .Replace("'", " ")
                       .Replace(" ", "");

            return text.Trim();
        }

        /// <summary>
        /// Parses comma-separated parameters from subject line
        /// Example: "category=correspondence,sub=admin supplement,grantnumber=1R01CA123456-01"
        /// </summary>
        public Dictionary<string, string> ParseSubjectParameters(string subject)
        {
            var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(subject))
                return parameters;

            string[] parts = subject.Split(',');
            foreach (string part in parts)
            {
                int equalsIndex = part.IndexOf('=');
                if (equalsIndex > 0)
                {
                    string key = part.Substring(0, equalsIndex).Trim().ToLower();
                    string value = part.Substring(equalsIndex + 1).Trim();
                    parameters[key] = value;
                    CommonUtilities.Logger?.Debug("Parsed parameter: {Key}={Value}", key, value);
                }
            }

            return parameters;
        }

        /// <summary>
        /// Gets file extension from filename
        /// VBScript equivalent: getFileType
        /// </summary>
        public string GetFileExtension(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return "txt";

            int lastDot = fileName.LastIndexOf('.');
            if (lastDot >= 0 && lastDot < fileName.Length - 1)
            {
                return fileName.Substring(lastDot + 1).ToLower();
            }

            return "txt";
        }

        #endregion

        #region Database Helper Methods

        /// <summary>
        /// Calls the getPlaceHolder_new stored procedure to create a WIP entry
        /// Returns the file number for saving the email/attachment
        /// </summary>
        private string CallGetPlaceHolderNew(SqlConnection con, string applId, string pa, DateTime receivedTime,
            string category, string fileType, string subject, string body, string subcategory)
        {
            try
            {
                using (var cmd = new SqlCommand("getPlaceHolder_new", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PARENTAPPLID", applId);
                    cmd.Parameters.AddWithValue("@pa", pa ?? "");
                    cmd.Parameters.AddWithValue("@Rcvd_dt", receivedTime);
                    cmd.Parameters.AddWithValue("@Catname", category);
                    cmd.Parameters.AddWithValue("@filetype", fileType);
                    cmd.Parameters.AddWithValue("@Sub", subject ?? "");
                    cmd.Parameters.AddWithValue("@body", body ?? "");
                    cmd.Parameters.AddWithValue("@SubCatname", subcategory ?? "");

                    var result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        string fileNumber = result.ToString();
                        CommonUtilities.Logger?.Information("Created WIP entry with file number: {FileNumber}", fileNumber);
                        return fileNumber;
                    }
                }
            }
            catch (Exception ex)
            {
                CommonUtilities.Logger?.Error(ex, "Error calling getPlaceHolder_new stored procedure");
            }

            return null;
        }

        /// <summary>
        /// Gets application ID from notification ID
        /// VBScript equivalent: getTempApplid
        /// </summary>
        private string GetApplIdFromNotification(SqlConnection con, string notificationId)
        {
            try
            {
                string sql = "SELECT appl_id FROM adsup_notification WHERE id = @NotifId";
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@NotifId", notificationId);
                    var result = cmd.ExecuteScalar();
                    string applId = result?.ToString() ?? "";
                    
                    if (!string.IsNullOrWhiteSpace(applId))
                        CommonUtilities.Logger?.Debug("Found appl_id {ApplId} for notification {NotifId}", applId, notificationId);
                    
                    return applId;
                }
            }
            catch (Exception ex)
            {
                CommonUtilities.Logger?.Error(ex, "Error getting application ID from notification {NotifId}", notificationId);
                return "";
            }
        }

        /// <summary>
        /// Gets application ID by matching text against database function
        /// VBScript equivalent: getApplid - calls Imm_fn_applid_match
        /// </summary>
        private string GetApplIdFromText(SqlConnection con, string text)
        {
            try
            {
                string sql = "SELECT dbo.Imm_fn_applid_match(@Text) as applid";
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@Text", " " + text + " ");
                    var result = cmd.ExecuteScalar();
                    string applId = result?.ToString() ?? "";
                    
                    if (!string.IsNullOrWhiteSpace(applId))
                        CommonUtilities.Logger?.Debug("Matched appl_id {ApplId} from text", applId);
                    
                    return applId;
                }
            }
            catch (Exception ex)
            {
                CommonUtilities.Logger?.Error(ex, "Error getting application ID from text");
                return "";
            }
        }

        /// <summary>
        /// Gets PA code by matching text against database function
        /// VBScript equivalent: getpa - calls fn_PA_match
        /// </summary>
        private string GetPAFromText(SqlConnection con, string text)
        {
            try
            {
                string sql = "SELECT dbo.fn_PA_match(@Text) as pa";
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@Text", " " + text + " ");
                    var result = cmd.ExecuteScalar();
                    string pa = result?.ToString() ?? "";
                    
                    if (!string.IsNullOrWhiteSpace(pa))
                        CommonUtilities.Logger?.Debug("Matched PA code: {PA}", pa);
                    
                    return pa;
                }
            }
            catch (Exception ex)
            {
                CommonUtilities.Logger?.Error(ex, "Error getting PA from text");
                return "";
            }
        }

        /// <summary>
        /// Updates the reply received date for a notification
        /// VBScript equivalent: CheckIFreply
        /// </summary>
        private void UpdateReplyReceived(SqlConnection con, string notificationId, string senderID)
        {
            try
            {
                string sql = @"UPDATE dbo.adsup_Notification_email_status 
                              SET reply_recieved_date = GETDATE() 
                              WHERE Notification_id = @NotifId 
                              AND email_address LIKE @SenderPattern";

                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@NotifId", notificationId);
                    cmd.Parameters.AddWithValue("@SenderPattern", senderID + "%");
                    int rowsAffected = cmd.ExecuteNonQuery();
                    
                    CommonUtilities.Logger?.Information("Updated reply received for notification {NotifId} from {Sender} ({Rows} rows)", 
                        notificationId, senderID, rowsAffected);
                }
            }
            catch (Exception ex)
            {
                CommonUtilities.Logger?.Error(ex, "Error updating reply received");
            }
        }

        #endregion

        #region File I/O Methods

        /// <summary>
        /// Saves an email as a text file
        /// VBScript equivalent: CItem.SaveAs OutDir & Alias, olTXT
        /// </summary>
        private void SaveEmailAsText(dynamic mailItem, string outDir, string fileName)
        {
            try
            {
                string fullPath = Path.Combine(outDir, fileName);
                mailItem.SaveAs(fullPath, OlTXT);
                CommonUtilities.Logger?.Information("Email saved to {Path}", fullPath);
                CommonUtilities.MoveFileToServerShare(fullPath, _serverDstPath, "y");
            }
            catch (Exception ex)
            {
                CommonUtilities.Logger?.Error(ex, "Error saving email as text: {FileName}", fileName);
                throw;
            }
        }

        /// <summary>
        /// Saves an email attachment to disk
        /// VBScript equivalent: CAttachments(1).SaveAsFile (OutDir & Alias)
        /// </summary>
        private void SaveAttachment(dynamic attachment, string outDir, string fileName)
        {
            try
            {
                string fullPath = Path.Combine(outDir, fileName);
                attachment.SaveAsFile(fullPath);
                CommonUtilities.Logger?.Information("Attachment saved to {Path}", fullPath);
                CommonUtilities.MoveFileToServerShare(fullPath, _serverDstPath, "y");
            }
            catch (Exception ex)
            {
                CommonUtilities.Logger?.Error(ex, "Error saving attachment: {FileName}", fileName);
                throw;
            }
        }

        #endregion
    }
}
