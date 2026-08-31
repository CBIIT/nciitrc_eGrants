using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using CommonUtilties;

namespace ExchangeFixed
{
    /// <summary>
    /// Processor class for Exchange Fixed email routing and document filing.
    /// 
    /// PURPOSE:
    /// Processes emails from a fixed Outlook public folder, parsing structured subject
    /// lines to extract document metadata, then files the email content (body text
    /// and/or attachments) into the eGrants document management system via stored procedures.
    /// 
    /// ORIGINAL SOURCE: Migrated from exchange_Fixed.vbs (exchange_latest.vbs)
    /// 
    /// WORKFLOW:
    /// 1. Connects to Outlook via late-bound COM automation and opens the configured folder
    /// 2. Iterates through all emails in the folder (processes from last to first)
    /// 3. For each email, parses the comma-delimited subject line for key=value metadata:
    ///    - grantnumber: Used to look up the application ID via Imm_fn_applid_match()
    ///    - applid: Direct application ID (takes precedence over grant number lookup)
    ///    - category: Document category (e.g., "Correspondence", "Budget", "PublicAccess")
    ///    - sub: Sub-category for finer classification
    ///    - extract: Determines what content to save (1=text, 2=attachment, 3=both)
    ///    - documentdate: Document date (defaults to email received time if not provided)
    ///    - documentid: Existing document ID for update scenarios
    /// 4. Resolves the application ID:
    ///    - If applid provided directly, use it
    ///    - Else look up from grant number via Imm_fn_applid_match()
    ///    - If no grant number, try matching from the subject line
    ///    - If still not found, try matching from the email body
    /// 5. Special handling for NCIOGAPROGESS sender:
    ///    - Auto-categorizes as "Notification" / "Late Progress Report"
    ///    - Sets extract=1, sends notification email
    /// 6. Calls SP_CREATE_EGRANTS_DOCUMENT_NEW to register the document
    /// 7. Based on category and extract mode, saves content:
    ///    - PublicAccess: Generates PDF via Acrobat SDK (merges subject header + body + attachments)
    ///    - JIT Info, CT.gov, Closeout, eRA Notification/JIT Submitted, Funding/dci-inth:
    ///      Generates PDF with embedded images via Word automation
    ///    - Correspondence (rppr unobligated balance): PDF with embedded images
    ///    - Standard extract=1: Writes email headers + body to .txt file
    ///    - extract=2: Saves all attachments (skipping "ATT*" prefixed files)
    ///    - extract=3: Saves both body text and all attachments
    /// 8. Moves processed emails to the "old" archive subfolder
    /// 9. Logs processing results; sends error notifications to admin on failure
    /// 10. Limits to 30 items per run with admin warning to prevent duplicate processing
    /// 
    /// GRANT NUMBER RESOLUTION:
    /// If no direct applid is provided, the grant number is cleaned of special characters
    /// and passed to the SQL function dbo.Imm_fn_applid_match() which returns the
    /// corresponding application ID from the EIM database.
    /// 
    /// SENDER IDENTIFICATION:
    /// - For Exchange (EX) senders: resolves the Exchange alias via GetExchangeUser()
    /// - If Exchange alias is empty, falls back to extracting alias from the EX address
    /// - For SMTP senders: uses the raw SMTP email address
    /// 
    /// QC (QUALITY CONTROL) FLAGGING:
    /// Files with non-standard extensions (not pdf, txt, doc, xls, docx, xlsx, ppt)
    /// are flagged for QC review (movetoqc="yes"). Also flagged if no applid is found.
    /// 
    /// OUTLOOK INTEGRATION:
    /// Uses late-bound COM automation (dynamic/Activator) to control Outlook.
    /// No Primary Interop Assembly (PIA) or NuGet interop package is required at compile time.
    /// Outlook must be installed and configured on the machine where this runs.
    /// 
    /// DEPENDENCIES:
    /// - Microsoft Outlook (COM Interop) - must be installed and configured
    /// - Microsoft Word (COM Interop) - for GeneratePDFWithEmbeddedImages
    /// - Adobe Acrobat SDK (COM Interop) - for saveMailAsPdf / PDF merging
    /// - SQL Server EIM database with:
    ///   - SP_CREATE_EGRANTS_DOCUMENT_NEW stored procedure
    ///   - SP_CLEAR_OLD_JIT_SUBMISSIONS stored procedure
    ///   - dbo.Imm_fn_applid_match() scalar function
    /// - File system write access to the output directory
    /// </summary>
    public class Processor
    {
        private string _outDir;
        private string _publicAccessBackup;
        private string _adminRecipients;
        private string _verbose;
        private dynamic _outlookApp;

