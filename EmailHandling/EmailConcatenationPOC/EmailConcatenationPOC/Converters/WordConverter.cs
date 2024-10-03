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
    public class WordConverter : IWordConverter, IConvertToPdf
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
            Console.WriteLine("Handling Word docx file type case ...");

            DocxToPdfRenderer docXRenderer = new DocxToPdfRenderer();
            PdfDocument pdfDocument = docXRenderer.RenderDocxAsPdf(content.Attachment.Data);
            return pdfDocument;
        }
    }
}
