using EmailConcatenationPOC.Interfaces;
using IronPdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EmailConcatenationPOC.Converters
{
    internal class UndiscoveredTextConverter : IUndiscoveredTextConverter
    {
        public bool SupportsThisFileType(string fileName)
        {
            if (fileName.ToLower().Contains(".") &&
                Constants.UndiscoveredFileTypes.Any(ftt => fileName.ToLower().Contains(ftt)))
                return true;
            return false;
        }

        public PdfDocument ToPdfDocument(ContentForPdf content)
        {
            Console.WriteLine("Handling undiscovered file type case ...");

            if (content.Type != ContentForPdf.ContentType.DataAttachment)
                throw new Exception("Converted didn't see the expected type for this conversion. Make sure you set the DataAttachment.");

            var sb = new StringBuilder();
            sb.AppendLine("<html><body><p><h1>Undiscovered Filetype</h1></p>");
            sb.AppendLine("<p>A file was discovered with an extension that is not supported due to its rarity.</p>");
            sb.AppendLine("<p>FYI : files with this type have been found to exist in the eGrants database.</p>");
            sb.AppendLine("<p>This file might just belong in a museum ! :)</p>");
            sb.AppendLine("<p>Please notify the eGrants development team at egrantsdev@mail.nih.gov</p>");
            sb.AppendLine("<p>And include as an attachment the file (or email) that this came from.</p>");
            sb.AppendLine($"<p>And include the offending file's name is : {content.Attachment.FileName}</p></body></html>");

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
