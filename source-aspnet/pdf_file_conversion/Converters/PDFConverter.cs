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
    internal class PDFConverter : IPDFConverter, IConvertToPdf
    {
        public bool SupportsThisFileType(string fileName)
        {
            if (fileName.ToLower().EndsWith(".pdf"))
                return true;
            return false;
        }

        public List<PdfDocument> ToPdfDocument(ContentForPdf content)
        {
            using (var memoryStream = new MemoryStream(content.GetBytes()))
            {
                if (memoryStream.Length == 0)
                    return null;

                var newPdfFile = new PdfDocument(memoryStream);
                newPdfFile.SecuritySettings.AllowUserFormData = false;    // Disable form editing
                newPdfFile.SecuritySettings.AllowUserAnnotations = true;   // Allow commends and highlights
                newPdfFile.Flatten();
                return new List<PdfDocument> { newPdfFile };
            }
        }
    }
}
