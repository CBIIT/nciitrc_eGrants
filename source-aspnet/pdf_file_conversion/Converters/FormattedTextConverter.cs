using EmailConcatenation.Interfaces;
using IronPdf;
using Markdig;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EmailConcatenation.Converters
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

        public List<PdfDocument> ToPdfDocument(ContentForPdf content)
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
                        return new List<PdfDocument> { pdfDocFromStream };
                    }
                }
            } else
            {
                var textContent = System.Text.Encoding.Default.GetString(content.Attachment.Data);

                // MLH : there is likely going to be some "markdown" here (e.g. \n to mean new line)
                // and this won't be natively rendered by the IronPDF chrome HTML converter
                textContent = Markdown.ToHtml(textContent);

                var renderer = new ChromePdfRenderer();

                var imagesRemovedContent = RemoveImagesFromHtmlText(textContent);

                using (var pdfDocument = renderer.RenderHtmlAsPdf(imagesRemovedContent))
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
}
