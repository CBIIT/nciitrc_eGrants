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
    internal class EmailTextConverter : IEmailTextConverter, IConvertToPdf
    {
        public bool SupportsThisFileType(string fileName)
        {
            // MLH : we shouldn't need this check much because these kinds of files are picked up outside of the regular attachment process

            if (fileName.ToLower().Contains(".") && fileName.ToLower().Contains(".msg"))
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
            Console.WriteLine("Handling email message case ...");

            if (content.Type != ContentForPdf.ContentType.MailMessage)
                throw new Exception("Converted didn't see the expected type for this conversion. Make sure you set the MailMessage.");

            var renderer = new ChromePdfRenderer();

            var messageTextAsHtml = content.Message.BodyText;
            var messageHtmlAsHtml = content.Message.BodyHtml;

            var imagesRemovedContent = RemoveImagesFromHtmlText(messageHtmlAsHtml);

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
        }
    }
}