        /// <summary>
        /// Main processing loop. Opens the Outlook folder and processes each email
        /// that has valid metadata in the subject line. Limits to 30 items per run.
        /// </summary>
        public int Process(string dirPath, SqlConnection con, string verbose, string outDir, string publicAccessBackup, string adminRecipients)
        {
            _outDir = outDir;
            _publicAccessBackup = publicAccessBackup;
            _adminRecipients = adminRecipients;
            _verbose = verbose;
            int itemsProcessed = 0;

            // Create Outlook application via late binding (no PIA needed)
            Type outlookType = Type.GetTypeFromProgID("Outlook.Application");
            if (outlookType == null)
                throw new InvalidOperationException("Outlook.Application COM class not found. Is Outlook installed?");
            _outlookApp = Activator.CreateInstance(outlookType);
            dynamic outlookNs = _outlookApp.GetNamespace("MAPI");

            con.Open();

            dynamic folder = GetCurrentFolder(outlookNs, dirPath);
            int totalCount = folder.Items.Count;
            CommonUtilities.ShowDiagnosticIfVerbose($"Total number of mail items to be processed are {totalCount}", verbose);
            int itemToProcess = totalCount;

            while (itemToProcess > 0)
            {
                try
                {
                    dynamic item = folder.Items[itemToProcess];
                    if (item != null)
                    {
                        string subject = (string)item.Subject;
                        var p = ParseSubjectLine(subject);
                        string senderId = GetSenderId(item);

                        // Resolve application ID
                        string applId = ResolveApplId(p, item, subject, con);

                        // Default document date to received time if not provided
                        if (string.IsNullOrWhiteSpace(p.DocumentDate))
                        {
                            DateTime receivedTime = (DateTime)item.ReceivedTime;
                            p.DocumentDate = receivedTime.ToString("M/d/yyyy");
                        }

                        // Special handling for NCIOGAPROGESS sender
                        if (senderId?.Trim() == "FD6862D09E7043D49596358F980D064F-NCI OGA PRO")
                        {
                            senderId = "NCIOGAPROGESS";
                            p.Category = "Notification";
                            p.SubCategory = "Late Progress Report";
                            p.Extract = "1";
                            SendNotificationEmail("Late Progress Report uploaded", applId);
                        }

                        // Determine QC requirement based on applid
                        string moveToQc = string.IsNullOrEmpty(applId) ? "yes" : "no";

                        // Process based on extract mode
                        if (p.Extract == "1")
                        {
                            ProcessExtractBody(con, item, p, applId, senderId, moveToQc, folder);
                        }
                        else if (p.Extract == "2")
                        {
                            ProcessExtractAttachment(con, item, p, applId, senderId, moveToQc, folder);
                        }
                        else if (p.Extract == "3")
                        {
                            ProcessExtractBoth(con, item, p, applId, senderId, moveToQc, folder);
                        }

                        // Release all COM references before moving
                        System.Runtime.InteropServices.Marshal.FinalReleaseComObject(item);
                        
                        // Small delay to ensure file handles are released
                        System.Threading.Thread.Sleep(1000);
                        
                        // NOW move to old folder - refresh the item reference first
                        dynamic freshItem = folder.Items[itemToProcess];
                        MoveToOldFolder(folder, freshItem);

                        CommonUtilities.WriteLog(8,
                            $"Processed! => EmailSender:{senderId}; Subjectline :{subject}; Recieved Date: {freshItem.ReceivedTime}",
                            null, DateTime.Now);

                        itemsProcessed++;
                    }
                }
                catch (Exception ex)
                {
                    string errorMsg = $"Error Number: {ex.HResult}, Error Description: {ex.Message}, Error Source: {ex.Source}";
                    CommonUtilities.WriteLog(8, $"Error Occured! item {itemToProcess}", errorMsg, DateTime.Now);

                    try
                    {
                        dynamic errorItem = folder.Items[itemToProcess];
                        RaiseErrorToAdmin(errorItem, "Error Occured! PROD Exchange_Latest vbs", errorMsg);
                    }
                    catch { }
                }

                // Safety limit: stop after 30 items to prevent duplicate processing
                if (itemsProcessed >= 30)
                {
                    SendAdminWarning(
                        "Warning! PROD Exchange_Latest vbs has processed 30 mail items in one instance!",
                        "Hello Admin, 30 items have been processed in one instance and the application is now exiting. Please check whether there is duplicate items processing.");
                    break;
                }

                itemToProcess = folder.Items.Count;
            }

            CommonUtilities.ShowDiagnosticIfVerbose($"{itemsProcessed} of {totalCount} items has been processed", verbose);
            con.Close();
            return itemsProcessed;
        }

        /// <summary>
        /// Resolves the application ID using the following priority:
        /// 1. Direct applid from subject line
        /// 2. Lookup from grant number via Imm_fn_applid_match()
        /// 3. Lookup from cleaned subject line
        /// 4. Lookup from cleaned email body
        /// </summary>
        private string ResolveApplId(SubjectParams p, dynamic item, string subject, SqlConnection con)
        {
            if (!string.IsNullOrWhiteSpace(p.ApplId))
                return p.ApplId;

            if (!string.IsNullOrWhiteSpace(p.GrantNumber))
                return GetApplId(RemoveSpecialChars(p.GrantNumber), con);

            // Try subject line
            string applId = GetApplId(RemoveSpecialChars(subject), con);
            if (!string.IsNullOrEmpty(applId))
                return applId;

            // Try email body
            return GetApplId(RemoveSpecialChars((string)item.Body), con);
        }

