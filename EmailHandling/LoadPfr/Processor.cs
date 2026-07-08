using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Xml;
using CommonUtilties;

namespace LoadPfr
{
    /// <summary>
    /// Processor for Loading Progress/Final Reports (PFR)
    /// 
    /// RESPONSIBILITY:
    /// Processes XML metadata files containing PFR information and loads the associated
    /// PDF documents into the eGrants document management system by calling the Create_PFR
    /// stored procedure and managing file operations.
    /// 
    /// PROCESSING LOGIC:
    /// 1. Scans source directory for XML metadata files
    /// 2. For each XML file:
    ///    - Parses document metadata (applid, filename, date, file type, creator, folderid)
    ///    - Sets catname="PFR" when folderid="19"
    ///    - Verifies the associated PDF file exists
    ///    - Calls Create_PFR stored procedure to register the document
    ///    - Copies the PDF to the final destination with the assigned file number name
    ///    - Moves both XML and PDF to the backup directory
    /// 3. Logs all operations and errors
    /// 4. Sends email notifications for successful processing and errors
    /// 
    /// XML STRUCTURE EXPECTED:
    /// &lt;root&gt;
    ///   &lt;record&gt;
    ///     &lt;APPLID&gt;12345&lt;/APPLID&gt;
    ///     &lt;FOLDERID&gt;19&lt;/FOLDERID&gt;
    ///     &lt;FILENAME&gt;report.pdf&lt;/FILENAME&gt;
    ///     &lt;DATE&gt;1/15/2024&lt;/DATE&gt;
    ///     &lt;FILE_TYPE&gt;pdf&lt;/FILE_TYPE&gt;
    ///     &lt;UID&gt;username&lt;/UID&gt;
    ///   &lt;/record&gt;
    /// &lt;/root&gt;
    /// 
    /// FILE FLOW:
    /// Source Directory (XML + PDF) → Process → Final Directory (renamed PDF) + Backup Directory (original files)
    /// 
    /// ERROR HANDLING:
    /// Individual file processing errors are logged but do not stop the batch.
    /// The processor continues with the next file if one fails.
    /// 
    /// EMAIL NOTIFICATIONS:
    /// - Success: Lists all processed applids
    /// - Errors: PDF not found, database errors
    /// </summary>
    public class Processor
    {
        private List<string> _processedApplIds = new List<string>();
        private List<(string Subject, string Body)> _pendingErrorEmails = new List<(string, string)>();
        private bool _emailEnabled = false;
        private string _toRecipients = "";
        private string _ccRecipients = "";
        private string _environment = "";

