using System;
using System.Collections.Generic;
using ExchangeFixed;
using CommonUtilties;

namespace EmailHandlingTests.ExchangeFixed
{
    internal class TestExchangeFixedProcessor : Processor
    {
        public List<TestExchangeEmailRecord> EmailsProcessedThisSession { get; } = new List<TestExchangeEmailRecord>();
        public List<TestFileSaveOperation> FileSaveOperations { get; } = new List<TestFileSaveOperation>();
        public List<string> AdminNotifications { get; } = new List<string>();
        public List<string> NotificationEmails { get; } = new List<string>();
        public int ProcessedCount { get; private set; }
        public bool ErrorOccurred { get; private set; }
        public string LastErrorMessage { get; private set; }
        public List<SimulatedExchangeEmail> SimulatedEmails { get; set; } = new List<SimulatedExchangeEmail>();
        public bool ItemLimitReached { get; private set; }

        public TestExchangeEmailRecord TestProcessSingleEmail(string subject, string body = "",
            string senderEmail = "user@nih.gov", int attachmentCount = 0,
            string verbose = "n", List<string> attachmentFileNames = null)
        {
            try
            {
                var parsedParams = ParseSubjectLinePublic(subject);

                if (string.IsNullOrEmpty(parsedParams.GrantNumber) && string.IsNullOrEmpty(parsedParams.ApplId))
                {
                    return null;
                }

                // Simulate NCIOGAPROGESS sender handling
                string resolvedSender = senderEmail;
                if (senderEmail?.Trim() == "FD6862D09E7043D49596358F980D064F-NCI OGA PRO")
                {
                    resolvedSender = "NCIOGAPROGESS";
                    parsedParams.Category = "Notification";
                    parsedParams.SubCategory = "Late Progress Report";
                    parsedParams.Extract = "1";
                    NotificationEmails.Add("Late Progress Report uploaded");
                }

                ProcessedCount++;

                // Determine QC flag
                string moveToQc = string.IsNullOrEmpty(parsedParams.ApplId) && string.IsNullOrEmpty(parsedParams.GrantNumber)
                    ? "yes" : "no";

                string category = parsedParams.Category ?? "Correspondence";
                string extract = parsedParams.Extract ?? "1";

                var record = new TestExchangeEmailRecord
                {
                    Subject = subject,
                    Body = body,
                    SenderEmail = resolvedSender,
                    GrantNumber = parsedParams.GrantNumber,
                    Category = category,
                    ApplId = parsedParams.ApplId,
                    SubCategory = parsedParams.SubCategory,
                    Extract = extract,
                    DocumentDate = parsedParams.DocumentDate,
                    DocumentId = parsedParams.DocumentId,
                    AttachmentCount = attachmentCount,
                    MoveToQc = moveToQc,
                    ProcessedTime = DateTime.Now
                };

                EmailsProcessedThisSession.Add(record);

                // Simulate file save operations based on extract type and category
                if (extract == "1" || extract == "3")
                {
                    // Determine if this category generates PDF or TXT
                    string saveType = DetermineBodySaveType(category, parsedParams.SubCategory ?? "");
                    FileSaveOperations.Add(new TestFileSaveOperation
                    {
                        FileName = $"placeholder.{(saveType == "PDF" ? "pdf" : "txt")}",
                        FileType = saveType == "PDF" ? "pdf" : "txt",
                        SaveType = saveType == "PDF" ? "EmailBodyPDF" : "EmailBody",
                        Category = category,
                        SubCategory = parsedParams.SubCategory
                    });
                }

                if ((extract == "2" || extract == "3") && attachmentCount > 0)
                {
                    // Simulate processing each attachment (skip ATT* prefixed)
                    var fileNames = attachmentFileNames ?? GenerateDefaultAttachmentNames(attachmentCount);
                    foreach (var fileName in fileNames)
                    {
                        string cleanName = RemoveJunkPublic(fileName);
                        if (!cleanName.StartsWith("ATT", StringComparison.OrdinalIgnoreCase))
                        {
                            string fileType = GetFileTypePublic(cleanName);
                            string qcRequired = IsQcRequiredPublic(fileType);
                            FileSaveOperations.Add(new TestFileSaveOperation
                            {
                                FileName = $"placeholder.{fileType}",
                                FileType = fileType,
                                SaveType = "Attachment",
                                QcRequired = qcRequired
                            });
                        }
                    }
                }

                // Check 30-item limit
                if (ProcessedCount >= 30)
                {
                    ItemLimitReached = true;
                    AdminNotifications.Add("Warning! 30 items processed in one instance.");
                }

                return record;
            }
            catch (Exception ex)
            {
                ErrorOccurred = true;
                LastErrorMessage = ex.Message;
                return null;
            }
        }

        public int TestProcessSimulatedEmails(string verbose = "n")
        {
            try
            {
                int processed = 0;
                foreach (var email in SimulatedEmails)
                {
                    if (ItemLimitReached) break;

                    var result = TestProcessSingleEmail(
                        email.Subject, email.Body, email.SenderEmail,
                        email.AttachmentCount, verbose, email.AttachmentFileNames);
                    if (result != null) processed++;
                }
                return processed;
            }
            catch (Exception ex)
            {
                ErrorOccurred = true;
                LastErrorMessage = ex.Message;
                return ProcessedCount;
            }
        }

