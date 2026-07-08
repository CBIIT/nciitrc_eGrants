using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Xml;
using CommonUtilties;

namespace LoadSuppPfr
{
    /// <summary>
    /// Processor for Loading Supplement Progress/Final Reports (Supplement PFR)
    /// 
    /// RESPONSIBILITY:
    /// Processes XML metadata files containing supplement PFR information and loads the
    /// associated PDF documents into the eGrants document management system by calling
    /// the getPlaceHolder_new stored procedure and managing file operations.
    /// 
    /// PROCESSING LOGIC:
    /// 1. Scans source directory for XML metadata files
    /// 2. For each XML file:
    ///    - Parses document metadata (applid, folderid, filename, date, file type)
    ///    - Sets catname="PFR" when folderid="19"
    ///    - Calls getPlaceHolder_new stored procedure to get a file number
    ///    - Copies the PDF to the final destination with the assigned file number name
    ///    - Moves both XML and PDF to the backup directory
    /// 3. Logs all operations and errors
    /// 4. Sends email notifications for errors
    /// 
    /// XML STRUCTURE EXPECTED:
    /// &lt;root&gt;
    ///   &lt;record&gt;
    ///     &lt;APPLID&gt;12345&lt;/APPLID&gt;
    ///     &lt;FOLDERID&gt;19&lt;/FOLDERID&gt;
    ///     &lt;FILENAME&gt;report.pdf&lt;/FILENAME&gt;
    ///     &lt;DATE&gt;1/15/2024&lt;/DATE&gt;
    ///     &lt;FILE_TYPE&gt;pdf&lt;/FILE_TYPE&gt;
    ///   &lt;/record&gt;
    /// &lt;/root&gt;
    /// 
    /// STORED PROCEDURE: getPlaceHolder_new
    /// Parameters (VBScript indexing 1-8):
    /// @param1 = applid
    /// @param2 = " " (single space)
    /// @param3 = document date
    /// @param4 = catname (e.g., "PFR")
    /// @param5 = file type (e.g., "pdf")
    /// @param6 = " " (single space)
    /// @param7 = " " (single space)
    /// @param8 = " " (single space)
    /// 
    /// FILE FLOW:
    /// Source Directory (XML + PDF) → Process → Final Directory (renamed PDF) + Backup Directory (original files)
    /// 
    /// ERROR HANDLING:
    /// Individual file processing errors are logged but do not stop the batch.
    /// The processor continues with the next file if one fails.
    /// 
    /// EMAIL NOTIFICATIONS:
    /// - Errors: When getPlaceHolder_new returns no data (database error)
    /// </summary>
    public class Processor
    {
        private bool _emailEnabled = false;
        private string _toRecipients = "";
        private string _ccRecipients = "";
        private string _environment = "";

        /// <summary>
        /// Main processing method that orchestrates the entire supplement PFR loading workflow.
        /// Opens database connection, finds XML files, processes each one, and returns count of files processed.
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
            return filesProcessed;
        }

        /// <summary>
        /// Processes a single XML metadata file and its associated PDF document.
        /// Parses the XML, extracts metadata (including folderid to determine catname),
        /// calls the getPlaceHolder_new stored procedure, copies the PDF to the final destination,
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
                Program.WriteLog($"XML file has no root element: {xmlFile.Name}", null, DateTime.Now, logDir);
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

