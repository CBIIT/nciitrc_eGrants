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
    internal class UnrecognizedTextConverter : IUnrecognizedTextConverter
    {
        public bool SupportsThisFileType(string fileName)
        {
            // MLH : this should be a last resort, handled intelligently by the parent process
            // This is because it's pretty hard to explicitly blacklist every unrecognizable thing that could show up

            if (!string.IsNullOrWhiteSpace(fileName))
                return true;
            return false;
        }

        public PdfDocument ToPdfDocument(ContentForPdf content)
        {
            Console.WriteLine("Handling unrecognized file type case ...");

            Console.WriteLine($"Here's the message text: {content.SimpleMessage}");

            if (content.Type != ContentForPdf.ContentType.DataAttachment)
                throw new Exception("Converted didn't see the expected type for this conversion. Make sure you set the SimpleGeneratedMessage.");

            var sb = new StringBuilder();

            switch (content.Type)
            {
                case ContentForPdf.ContentType.DataAttachment:
                    sb.AppendLine("<html><body><p><h1>Completely Unrecognized Filetype</h1></p>");
                    sb.AppendLine("<p>A file was discovered with an extension that was completely unanticipated.</p>");
                    sb.AppendLine("<p>Please notify the eGrants development team at egrantsdev@mail.nih.gov</p>");
                    sb.AppendLine("<p>And include as an attachment the file (or email) that this came from.</p>");
                    sb.AppendLine($"<p>And include the offending file's name is : {content.Attachment.FileName}</p></body></html>");
                    break;
                case ContentForPdf.ContentType.SimpleGeneratedMessage:
                    sb.AppendLine("<html><body><p><h1>Completely Unrecognized Filetype (top level failure)</h1></p>");
                    sb.AppendLine($"<p>{content.SimpleMessage}</p></body></html>");
                    break;
            }

            var renderer = new ChromePdfRenderer();

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