        /// <summary>
        /// Main processing method that orchestrates the entire PFR loading workflow.
        /// Opens database connection, finds XML files, processes each one, sends email notification, and returns count of files processed.
        /// </summary>
        /// <param name="con">SQL connection to the EIM database</param>
        /// <param name="docSrcPath">Source directory containing XML metadata and PDF files</param>
        /// <param name="bakDstPath">Backup directory where processed files are archived</param>
        /// <param name="finalDstPath">Final destination directory where PDFs are copied with new names</param>
        /// <param name="verbose">Verbose mode flag ("y" for detailed console output, "n" for minimal output)</param>
        /// <param name="logDir">Directory where log files are written</param>
        /// <param name="config">Configuration containing email settings</param>
        /// <returns>Number of XML files successfully processed</returns>
        public int Process(SqlConnection con, string docSrcPath, string bakDstPath, string finalDstPath, string serverDstPath, string verbose, string logDir, Microsoft.Extensions.Configuration.IConfiguration config)
        {
            int filesProcessed = 0;

            // Load email settings from configuration
            _emailEnabled = config["EmailSettings:Enabled"]?.ToLower() == "true";
            _toRecipients = config["EmailSettings:ToRecipients"] ?? "";
            _ccRecipients = config["EmailSettings:CcRecipients"] ?? "";
            _environment = config["EmailSettings:Environment"] ?? "DEV";
            _processedApplIds.Clear();
            _pendingErrorEmails.Clear();

            // Open database connection for processing
            con.Open();
            CommonUtilities.ShowDiagnosticIfVerbose("Database connection opened", verbose);

            // Validate source directory exists before attempting to process
            if (!Directory.Exists(docSrcPath))
            {
                CommonUtilities.ShowDiagnosticIfVerbose($"Source directory does not exist: {docSrcPath}", verbose);
                Program.WriteLog($"Source directory does not exist: {docSrcPath}", null, DateTime.Now, logDir);
                return 0;
            }

            // Get all XML metadata files in the source directory
            var xmlFiles = new DirectoryInfo(docSrcPath).GetFiles("*.xml");
            CommonUtilities.ShowDiagnosticIfVerbose($"Found {xmlFiles.Length} XML files to process", verbose);

            // Process each XML file individually
            foreach (var xmlFile in xmlFiles)
            {
                try
                {
                    CommonUtilities.ShowDiagnosticIfVerbose($"Processing: {xmlFile.Name}", verbose);
                    Program.WriteLog($"Processing: {xmlFile.FullName}", null, DateTime.Now, logDir);

                    // Process the XML file and its associated PDF
                    ProcessXmlFile(con, xmlFile, docSrcPath, bakDstPath, finalDstPath, serverDstPath, verbose, logDir);
                    filesProcessed++;
                }
                catch (Exception ex)
                {
                    // Log error but continue processing remaining files
                    Program.WriteLog($"Error processing: {xmlFile.Name}", ex.Message, DateTime.Now, logDir);
                    CommonUtilities.ShowDiagnosticIfVerbose($"Error processing {xmlFile.Name}: {ex.Message}", verbose);
                }
            }

            con.Close();
            CommonUtilities.ShowDiagnosticIfVerbose($"Total files processed: {filesProcessed}", verbose);

            // Send email notification if any files were processed
            if (_emailEnabled && _processedApplIds.Count > 0)
            {
                try
                {
                    string applList = string.Join("   ", _processedApplIds);
                    SendEmail($"{_environment}=>>PFR Processed", applList, verbose, logDir);
                }
                catch (Exception ex)
                {
                    Program.WriteLog("Error sending email notification", ex.Message, DateTime.Now, logDir);
                    CommonUtilities.ShowDiagnosticIfVerbose($"Error sending email: {ex.Message}", verbose);
                }
            }

            // Send any queued error notification emails after all files have been processed
            if (_emailEnabled && _pendingErrorEmails.Count > 0)
            {
                foreach (var (subject, body) in _pendingErrorEmails)
                {
                    try
                    {
                        SendEmail(subject, body, verbose, logDir);
                    }
                    catch (Exception ex)
                    {
                        Program.WriteLog($"Error sending error notification email: {subject}", ex.Message, DateTime.Now, logDir);
                        CommonUtilities.ShowDiagnosticIfVerbose($"Error sending error email: {ex.Message}", verbose);
                    }
                }
            }

            return filesProcessed;
        }

        /// <summary>
        /// Processes a single XML metadata file and its associated PDF document.
        /// Parses the XML, extracts metadata (including folderid to determine catname),
        /// calls the database stored procedure, copies the PDF to the final destination,
        /// and archives both files to backup.
        /// </summary>
        /// <param name="con">Open SQL connection to the EIM database</param>
        /// <param name="xmlFile">XML metadata file to process</param>
        /// <param name="docSrcPath">Source directory containing the PDF files</param>
        /// <param name="bakDstPath">Backup directory for archiving processed files</param>
        /// <param name="finalDstPath">Final destination for renamed PDF files</param>
        /// <param name="verbose">Verbose mode flag for diagnostic output</param>
        /// <param name="logDir">Directory for log files</param>
        private void ProcessXmlFile(SqlConnection con, FileInfo xmlFile, string docSrcPath, string bakDstPath, string finalDstPath, string serverDstPath, string verbose, string logDir)
        {
            // Load and parse the XML metadata file
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlFile.FullName);

            var documentElement = xmlDoc.DocumentElement;
            if (documentElement == null)
            {
                CommonUtilities.ShowDiagnosticIfVerbose($"XML file has no root element: {xmlFile.Name}", verbose);
                return;
            }