        /// <summary>
        /// Processes extract=1 (body only). Handles special categories that require PDF generation:
        /// PublicAccess, JIT Info, CT.gov, Closeout, eRA Notification/JIT Submitted,
        /// Correspondence/rppr unobligated balance, Funding/dci-inth.
        /// For standard categories, writes email headers and body to a .txt file.
        /// </summary>
        private void ProcessExtractBody(SqlConnection con, dynamic item, SubjectParams p, string applId, string senderId, string moveToQc, dynamic folder)
        {
            string category = p.Category ?? "Correspondence";
            string subcat = p.SubCategory ?? "";
            string fileType;
            string documentId;

            if (category == "PublicAccess")
            {
                CommonUtilities.ShowDiagnosticIfVerbose("handling PublicAccess", _verbose);
                fileType = "pdf";
                documentId = GetDocumentId(con, p.DocumentId, category, applId, "1", p.DocumentDate, senderId, fileType, moveToQc, subcat);
                if (string.IsNullOrEmpty(documentId))
                {
                    RaiseErrorToAdmin(item, "DB Error: Document_id NOT Found, PROD Exchange_Latest vbs /extract=1", "");
                }
                else
                {
                    CommonUtilities.ShowDiagnosticIfVerbose($"Documentid: {documentId}", _verbose);
                    System.Threading.Thread.Sleep(10000);
                    SaveMailAsPdf(item, documentId);
                }
            }
            else if (category == "JIT Info" || category == "CT.gov")
            {
                fileType = "pdf";
                documentId = GetDocumentId(con, p.DocumentId, category, applId, "1", p.DocumentDate, senderId, fileType, moveToQc, subcat);
                GeneratePdfWithEmbeddedImages(item, documentId);
            }
            else if (category == "eRA Notification" && subcat == "JIT Submitted")
            {
                fileType = "pdf";
                documentId = GetDocumentId(con, p.DocumentId, category, applId, "1", p.DocumentDate, senderId, fileType, moveToQc, subcat);
                CommonUtilities.ShowDiagnosticIfVerbose($"Documentid: {documentId}", _verbose);
                ClearOldJitSubmissions(con, documentId);
                CommonUtilities.ShowDiagnosticIfVerbose("Cleared old JIT submitted submissions", _verbose);
                GeneratePdfWithEmbeddedImages(item, documentId);
            }
            else if (category.ToLower() == "closeout" && subcat.ToLower() == "past due documents reminder")
            {
                CommonUtilities.ShowDiagnosticIfVerbose("handling closeout", _verbose);
                fileType = "pdf";
                documentId = GetDocumentId(con, p.DocumentId, category, applId, "1", p.DocumentDate, senderId, fileType, moveToQc, subcat);
                CommonUtilities.ShowDiagnosticIfVerbose($"Documentid: {documentId}", _verbose);
                GeneratePdfWithEmbeddedImages(item, documentId);
            }
            else if (category.ToLower() == "closeout" && subcat.ToLower() == "f-rppr acceptance past due reminder")
            {
                CommonUtilities.ShowDiagnosticIfVerbose("handling program closeout, f-rppr style", _verbose);
                fileType = "pdf";
                documentId = GetDocumentId(con, p.DocumentId, category, applId, "1", p.DocumentDate, senderId, fileType, moveToQc, subcat);
                CommonUtilities.ShowDiagnosticIfVerbose($"Documentid: {documentId}", _verbose);
                GeneratePdfWithEmbeddedImages(item, documentId);
            }
            else if (category.ToLower() == "correspondence" && subcat.ToLower() == "rppr unobligated balance")
            {
                CommonUtilities.ShowDiagnosticIfVerbose("handling obligatory rppr email", _verbose);
                fileType = "pdf";
                documentId = GetDocumentId(con, p.DocumentId, category, applId, "1", p.DocumentDate, senderId, fileType, moveToQc, subcat);
                CommonUtilities.ShowDiagnosticIfVerbose($"Documentid: {documentId}", _verbose);
                GeneratePdfWithEmbeddedImages(item, documentId);
            }
            else if (category == "Funding" && subcat.ToLower().Contains("dci-inth"))
            {
                fileType = "pdf";
                documentId = GetDocumentId(con, p.DocumentId, category, applId, "1", p.DocumentDate, senderId, fileType, moveToQc, subcat);
                GeneratePdfWithEmbeddedImages(item, documentId);
            }
            else
            {
                // Standard text extraction - write email headers and body to .txt file
                fileType = "txt";
                documentId = GetDocumentId(con, p.DocumentId, category, applId, "1", p.DocumentDate, senderId, fileType, moveToQc, subcat);
                if (string.IsNullOrEmpty(documentId))
                {
                    RaiseErrorToAdmin(item, "DB Error: Document_id NOT Found, PROD Exchange_Latest vbs /extract=1", "");
                }
                else
                {
                    string alias = $"{documentId}.txt";
                    string filePath = Path.Combine(_outDir, alias);
                    WriteEmailToTextFile(item, filePath, category, subcat);
                }
            }
        }