                // Extract metadata from XML nodes
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
                    }
                }

                CommonUtilities.ShowDiagnosticIfVerbose($"Creating Supplement PFR for ApplID: {applId}, CatName: {catName}, File: {fileName}", verbose);

                // Validate that the date field is not blank
                if (string.IsNullOrWhiteSpace(docDt))
                {
                    string dateErrorMsg = $"DATE is blank in XML file {xmlFile.Name} for ApplID: {applId}, File: {fileName}";
                    CommonUtilities.ShowDiagnosticIfVerbose(dateErrorMsg, verbose);
                    Program.WriteLog(dateErrorMsg, null, DateTime.Now, logDir);

                    if (_emailEnabled)
                    {
                        try
                        {
                            SendEmail("ERROR=> DATE is blank in XML", dateErrorMsg, verbose, logDir);
                        }
                        catch (Exception emailEx)
                        {
                            Program.WriteLog("Error sending blank date email", emailEx.Message, DateTime.Now, logDir);
                        }
                    }
                    continue;
                }

                // Call the getPlaceHolder_new stored procedure to get a file number
                // This procedure registers the supplement PFR document
                // Parameters must match VBScript exactly: applid, " ", date, catname, filetype, " ", " ", " "
                using (var cmd = new SqlCommand("getPlaceHolder_new", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Add parameters for the stored procedure matching VBScript parameter order
                    // VBScript uses Parameters(1) through Parameters(8) (1-indexed)
                    cmd.Parameters.AddWithValue("@PARENTAPPLID", applId);
                    cmd.Parameters.AddWithValue("@pa", " ");  // Single space (param 2)
                    cmd.Parameters.AddWithValue("@Rcvd_dt", DateTime.Parse(docDt));
                    cmd.Parameters.AddWithValue("@Catname", catName);
                    cmd.Parameters.AddWithValue("@filetype", fileType);
                    cmd.Parameters.AddWithValue("@Sub", " ");  // Single space (param 6)
                    cmd.Parameters.AddWithValue("@body", " ");  // Single space (param 7)
                    cmd.Parameters.AddWithValue("@SubCatname", " ");  // Single space (param 8)

                    // Execute stored procedure and get the assigned file number
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Get the file number returned by the stored procedure
                            string fileNumberName = reader["ABC"]?.ToString();
                            if (!string.IsNullOrEmpty(fileNumberName))
                            {
                                string pdfSrc = Path.Combine(docSrcPath, fileName);
                                
                                // Verify the PDF file exists
                                if (File.Exists(pdfSrc))
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
                                    
                                    // Move PDF to backup after successful copy
                                    try
                                    {
                                        Directory.CreateDirectory(bakDstPath);
                                        string pdfBackupPath = Path.Combine(bakDstPath, fileName);
                                        File.Move(pdfSrc, pdfBackupPath, true);
                                        CommonUtilities.ShowDiagnosticIfVerbose($"Moved PDF to backup: {pdfBackupPath}", verbose);
                                    }
                                    catch (Exception ex)
                                    {
                                        CommonUtilities.ShowDiagnosticIfVerbose($"Error moving PDF to backup: {ex.Message}", verbose);
                                        Program.WriteLog($"Error moving PDF to backup for {fileName}", ex.Message, DateTime.Now, logDir);
                                    }
                                }
                                else
                                {
                                    CommonUtilities.ShowDiagnosticIfVerbose($"Source PDF file not found: {pdfSrc}", verbose);
                                    Program.WriteLog($"Source PDF file not found: {pdfSrc}", null, DateTime.Now, logDir);
                                }
                            }
                            else
                            {
                                CommonUtilities.ShowDiagnosticIfVerbose("No file number name returned from stored procedure", verbose);
                                Program.WriteLog("No file number name returned from stored procedure", null, DateTime.Now, logDir);
                            }
                        }
                        else
                        {
                            string errorMsg = "Stored procedure returned no results";
                            CommonUtilities.ShowDiagnosticIfVerbose(errorMsg, verbose);
                            Program.WriteLog(errorMsg, null, DateTime.Now, logDir);

                            // Send email notification about database error (matching VBScript behavior)
                            if (_emailEnabled)
                            {
                                try
                                {
                                    SendEmail("ERROR: Could not create entry in WIP. Check DB proc : getPlaceHolder_new", 
                                             "Could not create entry in WIP. Check DB proc : getPlaceHolder_new", verbose, logDir);
                                }
                                catch (Exception emailEx)
                                {
                                    Program.WriteLog("Error sending DB error email", emailEx.Message, DateTime.Now, logDir);
                                }
                            }
                        }
                    }
                }

                // Move XML file to backup directory
                try
                {
                    Directory.CreateDirectory(bakDstPath);
                    string xmlBackupPath = Path.Combine(bakDstPath, xmlFile.Name);
                    File.Move(xmlFile.FullName, xmlBackupPath, true);
                    CommonUtilities.ShowDiagnosticIfVerbose($"Moved XML to backup: {xmlBackupPath}", verbose);
                    Program.WriteLog($"Moved XML to backup: {xmlFile.Name}", null, DateTime.Now, logDir);
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the entire process
                    CommonUtilities.ShowDiagnosticIfVerbose($"Error moving XML to backup: {ex.Message}", verbose);
                    Program.WriteLog($"Error moving XML to backup for {xmlFile.Name}", ex.Message, DateTime.Now, logDir);
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
                mailItem.Subject = GetEnvironmentPrefix() + $"{_environment}: {subject}";
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