            // Process each record in the XML file (may contain multiple documents)
            foreach (XmlNode listNode in documentElement.ChildNodes)
            {
                // Initialize variables to store parsed metadata
                string applId = "";
                string catName = "";
                string fileName = "";
                string docDt = "";
                string fileType = "";
                string createdBy = "";

                // Extract metadata from XML nodes
                // Node names use BaseName property for case-insensitive comparison
                // Special logic: when folderid="19", set catname="PFR"
                foreach (XmlNode fieldNode in listNode.ChildNodes)
                {
                    string baseName = fieldNode.Name.ToLower();
                    string nodeValue = fieldNode.InnerText;

                    CommonUtilities.ShowDiagnosticIfVerbose($"      fieldNode = {fieldNode.Name}, Value = {nodeValue}", verbose);

                    switch (baseName)
                    {
                        case "applid":
                            applId = nodeValue;
                            break;
                        case "folderid":
                            // Special handling: folderid=19 means this is a PFR document
                            if (nodeValue == "19")
                            {
                                catName = "PFR";
                            }
                            break;
                        case "filename":
                            fileName = nodeValue;
                            break;
                        case "date":
                            docDt = nodeValue;
                            break;
                        case "file_type":
                            fileType = nodeValue;
                            break;
                        case "uid":
                            createdBy = nodeValue;
                            break;
                    }
                }

                // Verify the PDF file exists in the source directory
                string pdfSrc = Path.Combine(docSrcPath, fileName);
                if (!File.Exists(pdfSrc))
                {
                    string errorMsg = $"PDF NOT FOUND: {pdfSrc}";
                    CommonUtilities.ShowDiagnosticIfVerbose($"Source file not found: {pdfSrc}", verbose);
                    Program.WriteLog($"Source file not found: {pdfSrc}", null, DateTime.Now, logDir);

                    // Send email notification about missing PDF
                    if (_emailEnabled)
                    {
                        _pendingErrorEmails.Add(("ERROR=> PDF NOT FOUND", $"PDF SOURCE={pdfSrc}"));
                    }
                    continue;
                }

                CommonUtilities.ShowDiagnosticIfVerbose($"Creating PFR for ApplID: {applId}, CatName: {catName}, File: {fileName}", verbose);

                // Validate that the date field is not blank
                if (string.IsNullOrWhiteSpace(docDt))
                {
                    string dateErrorMsg = $"DATE is blank in XML file {xmlFile.Name} for ApplID: {applId}, File: {fileName}";
                    CommonUtilities.ShowDiagnosticIfVerbose(dateErrorMsg, verbose);
                    Program.WriteLog(dateErrorMsg, null, DateTime.Now, logDir);

                    if (_emailEnabled)
                    {
                        _pendingErrorEmails.Add(("ERROR=> DATE is blank in XML", dateErrorMsg));
                    }
                    continue;
                }

                // Call the Create_PFR stored procedure to register the document
                // The stored procedure returns a file number name that will be used as the new filename
                using (var cmd = new SqlCommand("Create_PFR", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Add parameters for the stored procedure
                    cmd.Parameters.AddWithValue("@APPLID", applId);
                    cmd.Parameters.AddWithValue("@Rcvd_dt", DateTime.Parse(docDt));
                        cmd.Parameters.AddWithValue("@CreatedBy", createdBy);
                    cmd.Parameters.AddWithValue("@Catname", catName);
                    cmd.Parameters.AddWithValue("@filetype", fileType);

                    // Execute stored procedure and get the assigned file number
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Get the file number name returned by the stored procedure
                            string fileNumberName = reader["ABC"]?.ToString();
                            if (!string.IsNullOrEmpty(fileNumberName))
                            {
                                // Create new filename using the file number and extension
                                string alias = $"{fileNumberName}.{fileType}";
                                string destPath = Path.Combine(finalDstPath, alias);

                                // Ensure destination directory exists
                                Directory.CreateDirectory(finalDstPath);

                                // Copy PDF to final destination with new name
                                File.Copy(pdfSrc, destPath, true);
                                Program.WriteLog($"Copied to: {alias} (CatName: {catName})", null, DateTime.Now, logDir);
                                CommonUtilities.ShowDiagnosticIfVerbose($"Copied to: {destPath}", verbose);

                                // Move PDF to server share (fall back to copy+delete if move fails)
                                CommonUtilities.MoveFileToServerShare(destPath, serverDstPath, verbose);

                                // Track successfully processed applid for email notification
                                _processedApplIds.Add(applId);
                            }
                            else
                            {
                                string errorMsg = "No file number name returned from stored procedure";
                                CommonUtilities.ShowDiagnosticIfVerbose(errorMsg, verbose);
                                Program.WriteLog(errorMsg, null, DateTime.Now, logDir);

                                // Queue email notification about database error
                                if (_emailEnabled)
                                {
                                    _pendingErrorEmails.Add(($"{_environment}=>> ERROR: Could not create PFR in DB using Create_PFR",
                                        $"Could not create PFR in DB using Create_PFR for ApplID: {applId}"));
                                }
                            }
                        }
                        else
                        {
                            string errorMsg = "Stored procedure returned no results";
                            CommonUtilities.ShowDiagnosticIfVerbose(errorMsg, verbose);
                            Program.WriteLog(errorMsg, null, DateTime.Now, logDir);

                            // Queue email notification about database error (no data returned)
                            if (_emailEnabled)
                            {
                                _pendingErrorEmails.Add(($"{_environment}=>> ERROR: Could not create PFR in DB using Create_PFR",
                                    $"Could not create PFR in DB using Create_PFR for ApplID: {applId} - No data returned"));
                            }
                        }
                    }
                }

