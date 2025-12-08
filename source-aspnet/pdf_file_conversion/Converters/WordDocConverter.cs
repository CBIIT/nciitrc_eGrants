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




namespace EmailConcatenation.Converters
{
    public class WordDocConverter : IWordDocConverter, IConvertToPdf
    {
        private readonly string _libreOfficePath;

        public WordDocConverter(IConfiguration configuration)
        {
            _libreOfficePath = configuration["LibreOffice:Path"];
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

            // Write input file to temp location
            string tempInputPath = Path.Combine(Path.GetTempPath(), content.SingleFileFileName);
            File.WriteAllBytes(tempInputPath, content.GetBytes());

            // Prepare output path
            string tempOutputDir = Path.GetTempPath().TrimEnd('\\');

            string tempOutputPath = Path.Combine(
                tempOutputDir,
                Path.GetFileNameWithoutExtension(content.SingleFileFileName) + ".pdf"
            );

            // Call LibreOffice using configured path
            var processInfo = new ProcessStartInfo
            {
                FileName = _libreOfficePath,
                Arguments = $"--headless --convert-to pdf \"{tempInputPath}\" --outdir \"{tempOutputDir}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(processInfo))
            {
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    throw new Exception("LibreOffice conversion failed: " + process.StandardError.ReadToEnd());
                }
            }

            // Load PDF back into IronPdf
            //byte[] pdfBytes = File.ReadAllBytes(tempOutputPath);
            PdfDocument pdfDocument = PdfDocument.FromFile(tempOutputPath);

            // Clean up temp files
            try
            {
                File.Delete(tempInputPath);
                File.Delete(tempOutputPath);
            }
            catch { /* ignore cleanup errors */ }

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
