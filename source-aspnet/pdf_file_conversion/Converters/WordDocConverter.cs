using EmailConcatenation.Interfaces;
using IronPdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spire.Doc;

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

            using (var memoryStream = new MemoryStream(content.Attachment.Data))
            {
                Document document = new Document(memoryStream);
                using (var pdfStream = new MemoryStream())
                {
                    document.SaveToStream(pdfStream, FileFormat.PDF);
                    pdfStream.Position = 0;
                    var newPdfFile = new PdfDocument(pdfStream);
                    return new List<PdfDocument> { newPdfFile };
                }
            }
        }
    }
}
