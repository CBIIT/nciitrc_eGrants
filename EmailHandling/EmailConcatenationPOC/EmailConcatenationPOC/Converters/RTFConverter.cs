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
    public class RTFConverter : IRTFConverter, IConvertToPdf
    {
        public bool SupportsThisFileType(string fileName)
        {
            if (fileName.ToLower().EndsWith(".rtf"))
                return true;
            return false;
        }

        public PdfDocument ToPdfDocument(ContentForPdf content)
        {
            using (var memoryStream = new MemoryStream(content.Attachment.Data))
            {
                var htmlRenderer = new ChromePdfRenderer();
                using (StreamReader reader = new StreamReader(memoryStream))
                {
                    string rtfContent = reader.ReadToEnd();
                    PdfDocument pdf = htmlRenderer.RenderRtfStringAsPdf(rtfContent);
                    Console.WriteLine("adding rendered pdf");

                    // a lot of JUNK at the beginning that starts ... {\*\xmlnstbl {\xmlns1 http://schemas.microsoft.com/office/word/2003/wordml}}\paperw12240\
                    //      paperh15840\margl1440\margr1440\margt1440\margb1440\gutter0\ltrsect 
                    // a LOT of JUNK at the end that starts ... \par }{\*\themedata 504b03041400
                    
                    return pdf;
                }
            }
        }
    }
}
