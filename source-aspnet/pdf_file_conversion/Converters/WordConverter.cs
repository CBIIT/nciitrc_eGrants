using EmailConcatenation.Interfaces;
using IronPdf;
using IronPdf.Engines.Chrome;

using NPOI.OpenXml4Net.OPC;
using NPOI.OpenXmlFormats.Wordprocessing;
using NPOI.SS.Formula;
using NPOI.XWPF.UserModel;

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

            if (content.GetBytes().Length == 0)
                return null;

            DocxToPdfRenderer docXRenderer = new DocxToPdfRenderer();

            try
            {
                bool IsPortrait = DocXIsPortrait(content);
                docXRenderer.RenderingOptions.PaperOrientation = IsPortrait ? IronPdf.Rendering.PdfPaperOrientation.Portrait : IronPdf.Rendering.PdfPaperOrientation.Landscape;
            } catch(ArgumentException) { 
                // MLH : this is an edge case where we read "the end was not found" (?)
                // Best way to handle is probably to just swallow the exception and take default IronPdf inferred margin
            }

            PdfDocument pdfDocument = docXRenderer.RenderDocxAsPdf(content.GetBytes());
            return new List<PdfDocument> { pdfDocument };
        }

        private bool DocXIsPortrait(ContentForPdf content)
        {
            OPCPackage pkg = null;

            if (content.IsMemoryStream && content.MemoryStream != null)
            {
                pkg = OPCPackage.Open(content.MemoryStream);
            } else
            {
                pkg = OPCPackage.Open(new MemoryStream(content.GetBytes()));
            }
            var doc = new XWPFDocument(pkg);

            foreach (var paragraph in doc.Paragraphs)
            {
                var ctp = paragraph.GetCTP();
                if (ctp.IsSetPPr())
                {
                    var ppr = ctp.pPr;
                    if (ppr != null)
                    {
                        var sectPtr = ppr.sectPr;
                        if (sectPtr != null)
                        {
                            var pageSize = sectPtr.pgSz;
                            if (pageSize != null)
                            {
                                if (pageSize.orient == ST_PageOrientation.portrait)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            // check body
            var body = doc.Document.body;
            if (body.sectPr != null)
            {
                var sectPr = body.sectPr;
                if (sectPr != null)
                {
                    var pageSz = sectPr.pgSz;
                    if (pageSz != null)
                    {
                        if (pageSz.orient == ST_PageOrientation.portrait)
                            return true;
                    }
                }
            }

            // MLH : return true if any are portrait since we can only set it in the IronPdf DocX converter in one place

            return false;
        }
    }
}
