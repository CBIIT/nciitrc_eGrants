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
    public class HtmlConverter : IHtmlConverter, IConvertToPdf
    {
        public bool SupportsThisFileType(string fileName)
        {
            if (fileName.EndsWith(".html") ||
                  fileName.ToLower().EndsWith(".htm") )
                return true;
            return false;
        }

        public List<PdfDocument> ToPdfDocument(ContentForPdf content)
        {
            using (var memoryStream = new MemoryStream(content.Attachment.Data))
            {
                var htmlRenderer = new ChromePdfRenderer();

                using (StreamReader reader = new StreamReader(memoryStream))
                {
                    string htmlContent = reader.ReadToEnd();

                    // Render the HTML content as a PDF
                    PdfDocument pdf = htmlRenderer.RenderHtmlAsPdf(htmlContent);
                    Console.WriteLine("adding rendered pdf");
                    return new List<PdfDocument> { pdf };
                }
            }
        }
    }
}