        /// <summary>
        /// Determines whether the body extraction should produce PDF or TXT
        /// based on category and subcategory (matches Processor logic).
        /// </summary>
        private string DetermineBodySaveType(string category, string subcat)
        {
            if (category == "PublicAccess") return "PDF";
            if (category == "JIT Info" || category == "CT.gov") return "PDF";
            if (category == "eRA Notification" && subcat == "JIT Submitted") return "PDF";
            if (category.ToLower() == "closeout" && subcat.ToLower() == "past due documents reminder") return "PDF";
            if (category.ToLower() == "closeout" && subcat.ToLower() == "f-rppr acceptance past due reminder") return "PDF";
            if (category.ToLower() == "correspondence" && subcat.ToLower() == "rppr unobligated balance") return "PDF";
            if (category == "Funding" && subcat.ToLower().Contains("dci-inth")) return "PDF";
            return "TXT";
        }

        private List<string> GenerateDefaultAttachmentNames(int count)
        {
            var names = new List<string>();
            for (int i = 0; i < count; i++)
                names.Add($"attachment{i + 1}.pdf");
            return names;
        }

        public TestSubjectParams ParseSubjectLinePublic(string subject)
        {
            var p = new TestSubjectParams();
            if (string.IsNullOrEmpty(subject))
            {
                // Default extract to "1" even for empty subjects (matches Processor logic)
                p.Extract = "1";
                return p;
            }

            foreach (var part in subject.Split(','))
            {
                string lp = part.Trim().ToLower();
                if (lp.Contains("grantnumber")) p.GrantNumber = ExtractValuePublic(part, "grantnumber");
                else if (lp.Contains("category")) p.Category = ExtractValuePublic(part, "category");
                else if (lp.Contains("applid")) p.ApplId = ExtractValuePublic(part, "applid");
                else if (lp.Contains("documentdate")) p.DocumentDate = ExtractValuePublic(part, "documentdate");
                else if (lp.Contains("documentid")) p.DocumentId = ExtractValuePublic(part, "documentid");
                else if (lp.Contains("sub=")) p.SubCategory = ExtractValuePublic(part, "sub");
                else if (lp.Contains("extract")) p.Extract = ExtractValuePublic(part, "extract");
            }

            // Default extract to "1" if not specified
            if (string.IsNullOrEmpty(p.Extract))
                p.Extract = "1";

            return p;
        }

        public string ExtractValuePublic(string p, string name)
        {
            if (string.IsNullOrEmpty(p)) return null;
            string[] parts = p.Split('=');
            return (parts.Length == 2 && parts[0].Trim().ToLower().Contains(name)) ? parts[1].Trim() : null;
        }

        public string GetFileTypePublic(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || !fileName.Contains(".")) return "txt";
            string result = fileName;
            while (result.Contains("."))
            {
                int pos = result.IndexOf('.');
                result = result.Substring(pos + 1);
            }
            return result;
        }

        public string RemoveSpecialCharsPublic(string text)
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

        public string RemoveJunkPublic(string fileName)
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

        public string IsQcRequiredPublic(string fileType)
        {
            if (string.IsNullOrEmpty(fileType)) return "yes";
            string ft = fileType.ToLower();
            return (ft == "pdf" || ft == "txt" || ft == "doc" || ft == "xls" ||
                    ft == "docx" || ft == "xlsx" || ft == "ppt") ? "no" : "yes";
        }

        public string GetAliasFromExAddressPublic(string exAddress)
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

        public bool IsValidEmailForProcessing(string subject)
        {
            var p = ParseSubjectLinePublic(subject);
            return !string.IsNullOrEmpty(p.GrantNumber) || !string.IsNullOrEmpty(p.ApplId);
        }

        public void AddSimulatedEmail(string subject, string body = "", string senderEmail = "user@nih.gov",
            int attachmentCount = 0, List<string> attachmentFileNames = null)
        {
            SimulatedEmails.Add(new SimulatedExchangeEmail
            {
                Subject = subject,
                Body = body,
                SenderEmail = senderEmail,
                AttachmentCount = attachmentCount,
                AttachmentFileNames = attachmentFileNames,
                ReceivedTime = DateTime.Now.AddMinutes(-10)
            });
        }

        public void Reset()
        {
            EmailsProcessedThisSession.Clear();
            FileSaveOperations.Clear();
            AdminNotifications.Clear();
            NotificationEmails.Clear();
            ProcessedCount = 0;
            ErrorOccurred = false;
            LastErrorMessage = null;
            SimulatedEmails.Clear();
            ItemLimitReached = false;
        }
    }

    public class TestSubjectParams
    {
        public string GrantNumber { get; set; }
        public string Category { get; set; }
        public string ApplId { get; set; }
        public string SubCategory { get; set; }
        public string Extract { get; set; }
        public string DocumentDate { get; set; }
        public string DocumentId { get; set; }
    }

    public class TestExchangeEmailRecord
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public string SenderEmail { get; set; }
        public string GrantNumber { get; set; }
        public string Category { get; set; }
        public string ApplId { get; set; }
        public string SubCategory { get; set; }
        public string Extract { get; set; }
        public string DocumentDate { get; set; }
        public string DocumentId { get; set; }
        public int AttachmentCount { get; set; }
        public string MoveToQc { get; set; }
        public DateTime ProcessedTime { get; set; }
    }

    public class TestFileSaveOperation
    {
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string SaveType { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string QcRequired { get; set; }
    }

    public class SimulatedExchangeEmail
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public string SenderEmail { get; set; }
        public int AttachmentCount { get; set; }
        public List<string> AttachmentFileNames { get; set; }
        public DateTime ReceivedTime { get; set; }
    }
}
