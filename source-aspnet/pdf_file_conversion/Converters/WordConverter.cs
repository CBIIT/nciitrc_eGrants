using EmailConcatenation.Interfaces;
using IronPdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailConcatenation.Converters
{
    public class WordConverter : IWordConverter, IConvertToPdf
    {
        public bool SupportsThisFileType(string fileName)
        {
            if (!string.IsNullOrWhiteSpace(fileName) &&
                fileName.ToLower().EndsWith(".docx") )
                return true;
            return false;
        }

        public List<PdfDocument> ToPdfDocument(ContentForPdf content)
        {
            Console.WriteLine("Handling Word docx file type case ...");

            DocxToPdfRenderer docXRenderer = new DocxToPdfRenderer();

            if (content.GetBytes().Length == 0)
                return null;

            PdfDocument pdfDocument = docXRenderer.RenderDocxAsPdf(content.GetBytes());
            return new List<PdfDocument> { pdfDocument };
        }
    }
}
