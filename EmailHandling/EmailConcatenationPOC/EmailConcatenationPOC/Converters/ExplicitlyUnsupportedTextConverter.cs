using EmailConcatenationPOC.Interfaces;
using IronPdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailConcatenationPOC.Converters
{
    internal class ExplicitlyUnsupportedTextConverter : IExplicitlyUnsupportedTextConverter
    {
        public bool SupportsThisFileType(string fileName)
        {
            // MLH : get .doc converting to .docx and then remove these two caveats here

            if (fileName.ToLower().Contains(".") &&
                Constants.ExplicitlyUnsupportedFileTypes
                    .Any(ftt => fileName.ToLower().Contains(ftt) && !fileName.ToLower().Contains(".docx")) ||
                //Constants.ExcelTypes
                //    .Any(ftt => fileName.ToLower().Contains(ftt) && !fileName.ToLower().Contains(".docx"))
                Constants.UnsupportedExcelTypes
                        .Any(ftt => fileName.ToLower().Contains(ftt))
                    )
                return true;
            return false;
        }

        public List<PdfDocument> ToPdfDocument(ContentForPdf content)
        {
            Console.WriteLine("Handling explicitly unsupported file type case ...");

            if (content.Type != ContentForPdf.ContentType.DataAttachment)
                throw new Exception("Converted didn't see the expected type for this conversion. Make sure you set the DataAttachment.");

            var renderer = new ChromePdfRenderer();

            var sb = new StringBuilder();
            sb.AppendLine("<html><body><p><h1>Explicitly Unsupported Filetype</h1></p>");
            sb.AppendLine("<p>A file was discovered with an extension that does not render to PDF.</p>");
            sb.AppendLine("<p>FYI : files with this type have been found to exist in the eGrants database.</p>");
            sb.AppendLine($"<p>The offending file's name is : {content.Attachment.FileName}</p></body></html>");

            using (var pdfDocument = renderer.RenderHtmlAsPdf(sb.ToString()))
            {
                using (var memoryStream = new MemoryStream())
                {
                    pdfDocument.Stream.CopyTo(memoryStream);

                    var bytes = memoryStream.ToArray();
                    var pdfDocFromStream = new PdfDocument(bytes);
                    return new List<PdfDocument> { pdfDocFromStream };
                }
            }
        }
    }
}
