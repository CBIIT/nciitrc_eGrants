using EmailConcatenation.Interfaces;
using IronPdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spire.Doc;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace EmailConcatenation.Converters
{
    public class WordDocConverter : IWordDocConverter, IConvertToPdf
    {
        public bool SupportsThisFileType(string fileName)
        {
            if (!string.IsNullOrWhiteSpace(fileName) &&
                fileName.ToLower().EndsWith(".doc") )
                return true;
            return false;
        }

        public List<PdfDocument> ToPdfDocument(ContentForPdf content)
        {
            Console.WriteLine("Handling Word .doc file type case ...");

            string tempFilePath = Path.GetTempFileName();
            File.WriteAllBytes(tempFilePath, content.GetBytes());

            string sofficePath = @"""C:\Program Files\LibreOffice\program\soffice.com""";
            string args = $"--headless --convert-to docx \"{tempFilePath}\" --outdir \"{Path.GetDirectoryName(tempFilePath)}\"";

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = sofficePath,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using (Process process = new Process { StartInfo = startInfo })
            {
                process.Start();

                // make sure it doesn't have any output like this :
                // Entity: line 1: parser error : Document is empty

                string errorIndicatorString = "parser error :";

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (output.Contains(errorIndicatorString) || error.Contains(errorIndicatorString))
                {
                    throw new ExternalException($"Creating the Word .docx file from the .doc failed after attempting to use this command : {sofficePath} {args}");
                }
            }

            string convertedFilePath = Path.Combine(Path.GetDirectoryName(tempFilePath), Path.GetFileNameWithoutExtension(tempFilePath) + ".docx");

            DocxToPdfRenderer docXRenderer = new DocxToPdfRenderer();
            PdfDocument pdfDocument = docXRenderer.RenderDocxAsPdf(convertedFilePath);

            return new List<PdfDocument> { pdfDocument };
        }
    }
}
