using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using DocumentFormat.OpenXml.Wordprocessing;

using EmailConcatenation.Interfaces;

using IronPdf;
using IronPdf.Rendering;

using Microsoft.Extensions.Configuration;

using NPOI.OpenXml4Net.OPC;
using NPOI.OpenXmlFormats.Wordprocessing;
using NPOI.XWPF.UserModel;

using Spire.Doc;

using Serilog;


namespace EmailConcatenation.Converters
{
    public class WordDocConverter : IWordDocConverter, IConvertToPdf
    {
        private readonly string _libreOfficePath;

        public WordDocConverter(IConfiguration configuration)
        {
#if DEBUG
            _libreOfficePath = "C:\\Development\\LibreOffice\\program\\soffice.exe";
#else
            _libreOfficePath = configuration["LibreOffice:Path"];
#endif

            if (string.IsNullOrEmpty(_libreOfficePath))
            {
                throw new InvalidOperationException("LibreOffice path not configured in appsettings.json");
            }
        }

        public bool SupportsThisFileType(string fileName)
        {
            if (!string.IsNullOrWhiteSpace(fileName) &&
                fileName.ToLower().EndsWith(".doc") )
                return true;
            return false;
        }

        public List<PdfDocument> ToPdfDocument(ContentForPdf content)
        {
            if (content.GetBytes().Length == 0)
                return null;

            string tempDir = Path.GetTempPath().TrimEnd('\\');

            // Determine original extension
            string originalExt = Path.GetExtension(content.SingleFileFileName)
                                        .ToLowerInvariant();

            // 1. Write original file to temp
            string tempInputPath = Path.Combine(tempDir, content.SingleFileFileName);
            File.WriteAllBytes(tempInputPath, content.GetBytes());
            Log.Information("tempInputPath: " + tempInputPath);

            // 2. Ensure we have a DOCX file to convert to PDF
            string tempDocxPath;

            if (originalExt == ".doc")
            {
                // Build DOCX output path
                string docxFileName = Path.GetFileNameWithoutExtension(tempInputPath) + ".docx";
                tempDocxPath = Path.Combine(tempDir, docxFileName);
                Log.Information("Converting .doc → .docx: " + tempDocxPath);

                // Convert DOC → DOCX
                var toDocx = new ProcessStartInfo
                {
                    FileName = _libreOfficePath,
                    Arguments =
                        "--headless --nologo --nofirststartwizard " +
                        $"--convert-to docx \"{tempInputPath}\" --outdir \"{tempDir}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(toDocx))
                {
                    string stdout = process.StandardOutput.ReadToEnd();
                    string stderr = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    Log.Information("DOC→DOCX stdout: {Stdout}", stdout);
                    Log.Information("DOC→DOCX stderr: {Stderr}", stderr);

                    if (process.ExitCode != 0)
                        throw new Exception($"DOC→DOCX conversion failed. ExitCode={process.ExitCode}. Error: {stderr}");
                }

                if (!File.Exists(tempDocxPath))
                    throw new Exception("DOC→DOCX conversion reported success but no DOCX was created.");
            }
            else if (originalExt == ".docx")
            {
                // Already DOCX — use it directly
                tempDocxPath = tempInputPath;
                Log.Information("Input is already .docx, skipping conversion.");
            }
            else
            {
                throw new Exception("Unsupported file type. Only .doc and .docx are supported.");
            }

            // 3. Convert DOCX → PDF
            string pdfFileName = Path.GetFileNameWithoutExtension(tempDocxPath) + ".pdf";
            string tempPdfPath = Path.Combine(tempDir, pdfFileName);
            Log.Information("tempPdfPath: " + tempPdfPath);

            var toPdf = new ProcessStartInfo
            {
                FileName = _libreOfficePath,
                Arguments =
                    "--headless --nologo --nofirststartwizard " +
                    $"--convert-to pdf \"{tempDocxPath}\" --outdir \"{tempDir}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(toPdf))
            {
                string stdout = process.StandardOutput.ReadToEnd();
                string stderr = process.StandardError.ReadToEnd();
                process.WaitForExit();

                Log.Information("DOCX→PDF stdout: {Stdout}", stdout);
                Log.Information("DOCX→PDF stderr: {Stderr}", stderr);

                if (process.ExitCode != 0)
                    throw new Exception($"DOCX→PDF conversion failed. ExitCode={process.ExitCode}. Error: {stderr}");
            }

            if (!File.Exists(tempPdfPath))
                throw new Exception("DOCX→PDF conversion reported success but no PDF was created.");

            // 4. Load PDF
            PdfDocument pdfDocument = PdfDocument.FromFile(tempPdfPath);

            // 5. Cleanup
            try
            {
                File.Delete(tempInputPath);
                if (originalExt == ".doc") File.Delete(tempDocxPath);
                File.Delete(tempPdfPath);
            }
            catch { }

            return new List<PdfDocument> { pdfDocument };
        }

        private static HttpContent Upload(string actionUrl, string paramString, Stream paramFileStream, byte[] paramFileBytes)
        {
            using (var client = new HttpClient())
            using (var formData = new MultipartFormDataContent())
            {
                paramFileStream.Position = 0;
                HttpContent fileStreamContent = new StreamContent(paramFileStream);
                Console.WriteLine("Acquired client and formData.");

                fileStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                var response = client.PostAsync(actionUrl, fileStreamContent).Result;
                response.EnsureSuccessStatusCode();

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }
                return response.Content;
            }
        }

        private bool DocXIsPortrait(ContentForPdf content)
        {
            OPCPackage pkg = null;

            if (content.IsMemoryStream && content.MemoryStream != null)
            {
                pkg = OPCPackage.Open(content.MemoryStream);
            }
            else
            {
                pkg = OPCPackage.Open(new MemoryStream(content.GetBytes()));
            }
            var doc = new XWPFDocument(pkg);

            foreach (var paragraph in doc.Paragraphs)
            {
                var ctp = paragraph.GetCTP();
                if (ctp.IsSetPPr())
                {
                    var ppr = ctp.pPr;
                    if (ppr != null)
                    {
                        var sectPtr = ppr.sectPr;
                        if (sectPtr != null)
                        {
                            var pageSize = sectPtr.pgSz;
                            if (pageSize != null)
                            {
                                if (pageSize.orient == ST_PageOrientation.portrait)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            // check body
            var body = doc.Document.body;
            if (body.sectPr != null)
            {
                var sectPr = body.sectPr;
                if (sectPr != null)
                {
                    var pageSz = sectPr.pgSz;
                    if (pageSz != null)
                    {
                        if (pageSz.orient == ST_PageOrientation.portrait)
                            return true;
                    }
                }
            }

            // MLH : return true if any are portrait since we can only set it in the IronPdf DocX converter in one place

            return false;
        }
    }
}