        /// <summary>
        /// Processes extract=2 (attachments only). Iterates all attachments,
        /// skipping those with filenames starting with "ATT". Each attachment is
        /// registered separately in the database and saved with the document ID as filename.
        /// </summary>
        private void ProcessExtractAttachment(SqlConnection con, dynamic item, SubjectParams p, string applId, string senderId, string moveToQc, dynamic folder)
        {
            string category = p.Category ?? "Correspondence";
            string subcat = p.SubCategory ?? "";

            int attachCount = (int)item.Attachments.Count;
            if (attachCount > 0)
            {
                for (int i = 1; i <= attachCount; i++)
                {
                    string fileName = RemoveJunk((string)item.Attachments[i].FileName);
                    string fileType = GetFileType(fileName);
                    string qcRequired = IsQcRequired(fileType);

                    // Skip attachments with names starting with "ATT"
                    if (!fileName.StartsWith("ATT", StringComparison.OrdinalIgnoreCase))
                    {
                        string documentId = GetDocumentId(con, p.DocumentId, category, applId, "1", p.DocumentDate, senderId, fileType, qcRequired, subcat);
                        if (string.IsNullOrEmpty(documentId))
                        {
                            RaiseErrorToAdmin(item, "DB Error: Document_id NOT Found, PROD Exchange_Latest vbs /extract=2", "");
                        }
                        else
                        {
                            string alias = $"{documentId}.{fileType}";
                            CommonUtilities.ShowDiagnosticIfVerbose($"OutDir: {_outDir}", _verbose);
                            CommonUtilities.ShowDiagnosticIfVerbose($"Alias: {alias}", _verbose);
                            item.Attachments[i].SaveAsFile(Path.Combine(_outDir, alias));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Processes extract=3 (body and attachments). First saves the email body
        /// (as PDF for PublicAccess, or as .txt with headers for standard categories),
        /// then processes all attachments individually.
        /// </summary>
        private void ProcessExtractBoth(SqlConnection con, dynamic item, SubjectParams p, string applId, string senderId, string moveToQc, dynamic folder)
        {
            string category = p.Category ?? "Correspondence";
            string subcat = p.SubCategory ?? "";

            if (category == "PublicAccess")
            {
                CommonUtilities.ShowDiagnosticIfVerbose("handling public access", _verbose);
                string fileType = "pdf";
                string documentId = GetDocumentId(con, p.DocumentId, category, applId, "1", p.DocumentDate, senderId, fileType, moveToQc, subcat);
                if (string.IsNullOrEmpty(documentId))
                {
                    RaiseErrorToAdmin(item, "DB Error: Document_id NOT Found, PROD Exchange_Latest vbs /extract=1", "");
                }
                else
                {
                    SaveMailAsPdf(item, documentId);
                }
            }
            else
            {
                // Extract body as text
                string fileType = "txt";
                string documentId = GetDocumentId(con, p.DocumentId, category, applId, "1", p.DocumentDate, senderId, fileType, moveToQc, subcat);
                if (string.IsNullOrEmpty(documentId))
                {
                    RaiseErrorToAdmin(item, "DB Error: Document_id NOT Found, PROD Exchange_Latest vbs /extract=3", "");
                }
                else
                {
                    string alias = $"{documentId}.txt";
                    string filePath = Path.Combine(_outDir, alias);
                    WriteEmailToTextFile(item, filePath, category, subcat);
                }

                // Extract all attachments
                int attachCount = (int)item.Attachments.Count;
                if (attachCount > 0)
                {
                    for (int i = 1; i <= attachCount; i++)
                    {
                        string fileName = RemoveJunk((string)item.Attachments[i].FileName);
                        string attFileType = GetFileType(fileName);
                        string qcRequired = IsQcRequired(attFileType);

                        // Skip attachments with names starting with "ATT"
                        if (!fileName.StartsWith("ATT", StringComparison.OrdinalIgnoreCase))
                        {
                            string attDocId = GetDocumentId(con, p.DocumentId, category, applId, "1", p.DocumentDate, senderId, attFileType, qcRequired, subcat);
                            if (string.IsNullOrEmpty(attDocId))
                            {
                                RaiseErrorToAdmin(item, "DB Error: Document_id NOT Found, PROD Exchange_Latest vbs /extract=3", "");
                            }
                            else
                            {
                                string attAlias = $"{attDocId}.{attFileType}";
                                item.Attachments[i].SaveAsFile(Path.Combine(_outDir, attAlias));
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Writes email metadata (From, Sent, To, Subject, Attachments) and body to a text file.
        /// Matches the VBS behavior of writing structured headers before the body content.
        /// </summary>
        private void WriteEmailToTextFile(dynamic item, string filePath, string category, string subcat)
        {
            using (var writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine($"Category: {category}");
                writer.WriteLine($"subcat: {subcat}");
                writer.WriteLine($"From: {item.SenderName}");
                writer.WriteLine($"Sent: {item.ReceivedTime}");
                writer.WriteLine($"To: {item.To}");
                writer.WriteLine($"Subject: {item.Subject}");

                int attachCount = (int)item.Attachments.Count;
                string attachList = "";
                if (attachCount > 0)
                {
                    for (int i = 1; i <= attachCount; i++)
                    {
                        if (!string.IsNullOrEmpty(attachList)) attachList += ", ";
                        attachList += (string)item.Attachments[i].FileName;
                    }
                }
                writer.WriteLine($"Attachments: {attachList}");
                writer.WriteLine((string)item.Body);
            }
        }

        /// <summary>
        /// Generates a PDF from the email using Word automation to preserve embedded images.
        /// Saves the email as .doc format (olDoc=4), opens in Word, sets narrow margins,
        /// and exports as PDF. Used for JIT Info, CT.gov, Closeout, and similar categories.
        /// </summary>
        private void GeneratePdfWithEmbeddedImages(dynamic item, string documentId)
        {
            if (string.IsNullOrEmpty(documentId)) return;

            string strFile = Path.Combine(_outDir, documentId);
            string docName = $"{strFile}.doc";
            string pdfName = $"{strFile}.pdf";

            // Save email as Word document (olDoc = 4)
            item.SaveAs(docName, 4);

            // Open in Word and export as PDF
            dynamic wordApp = null;
            try
            {
                Type wordType = Type.GetTypeFromProgID("Word.Application");
                wordApp = Activator.CreateInstance(wordType);
                wordApp.Visible = false;
                wordApp.Documents.Open(docName);
                wordApp.Documents[docName].Activate();

                // Set narrow margins (18 points = 0.25 inches)
                const int margin = 18;
                dynamic wdDoc = wordApp.ActiveDocument;
                wdDoc.Sections[1].PageSetup.LeftMargin = margin;
                wdDoc.Sections[1].PageSetup.RightMargin = margin;
                wdDoc.Sections[1].PageSetup.TopMargin = margin;
                wdDoc.Sections[1].PageSetup.BottomMargin = margin;
                wdDoc.Save();

                // Export as PDF (wdExportFormatPDF = 17)
                wdDoc.ExportAsFixedFormat(pdfName, 17);
                wdDoc.Close();
            }
            finally
            {
                wordApp?.Quit();
            }

            // Clean up the .doc file
            try { File.Delete(docName); } catch { }
        }

        /// <summary>
        /// Generates a PDF for PublicAccess category emails using Adobe Acrobat SDK.
        /// Creates a subject/header page, merges with the email body PDF, and appends
        /// any non-ATT attachments as additional pages. Outputs to the OutDir.
        /// 
        /// Process:
        /// 1. Clears the working directory
        /// 2. Exports email body as PDF via Outlook Inspector/Word editor
        /// 3. Writes email headers to a text file, converts to PDF via Acrobat
        /// 4. Merges subject PDF + body PDF into a single document
        /// 5. Saves any attachments to the working directory
        /// 6. Converts non-PDF attachments to PDF via Acrobat
        /// 7. Appends all attachment PDFs to the merged document
        /// 8. Moves final PDF to the output directory
        /// </summary>
        private void SaveMailAsPdf(dynamic item, string documentId)
        {
            CommonUtilities.ShowDiagnosticIfVerbose("About to generate PDF for PublicAccess", _verbose);

            string pdfDir = _publicAccessBackup;
            string pdfBackup = Path.Combine(_publicAccessBackup, "backup\\");
            string bodyPdf = Path.Combine(_publicAccessBackup, "body.pdf");
            string subjTxt = Path.Combine(_publicAccessBackup, "subj.txt");
            string subjPdf = Path.Combine(_publicAccessBackup, "subj.pdf");
            string pdfDoc = Path.Combine(_publicAccessBackup, $"{documentId}.pdf");

            // Clear working directory
            if (Directory.Exists(pdfDir))
            {
                foreach (var file in Directory.GetFiles(pdfDir))
                {
                    try { File.Delete(file); } catch { }
                }
            }
            else
            {
                Directory.CreateDirectory(pdfDir);
            }

            // Export email body as PDF via Outlook Inspector/Word editor
            dynamic objInspector = item.GetInspector;
            dynamic objDoc = objInspector.WordEditor;
            objDoc.ExportAsFixedFormat(bodyPdf, 17); // wdExportFormatPDF = 17

            System.Threading.Thread.Sleep(3000);

            // Write subject/header text file
            using (var writer = new StreamWriter(subjTxt, true))
            {
                writer.WriteLine($"From: {item.SenderName}");
                writer.WriteLine($"Sent: {item.ReceivedTime}");
                writer.WriteLine($"To: {item.To}");
                writer.WriteLine($"Subject: {item.Subject}");

                int attachCount = (int)item.Attachments.Count;
                string attachList = "";
                if (attachCount > 0)
                {
                    for (int i = 1; i <= attachCount; i++)
                    {
                        string cName = RemoveJunk((string)item.Attachments[i].FileName);
                        if (!cName.StartsWith("ATT", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!string.IsNullOrEmpty(attachList)) attachList += ", ";
                            attachList += (string)item.Attachments[i].FileName;
                            item.Attachments[i].SaveAsFile(Path.Combine(pdfDir, (string)item.Attachments[i].FileName));
                        }
                    }
                }
                writer.WriteLine($"Attachments: {attachList}");
            }

            // Convert subject/header text to PDF and merge with body via Acrobat SDK
            dynamic app = null;
            try
            {
                Type acroAppType = Type.GetTypeFromProgID("AcroExch.App");

                // TODO: FAILING - COM Error 0x80004002 (E_NOINTERFACE) "No such interface supported"
                // Adobe Acrobat COM component not properly registered or not installed.
                // Requires Adobe Acrobat Pro (not Reader) with proper COM registration.
                // Solution: Run as admin: regsvr32 "C:\Program Files\Adobe\Acrobat DC\Acrobat\acrobat.exe" /regserver
                // OR: Consider replacing with modern PDF library (PdfSharp, iTextSharp) to eliminate COM dependency.
                app = Activator.CreateInstance(acroAppType);

                // Convert subject.txt to subject.pdf
                Type avDocType = Type.GetTypeFromProgID("AcroExch.AVDoc");
                dynamic subjObj = Activator.CreateInstance(avDocType);
                subjObj.Open(subjTxt, "");
                dynamic subjPdfObj = subjObj.GetPDDoc();
                subjPdfObj.Save(1, subjPdf);
                subjPdfObj.Close();
                subjObj.Close(-1);
                File.Delete(subjTxt);

                // Merge: subject PDF + body PDF
                Type pdDocType = Type.GetTypeFromProgID("AcroExch.PDDoc");
                dynamic basePdf = Activator.CreateInstance(pdDocType);
                dynamic insrtPdf = Activator.CreateInstance(pdDocType);
                basePdf.Open(subjPdf);
                insrtPdf.Open(bodyPdf);
                int pages = insrtPdf.GetNumPages();
                basePdf.InsertPages(0, insrtPdf, 0, pages, 1);
                basePdf.Save(1, pdfDoc);
                basePdf.Close();
                insrtPdf.Close();
                File.Delete(subjPdf);
                File.Delete(bodyPdf);

                // Convert non-PDF attachments to PDF
                ConvertToPdf(pdfDir, pdfDoc);
            }
            finally
            {
                try
                {
                    app?.CloseAllDocs();
                    app?.Exit();
                }
                catch { }
            }
        }

        /// <summary>
/// Converts non-PDF files in the working directory to PDF using Acrobat SDK,
/// then merges all PDFs into the main document. Moves the final merged PDF
/// to the output directory.
/// </summary>
private void ConvertToPdf(string pdfDir, string pdfDoc)
{
    CommonUtilities.ShowDiagnosticIfVerbose("in converttopdf", _verbose);

    dynamic app = null;
    try
    {
        Type acroAppType = Type.GetTypeFromProgID("AcroExch.App");
        
        // TODO: FAILING - COM Error 0x80004002 (E_NOINTERFACE) "No such interface supported"
        // Adobe Acrobat COM component not properly registered or not installed.
        // Requires Adobe Acrobat Pro (not Reader) with proper COM registration.
        // Solution: Run as admin: regsvr32 "C:\Program Files\Adobe\Acrobat DC\Acrobat\acrobat.exe" /regserver
        // OR: Consider replacing with modern PDF library (PdfSharp, iTextSharp) to eliminate COM dependency.
        app = Activator.CreateInstance(acroAppType);

        var files = Directory.GetFiles(pdfDir);
        if (files.Length == 0) return;

        // Convert non-PDF files to PDF
        Type avDocType = Type.GetTypeFromProgID("AcroExch.AVDoc");
        foreach (var filePath in files)
        {
            string ext = Path.GetExtension(filePath).TrimStart('.').ToLower();
            if (ext != "pdf")
            {
                dynamic fileObj = Activator.CreateInstance(avDocType);
                fileObj.Open(filePath, "");
                dynamic filePdf = fileObj.GetPDDoc();
                filePdf.Save(1, filePath + ".pdf");
                filePdf.Close();
                fileObj.Close(-1);
                File.Delete(filePath);
            }
        }

        // Merge all remaining PDFs into the main document
        if (File.Exists(pdfDoc))
        {
            Type pdDocType = Type.GetTypeFromProgID("AcroExch.PDDoc");
            foreach (var filePath in Directory.GetFiles(pdfDir))
            {
                if (filePath != pdfDoc)
                {
                    dynamic basePdf = Activator.CreateInstance(pdDocType);
                    dynamic insrtPdf = Activator.CreateInstance(pdDocType);
                    basePdf.Open(pdfDoc);
                    insrtPdf.Open(filePath);
                    int lastPg = basePdf.GetNumPages() - 1;
                    int pages = insrtPdf.GetNumPages();
                    basePdf.InsertPages(lastPg, insrtPdf, 0, pages, 1);
                    basePdf.Save(1, pdfDoc);
                    basePdf.Close();
                    insrtPdf.Close();
                    File.Delete(filePath);
                }
            }
        }

        // Move final PDF to output directory
        if (File.Exists(pdfDoc))
        {
            string destPath = Path.Combine(_outDir, Path.GetFileName(pdfDoc));
            File.Move(pdfDoc, destPath, true);
        }
    }
    finally
    {
        try
        {
            app?.CloseAllDocs();
            app?.Exit();
        }
        catch { }
    }
}

        /// <summary>
        /// Calls SP_CREATE_EGRANTS_DOCUMENT_NEW to register a document in the EIM database.
        /// Returns the document ID if successful ("Success" or "Advisory"), empty string otherwise.
        /// </summary>
        private string GetDocumentId(SqlConnection con, string documentId, string category, string applId,
            string profileId, string docDate, string senderId, string fileType, string moveToQc, string subcat)
        {
            try
            {
                using (var cmd = new SqlCommand("SP_CREATE_EGRANTS_DOCUMENT_NEW", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@DOCID", documentId ?? "");
                    cmd.Parameters.AddWithValue("@CAT", category ?? "");
                    cmd.Parameters.AddWithValue("@APPID", applId ?? "");
                    cmd.Parameters.AddWithValue("@PROFILEID", profileId ?? "1");
                    cmd.Parameters.AddWithValue("@DD", docDate ?? "");
                    cmd.Parameters.AddWithValue("@UID", senderId ?? "");
                    cmd.Parameters.AddWithValue("@FT", fileType ?? "txt");
                    cmd.Parameters.AddWithValue("@QCFLAG", moveToQc ?? "no");
                    cmd.Parameters.AddWithValue("@SUB", subcat ?? "");

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string name = reader["name"]?.ToString() ?? "";
                            if (name == "Success" || name == "Advisory")
                            {
                                return reader["value"]?.ToString() ?? "";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonUtilities.WriteLog(8, "Error calling SP_CREATE_EGRANTS_DOCUMENT_NEW", ex.Message, DateTime.Now);
            }
            return "";
        }

        /// <summary>
        /// Calls SP_CLEAR_OLD_JIT_SUBMISSIONS for JIT Submitted documents
        /// to clear previous submissions before registering the new one.
        /// </summary>
        private void ClearOldJitSubmissions(SqlConnection con, string documentId)
        {
            try
            {
                using (var cmd = new SqlCommand($"exec SP_CLEAR_OLD_JIT_SUBMISSIONS '{documentId}'", con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                CommonUtilities.WriteLog(8, "Error clearing old JIT submissions", ex.Message, DateTime.Now);
            }
        }

        /// <summary>
        /// Moves a processed email to the "old" subfolder of its current folder.
        /// </summary>
        private void MoveToOldFolder(dynamic folder, dynamic item)
        {
            CommonUtilities.ShowDiagnosticIfVerbose($"Moving old email with subject : '{item.Subject}'", _verbose);
            CommonUtilities.ShowDiagnosticIfVerbose($"Moving to old folder which is : '{folder.FolderPath}'", _verbose);
            dynamic oldFolder = folder.Folders["old"];
            item.Move(oldFolder);
            CommonUtilities.ShowDiagnosticIfVerbose("moved", _verbose);
        }

        /// <summary>
        /// Forwards an error notification email to admin recipients with the error details.
        /// </summary>
        private void RaiseErrorToAdmin(dynamic item, string errorMsg1, string errorMsg2)
        {
            try
            {
                dynamic outMail = item.Forward();
                string[] recipients = _adminRecipients.Split(';');
                foreach (var recipient in recipients)
                {
                    if (!string.IsNullOrWhiteSpace(recipient))
                        outMail.Recipients.Add(recipient.Trim());
                }
                outMail.Subject = GetEnvironmentPrefix() + $"{errorMsg1} >>(Subj: {item.Subject})";
                outMail.Body = $"{errorMsg2}\r\n\r\n{item.Body}";
                outMail.Send();
            }
            catch (Exception ex)
            {
                CommonUtilities.WriteLog(8, "Failed to send error notification", ex.Message, DateTime.Now);
            }
        }

        /// <summary>
        /// Sends an admin warning email (not a forward, a new email).
        /// Used for the 30-item limit warning.
        /// </summary>
        private void SendAdminWarning(string subject, string body)
        {
            try
            {
                dynamic mailItem = _outlookApp.CreateItem(0); // olMailItem = 0
                mailItem.To = _adminRecipients.Replace(";", ";");
                mailItem.Subject = GetEnvironmentPrefix() + subject;
                mailItem.BodyFormat = 2; // olFormatHTML
                mailItem.HTMLBody = " " + body;
                mailItem.Send();
            }
            catch (Exception ex)
            {
                CommonUtilities.WriteLog(8, "Failed to send admin warning", ex.Message, DateTime.Now);
            }
        }

        /// <summary>
        /// Sends a notification email (e.g., Late Progress Report uploaded).
        /// </summary>
        private void SendNotificationEmail(string subject, string body)
        {
            try
            {
                dynamic mailItem = _outlookApp.CreateItem(0); // olMailItem = 0
                mailItem.To = "daryl.dehuff@nih.gov"; // "egrantsdevs@mail.nih.gov";
                mailItem.Subject = GetEnvironmentPrefix() + subject;
                mailItem.BodyFormat = 2;
                mailItem.HTMLBody = " " + body;
                mailItem.Send();
            }
            catch (Exception ex)
            {
                CommonUtilities.WriteLog(8, "Failed to send notification", ex.Message, DateTime.Now);
            }
        }

        /// <summary>
        /// Parses a comma-delimited subject line into structured metadata.
        /// Expected format: "grantnumber=VALUE, category=VALUE, sub=VALUE, extract=VALUE"
        /// Also handles: applid, documentdate, documentid
        /// </summary>
        private SubjectParams ParseSubjectLine(string subject)
        {
            var p = new SubjectParams();
            foreach (var part in subject.Split(','))
            {
                string lp = part.Trim().ToLower();
                if (lp.Contains("grantnumber")) p.GrantNumber = ExtractValue(part, "grantnumber");
                else if (lp.Contains("category")) p.Category = ExtractValue(part, "category");
                else if (lp.Contains("applid")) p.ApplId = ExtractValue(part, "applid");
                else if (lp.Contains("documentdate")) p.DocumentDate = ExtractValue(part, "documentdate");
                else if (lp.Contains("documentid")) p.DocumentId = ExtractValue(part, "documentid");
                else if (lp.Contains("sub=")) p.SubCategory = ExtractValue(part, "sub");
                else if (lp.Contains("extract")) p.Extract = ExtractValue(part, "extract");
            }

            // Default extract to "1" if not specified but has grant number or applid
            if (string.IsNullOrEmpty(p.Extract))
                p.Extract = "1";

            return p;
        }

        /// <summary>
        /// Navigates to an Outlook MAPI folder using a backslash-separated path string.
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
        /// Falls back to extracting alias from the EX address if GetExchangeUser fails.
        /// For SMTP senders, returns the raw email address.
        /// </summary>
        private string GetSenderId(dynamic item)
        {
            try
            {
                if ((string)item.SenderEmailType == "EX")
                {
                    var exchUser = item.Sender?.GetExchangeUser();
                    if (exchUser != null)
                    {
                        string alias = (string)exchUser.Alias;
                        if (!string.IsNullOrEmpty(alias)) return alias;
                    }

                    // Fallback: extract alias from EX address (last segment after '=')
                    return GetAliasFromExAddress((string)item.SenderEmailAddress);
                }
                return (string)item.SenderEmailType == "SMTP" ? (string)item.SenderEmailAddress : "";
            }
            catch { return ""; }
        }

        /// <summary>
        /// Extracts the alias from an Exchange (EX) address by taking the last segment after '='.
        /// </summary>
        private string GetAliasFromExAddress(string exAddress)
        {
            if (string.IsNullOrEmpty(exAddress)) return "";
            string result = exAddress;
            while (result.Contains("="))
            {
                int pos = result.IndexOf('=');
                result = result.Substring(pos + 1);
            }
            return result;
        }

        /// <summary>
        /// Looks up an application ID from a grant number using the
        /// dbo.Imm_fn_applid_match() SQL function.
        /// </summary>
        private string GetApplId(string text, SqlConnection con)
        {
            try
            {
                using (var cmd = new SqlCommand($"SELECT dbo.Imm_fn_applid_match(' {text} ') as applid", con))
                {
                    var result = cmd.ExecuteScalar();
                    return result == null || result == DBNull.Value ? "" : result.ToString();
                }
            }
            catch { return ""; }
        }

        /// <summary>
        /// Determines if QC review is required based on file type.
        /// Standard types (pdf, txt, doc, xls, docx, xlsx, ppt) do not require QC.
        /// All other types require QC review.
        /// </summary>
        private string IsQcRequired(string fileType)
        {
            if (string.IsNullOrEmpty(fileType)) return "yes";
            string ft = fileType.ToLower();
            return (ft == "pdf" || ft == "txt" || ft == "doc" || ft == "xls" ||
                    ft == "docx" || ft == "xlsx" || ft == "ppt") ? "no" : "yes";
        }

        /// <summary>
        /// Extracts the value from a "key=value" string if the key contains the specified name.
        /// </summary>
        private string ExtractValue(string p, string name)
        {
            string[] parts = p.Split('=');
            if (parts.Length == 2 && parts[0].Trim().ToLower().Contains(name))
                return parts[1].Trim();
            return null;
        }

        /// <summary>
        /// Gets the file extension from a filename, defaulting to "txt" if none found.
        /// </summary>
        private string GetFileType(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || !fileName.Contains(".")) return "txt";
            // Get last extension segment (handles names like "file.name.pdf")
            string result = fileName;
            while (result.Contains("."))
            {
                int pos = result.IndexOf('.');
                result = result.Substring(pos + 1);
            }
            return result;
        }

        /// <summary>
        /// Removes special characters (colons, slashes, angle brackets, etc.) from text
        /// to normalize grant numbers for database lookup. Matches VBS removespcharacters().
        /// </summary>
        private string RemoveSpecialChars(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text
                .Replace("\n", "\r\n")
                .Replace(":", " ")
                .Replace("/", " ")
                .Replace("\\", " ")
                .Replace("&", "and")
                .Replace(";", " ")
                .Replace("<", " ")
                .Replace(">", " ")
                .Replace("<<", " ")
                .Replace(">>", " ")
                .Replace("^", " ")
                .Replace("%", " ")
                .Replace("@", " ")
                .Replace("'", " ")
                .Replace(" ", "")
                .Trim();
        }

        /// <summary>
        /// Removes junk characters from attachment filenames (colons, slashes, ampersands, semicolons).
        /// Matches VBS removejunk() function.
        /// </summary>
        private string RemoveJunk(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return "";
            return fileName
                .Replace(":", " ")
                .Replace("/", " ")
                .Replace("\\", " ")
                .Replace("&", "and")
                .Replace(";", " ")
                .Trim();
        }

        /// <summary>
        /// Holds parsed metadata from a structured email subject line.
        /// </summary>
        private class SubjectParams
        {
            public string GrantNumber { get; set; }
            public string Category { get; set; }
            public string ApplId { get; set; }
            public string SubCategory { get; set; }
            public string Extract { get; set; }
            public string DocumentDate { get; set; }
            public string DocumentId { get; set; }
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
