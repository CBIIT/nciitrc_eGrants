using EmailConcatenationPOC.Interfaces;
using IronPdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace EmailConcatenationPOC.Converters
{
    internal class ExcelConverter : IExcelConverter, IConvertToPdf
    {
        public bool SupportsThisFileType(string fileName)
        {
            if (fileName.ToLower().Contains(".") &&
                fileName.ToLower().Contains(".docx"))
                //Constants.ExcelTypes.Any(ftt => fileName.ToLower().Contains(ftt)))    // TODO : get excel docs converting to DocX
                return true;
            return false;
        }

        public PdfDocument ToPdfDocument(ContentForPdf content)
        {
            Console.WriteLine("Handling Excel file type case ...");

            if (content.Type != ContentForPdf.ContentType.DataAttachment)
                throw new Exception("Converted didn't see the expected type for this conversion. Make sure you set the Attachment.");

            var renderer = new ChromePdfRenderer();

            var sb = new StringBuilder();
            // we need IronXL (or some open source stuff Lata is gathering feedback on)
            sb.AppendLine("<html><body><p><h1>Explicitly Unsupported File Type</h1></p>");
            sb.AppendLine("<p>We can't directly handle that data type without an additional framework.</p>");
            sb.AppendLine("<p>For now you can save .doc files (old Word format) to .docx.</p>");
            sb.AppendLine("<p>FYI : files with this type have been found to exist in the eGrants database.</p>");
            sb.AppendLine($"<p>The offending file's name is : {content.Attachment.FileName}</p></body></html>");

            using (var pdfDocument = renderer.RenderHtmlAsPdf(sb.ToString()))
            {
                using (var memoryStream = new MemoryStream())
                {
                    pdfDocument.Stream.CopyTo(memoryStream);

                    var bytes = memoryStream.ToArray();
                    var pdfDocFromStream = new PdfDocument(bytes);
                    return pdfDocFromStream;
                }
            }
        }
    }
}
