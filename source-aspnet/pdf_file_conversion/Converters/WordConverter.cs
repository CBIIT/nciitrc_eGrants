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

        private bool _isPortrait;

        // margin info
        private const ulong _defaultBottom = 1440;
        private const ulong _defaultFooter = 720;
        private const ulong _defaultGutter = 0;
        private const ulong _defaultHeader = 720;
        private const ulong _defaultLeft = 1440;
        private const ulong _defaultRight = 1440;
        private const ulong _defaultTop = 1440;

        private ulong _bottom = _defaultBottom;
        private ulong _footer = _defaultFooter;
        private ulong _gutter = _defaultGutter;
        private ulong _header = _defaultHeader;
        private ulong _left = _defaultLeft;
        private ulong _right = _defaultRight;
        private ulong _top = _defaultTop;

        public List<PdfDocument> ToPdfDocument(ContentForPdf content)
        {
            Console.WriteLine("Handling Word docx file type case ...");

            ScanDimensions(content);

            DocxToPdfRenderer docXRenderer = new DocxToPdfRenderer();

            if (content.GetBytes().Length == 0)
                return null;

            docXRenderer.RenderingOptions.PaperOrientation = _isPortrait ? IronPdf.Rendering.PdfPaperOrientation.Portrait : IronPdf.Rendering.PdfPaperOrientation.Landscape;

            docXRenderer.RenderingOptions.MarginTop = _top;
            docXRenderer.RenderingOptions.MarginBottom = _bottom;
            docXRenderer.RenderingOptions.MarginLeft = _left;
            docXRenderer.RenderingOptions.MarginRight = _right;

            PdfDocument pdfDocument = docXRenderer.RenderDocxAsPdf(content.GetBytes());
            return new List<PdfDocument> { pdfDocument };
        }

        private void UpdateMarginInfoHere(CT_PageMar marginInfo)
        {
            if (marginInfo == null) return;

            if (marginInfo.bottom != _defaultBottom)
                _bottom = marginInfo.bottom;
            if (marginInfo.bottom != _defaultBottom)
                _bottom = marginInfo.footer;
            if (marginInfo.bottom != _defaultBottom)
                _bottom = marginInfo.gutter;
            if (marginInfo.bottom != _defaultBottom)
                _bottom = marginInfo.header;
            if (marginInfo.bottom != _defaultBottom)
                _bottom = marginInfo.left;
            if (marginInfo.bottom != _defaultBottom)
                _bottom = marginInfo.right;
            if (marginInfo.bottom != _defaultBottom)
                _bottom = marginInfo.top;
        }

        private void ScanDimensions(ContentForPdf content)
        {
            OPCPackage pkg = null;

            _isPortrait = false;

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
                            UpdateMarginInfoHere(sectPtr.pgMar);
                            var pageSize = sectPtr.pgSz;
                            if (pageSize != null)
                            {
                                if (pageSize.orient == ST_PageOrientation.portrait)
                                {
                                    _isPortrait = true;
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
                    UpdateMarginInfoHere(sectPr.pgMar);
                    var pageSz = sectPr.pgSz;
                    if (pageSz != null)
                    {
                        if (pageSz.orient == ST_PageOrientation.portrait)
                            _isPortrait = true;
                    }
                }
            }
        }
    }
}
