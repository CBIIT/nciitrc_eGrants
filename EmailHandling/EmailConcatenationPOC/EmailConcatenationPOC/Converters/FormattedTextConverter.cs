using EmailConcatenationPOC.Interfaces;
using IronPdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EmailConcatenationPOC.Converters
{
    internal class FormattedTextConverter : IFormattedTextConverter, IConvertToPdf
    {
        public bool SupportsThisFileType(string fileName)
        {
            if (fileName.ToLower().Contains(".") &&
                Constants.FormattedTextTypes.Any(ftt => fileName.ToLower().Contains(ftt)) )
                return true;
            return false;
        }

        static string RemoveImagesFromHtmlText(string content)
        {
            if (string.IsNullOrWhiteSpace(content) ||
                !content.Contains("<html"))
            {
                return content;
            }

            string output = Regex.Replace(content, @"<img(.)*?>", " ");

            return output;
        }

        public PdfDocument ToPdfDocument(ContentForPdf content)
        {
            Console.WriteLine("Handling general image case ...");

            if (content.IsMessage)
            {
                Console.WriteLine($"Here's the message text: {content.SimpleMessage}");

                if (content.Type != ContentForPdf.ContentType.SimpleGeneratedMessage)
                    throw new Exception("Converted didn't see the expected type for this conversion. Make sure you set the SimpleGeneratedMessage.");

                var renderer = new ChromePdfRenderer();

                var imagesRemovedContent = RemoveImagesFromHtmlText(content.SimpleMessage);

                using (var pdfDocument = renderer.RenderHtmlAsPdf(imagesRemovedContent))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        pdfDocument.Stream.CopyTo(memoryStream);

                        var bytes = memoryStream.ToArray();
                        var pdfDocFromStream = new PdfDocument(bytes);
                        return pdfDocFromStream;
                    }
                }
            } else
            {
                using (var memoryStream = new MemoryStream(content.Attachment.Data))
                {
                    var htmlRenderer = new ChromePdfRenderer();

                    using (StreamReader reader = new StreamReader(memoryStream))
                    {
                        string htmlContent = reader.ReadToEnd();

                        htmlContent = RemoveImagesFromHtmlText(htmlContent);

                        // Render the HTML content as a PDF
                        PdfDocument pdf = htmlRenderer.RenderHtmlAsPdf(htmlContent);
                        Console.WriteLine("adding rendered pdf");
                        return pdf;
                    }
                }
            }
        }
    }
}
