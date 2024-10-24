using IronPdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailConcatenation
{
    public interface IConvertToPdf
    {
        bool SupportsThisFileType(string fileName);

        List<PdfDocument> ToPdfDocument(ContentForPdf content);
    }
}
