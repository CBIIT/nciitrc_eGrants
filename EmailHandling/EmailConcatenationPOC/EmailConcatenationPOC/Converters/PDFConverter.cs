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
    internal class PDFConverter : IPDFConverter, IConvertToPdf
    {
        public bool SupportsThisFileType(string fileName)
        {
            if (fileName.ToLower().EndsWith(".pdf"))
                return true;
            return false;
        }

        public PdfDocument ToPdfDocument(ContentForPdf content)
        {
            using (var memoryStream = new MemoryStream(content.Attachment.Data))
            {
                var newPdfFile = new PdfDocument(memoryStream);
                return newPdfFile;
            }
        }
    }
}
