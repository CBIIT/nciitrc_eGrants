using IronPdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailConcatenationPOC
{
    public interface IConvertToPdf
    {
        bool SupportsThisFileType(string fileName);

        PdfDocument ToPdfDocument(ContentForPdf content);
    }
}
