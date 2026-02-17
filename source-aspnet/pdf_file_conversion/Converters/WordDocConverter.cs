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

            // 1. Write input DOC to temp
            string tempInputPath = Path.Combine(Path.GetTempPath(), content.SingleFileFileName);
            Log.Information("tempInputPath: " + tempInputPath);
            File.WriteAllBytes(tempInputPath, content.GetBytes());

            // 2. Determine output PDF path
            string tempOutputDir = Path.GetTempPath().TrimEnd('\\');
            string tempOutputPath = Path.Combine(
                tempOutputDir,
                Path.GetFileNameWithoutExtension(content.SingleFileFileName) + ".pdf"
            );

            Log.Information("tempOutputDir: " + tempOutputDir);

            // 3. Build LibreOffice process info
            var processInfo = new ProcessStartInfo
            {
                FileName = _libreOfficePath,
                Arguments =
                    "--headless --nologo --nofirststartwizard " +
                    $"--convert-to pdf \"{tempInputPath}\" --outdir \"{tempOutputDir}\"",

                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // 4. Execute conversion
            using (var process = Process.Start(processInfo))
            {
                string stdout = process.StandardOutput.ReadToEnd();
                string stderr = process.StandardError.ReadToEnd();

                process.WaitForExit();

                Log.Information("LibreOffice stdout: {Stdout}", stdout);
                Log.Information("LibreOffice stderr: {Stderr}", stderr);

                if (process.ExitCode != 0)
                {
                    throw new Exception($"LibreOffice conversion failed. ExitCode={process.ExitCode}. Error: {stderr}");
                }
            }

            // 5. Load PDF
            if (!File.Exists(tempOutputPath))
                throw new Exception("LibreOffice reported success but no PDF was created.");

            PdfDocument pdfDocument = PdfDocument.FromFile(tempOutputPath);

            // 6. Cleanup
            try
            {
                File.Delete(tempInputPath);
                File.Delete(tempOutputPath);
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
