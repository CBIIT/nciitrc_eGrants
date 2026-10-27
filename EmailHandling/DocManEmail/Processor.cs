using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using CommonUtilties;

namespace DocManEmail
{
    /// <summary>
    /// Processor class for Document Management Email.
    /// 
    /// PURPOSE:
    /// Processes inbound emails from the eGrants Document Management (DocMan) system.
    /// Users submit documents (PDF attachments) via email with structured subject lines
    /// containing metadata. This processor extracts the metadata, registers the document
    /// in the database, and saves the attachment to the file system.
    /// 
    /// ORIGINAL SOURCE: Migrated from DocMan_email_2008_Prod.vbs
    /// 
    /// WORKFLOW:
    /// 1. Connects to Outlook via late-bound COM automation and opens the DocMan public folder
    /// 2. Iterates through emails in the folder (up to 50 per run to avoid long-running processes)
    /// 3. For each email, parses the subject line to extract document metadata:
    ///    - cpiid: The Competitive Proposal Identification ID (links document to a grant application)
    ///    - docid: An existing document ID (for document replacement/update scenarios)
    ///    - catid: Document category ID (classifies the document type)
    ///    - num: Sequence number (ordering within a category)
    ///    - date: Document date
    /// 4. Calls the SP_CREATE_DOCMAN_DOCUMENT_NEW stored procedure to register the document
    ///    in the EIM database, passing the extracted metadata and sender identity
    /// 5. Saves the first PDF attachment to the output directory using the database-generated
    ///    document ID as the filename (e.g., "12345.pdf")
    /// 6. Moves processed emails to the "old" archive subfolder
    /// 
    /// EMAIL SUBJECT FORMAT:
    /// The subject line is comma-delimited with key=value pairs:
    ///   "cpiid=123456, catid=5, num=1, date=2024-01-15"
    ///   or for document updates: "docid=789, catid=5, num=1, date=2024-01-15"
    /// 
    /// SENDER IDENTIFICATION:
    /// - For Exchange (EX) senders: resolves the Exchange alias via GetExchangeUser()
    /// - For SMTP senders: uses the raw SMTP email address
    /// - The sender identity is stored as the uploading user in the database
    /// 
    /// PROCESSING LIMITS:
    /// - Maximum 50 items per execution to prevent timeouts in scheduled task scenarios
    /// - Items are re-counted after each move to handle index shifting
    /// 
    /// OUTLOOK INTEGRATION:
    /// Uses late-bound COM automation (dynamic/Activator) to control Outlook.
    /// No Primary Interop Assembly (PIA) or NuGet interop package is required at compile time.
    /// Outlook must be installed and configured on the machine where this runs.
    /// 
    /// DEPENDENCIES:
    /// - Microsoft Outlook (COM Interop) - must be installed and configured
    /// - SQL Server database with SP_CREATE_DOCMAN_DOCUMENT_NEW stored procedure
    /// - File system write access to the output directory for PDF storage
    /// 
    /// SCHEDULED TASK:
    /// Typically run as a Windows Scheduled Task to poll for new document submissions.
    /// </summary>
    public class Processor
    {
        public int Process(SqlConnection con, string dirPath, string outDir, string verbose, string logDir)
        {
            int itemsProcessed = 0;

            // Create Outlook application via late binding (no PIA needed)
            Type outlookType = Type.GetTypeFromProgID("Outlook.Application");
            if (outlookType == null)
                throw new InvalidOperationException("Outlook.Application COM class not found. Is Outlook installed?");
            dynamic outlookApp = Activator.CreateInstance(outlookType);
            dynamic outlookNs = outlookApp.GetNamespace("MAPI");

            con.Open();

            dynamic currentFolder = GetCurrentFolder(outlookNs, dirPath);
            dynamic oldFolder = currentFolder.Folders["old"];
            int itemToProcess = currentFolder.Items.Count;

            while (itemToProcess > 0 && itemsProcessed < 50)
            {
                try
                {
                    dynamic item = currentFolder.Items[itemToProcess];
                    if (item != null)
                    {
                        string senderId = GetSenderId(item);
                        string subject = (string)item.Subject;
                        string cpiid = ExtractValue(ExtractElement(subject, 1), "cpiid");
                        string docid = ExtractValue(ExtractElement(subject, 1), "docid");

                        if (!string.IsNullOrWhiteSpace(cpiid) || !string.IsNullOrWhiteSpace(docid))
                        {
                            ProcessDocument(con, item, subject, cpiid, docid, senderId, outDir, verbose);
                            item.Move(oldFolder);
                            itemsProcessed++;
                            Program.WriteLog($"Processed: {senderId}; {subject}", null, DateTime.Now, logDir);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Program.WriteLog($"Error item {itemToProcess}", ex.Message, DateTime.Now, logDir);
                }
                itemToProcess = currentFolder.Items.Count;
            }
            con.Close();
            return itemsProcessed;
        }

        /// <summary>
        /// Registers a document in the EIM database and saves its PDF attachment to disk.
        /// Calls SP_CREATE_DOCMAN_DOCUMENT_NEW which returns the new document ID,
        /// then saves the first attachment using that ID as the filename.
        /// ActionID 1 = new document (has cpiid), ActionID 2 = update (has docid).
        /// </summary>
        private void ProcessDocument(SqlConnection con, dynamic item, string subject, string cpiid, string docid, string senderId, string outDir, string verbose)
        {
            using (var cmd = new SqlCommand("SP_CREATE_DOCMAN_DOCUMENT_NEW", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CP", string.IsNullOrEmpty(cpiid) ? (object)DBNull.Value : cpiid);
                cmd.Parameters.AddWithValue("@CAT", ExtractValue(ExtractElement(subject, 2), "catid") ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@SEQ", ExtractValue(ExtractElement(subject, 3), "num") ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@DD", ExtractValue(ExtractElement(subject, 4), "date") ?? "");
                cmd.Parameters.AddWithValue("@UID", senderId);
                cmd.Parameters.AddWithValue("@FT", "pdf");
                cmd.Parameters.AddWithValue("@ACTIONID", string.IsNullOrEmpty(cpiid) ? "2" : "1");
                cmd.Parameters.AddWithValue("@DOCID", string.IsNullOrEmpty(docid) ? (object)DBNull.Value : docid);
                cmd.Parameters.AddWithValue("@REASON", ExtractValue(ExtractElement(subject, 3), "reason") ?? (object)DBNull.Value);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read() && (int)item.Attachments.Count > 0)
                    {
                        string docId = reader[0].ToString();
                        item.Attachments[1].SaveAsFile(Path.Combine(outDir, $"{docId}.pdf"));
                    }
                }
            }
        }

        /// <summary>
        /// Navigates to an Outlook MAPI folder using a backslash-separated path string.
        /// Splits the path and traverses each subfolder level sequentially.
        /// </summary>
        private dynamic GetCurrentFolder(dynamic ns, string dirPath)
        {
            string[] dirs = dirPath.Split('\\');
            dynamic folder = ns.Folders[dirs[0]];
            for (int i = 1; i < dirs.Length; i++)
                if (!string.IsNullOrEmpty(dirs[i])) folder = folder.Folders[dirs[i]];
            return folder;
        }

        /// <summary>
        /// Resolves the sender's identity from an Outlook mail item.
        /// For Exchange (EX) senders, retrieves the Exchange alias.
        /// For SMTP senders, returns the raw email address.
        /// </summary>
        private string GetSenderId(dynamic item)
        {
            try
            {
                if ((string)item.SenderEmailType == "EX")
                {
                    var exchUser = item.Sender?.GetExchangeUser();
                    if (exchUser != null) return (string)exchUser.Alias;
                }
                return (string)item.SenderEmailType == "SMTP" ? (string)item.SenderEmailAddress : "";
            }
            catch { return ""; }
        }

        /// <summary>
        /// Extracts the nth comma-separated element from a string (1-based index).
        /// Used to parse the structured email subject line.
        /// </summary>
        private string ExtractElement(string str, int n)
        {
            string[] parts = str.Split(',');
            return (n > 0 && n <= parts.Length) ? parts[n - 1].Trim() : "";
        }

        /// <summary>
        /// Extracts a value from a "key=value" string if the key contains the specified name.
        /// Returns null if the format doesn't match or the key name is not found.
        /// </summary>
        private string ExtractValue(string p, string name)
        {
            if (string.IsNullOrEmpty(p)) return null;
            string[] parts = p.Split('=');
            return (parts.Length == 2 && parts[0].Trim().ToLower().Contains(name)) ? parts[1].Trim() : null;
        }
    }
}
