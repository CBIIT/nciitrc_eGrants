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

            bool IsPortrait = DocXIsPortrait(content);

            DocxToPdfRenderer docXRenderer = new DocxToPdfRenderer();

            if (content.GetBytes().Length == 0)
                return null;

            docXRenderer.RenderingOptions.PaperOrientation = IsPortrait ? IronPdf.Rendering.PdfPaperOrientation.Portrait : IronPdf.Rendering.PdfPaperOrientation.Landscape;

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

            // MLH : return true if any are portrait ... circle back later and find a better approach

            return false;
        }
    }
}