                // Move processed files to backup directory for archival
                try
                {
                    // Ensure backup directory exists
                    Directory.CreateDirectory(bakDstPath);
                    
                    // Create backup paths for both XML and PDF files
                    string xmlBackupPath = Path.Combine(bakDstPath, xmlFile.Name);
                    string fileBackupPath = Path.Combine(bakDstPath, fileName);
                    
                    // Move both files to backup (overwrite if exists)
                    File.Move(xmlFile.FullName, xmlBackupPath, true);
                    File.Move(pdfSrc, fileBackupPath, true);
                    
                    CommonUtilities.ShowDiagnosticIfVerbose($"Moved files to backup: {bakDstPath}", verbose);
                    Program.WriteLog($"Moved files to backup: XML={xmlFile.Name}, PDF={fileName}", null, DateTime.Now, logDir);
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the entire process
                    CommonUtilities.ShowDiagnosticIfVerbose($"Error moving files to backup: {ex.Message}", verbose);
                    Program.WriteLog($"Error moving files to backup for {fileName}", ex.Message, DateTime.Now, logDir);
                }
            }
        }

        /// <summary>
        /// Sends an email notification via Outlook COM automation.
        /// Matches the VBScript emailme() function behavior.
        /// </summary>
        /// <param name="subject">Email subject line</param>
        /// <param name="body">Email body content</param>
        /// <param name="verbose">Verbose mode for diagnostic output</param>
        /// <param name="logDir">Log directory for error logging</param>
        private void SendEmail(string subject, string body, string verbose, string logDir)
        {
            try
            {
                CommonUtilities.ShowDiagnosticIfVerbose($"Sending email: {subject}", verbose);

                Type outlookType = Type.GetTypeFromProgID("Outlook.Application");
                if (outlookType == null)
                    throw new InvalidOperationException("Outlook.Application COM class not found. Is Outlook installed?");

                dynamic outlookApp = Activator.CreateInstance(outlookType);
                // CreateItem(0) = olMailItem
                dynamic mailItem = outlookApp.CreateItem(0);

                mailItem.To = _toRecipients;
                mailItem.CC = _ccRecipients;
                mailItem.Subject = GetEnvironmentPrefix() + subject;
                mailItem.BodyFormat = 2; // olFormatHTML
                mailItem.HTMLBody = body;
                mailItem.Send();

                Program.WriteLog($"Email sent: {subject}", null, DateTime.Now, logDir);
                CommonUtilities.ShowDiagnosticIfVerbose("Email sent successfully", verbose);
            }
            catch (Exception ex)
            {
                string errorMsg = $"Failed to send email: {ex.Message}";
                CommonUtilities.ShowDiagnosticIfVerbose(errorMsg, verbose);
                Program.WriteLog("Email send failed", ex.Message, DateTime.Now, logDir);
                throw; // Re-throw to let caller handle
            }
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
    }
}
