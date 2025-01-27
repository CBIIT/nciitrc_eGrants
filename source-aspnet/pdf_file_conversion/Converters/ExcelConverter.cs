using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;

using EmailConcatenation.Interfaces;
using IronPdf;
using MathNet.Numerics;

using MsgReader.Outlook;
using NPOI.HSSF;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Interop;
using static System.Net.WebRequestMethods;
using CellType = NPOI.SS.UserModel.CellType;

namespace EmailConcatenation.Converters
{
    internal class ExcelConverter : IExcelConverter, IConvertToPdf
    {
        private readonly string DYNAMIC_SECTION_TAG = "|| Here is where the generated styles go ||";
        private Dictionary<string, EGStyle> cssToClass = new Dictionary<string, EGStyle>();
        int ROW_BREAK_INTERVAL = 200;

        public bool SupportsThisFileType(string fileName)
        {
            if (!string.IsNullOrWhiteSpace(fileName) &&
                (fileName.ToLower().EndsWith(".xls") ||
                fileName.ToLower().EndsWith(".xlsx") ||
                fileName.ToLower().EndsWith(".xlt")))
                return true;
            return false;
        }
        public List<PdfDocument> ToPdfDocument(ContentForPdf content)
        {
            Console.WriteLine("Handling Excel file type case ...");

            var allSheetsAsSeparatePdfs = new List<PdfDocument>();

            if (content.Type != ContentForPdf.ContentType.DataAttachment)
                throw new Exception("Converted didn't see the expected type for this conversion. Make sure you set the Attachment.");

            var sb = new StringBuilder();

            //            using (FileStream fileStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))

            // top formatting features :
            // 1    cell borders, shading (is that like highlighting?) (supported!)
            // 2    Conditional formatting (don't support this)
            // 3    Number formatting (does this just happen for free already?)
            // 4    Font styles and sizes (forget the styles !! not portable!)
            // 5    Alignment and text wrapping

            //if (content.Attachment.FileName == "workbook_calcs.xlsx")
            //{
            //    Console.WriteLine("ahoy !");
            //}

            try
            {
                if (!content.IsMemoryStream)
                {
                    using (var memoryStream = new MemoryStream(content.Attachment.Data))
                    {
                        ConvertStream(content.Attachment.FileName, allSheetsAsSeparatePdfs, memoryStream);
                    }
                } else
                {
                    ConvertStream(content.SingleFileFileName, allSheetsAsSeparatePdfs, content.MemoryStream);
                }
            }
            catch (OldExcelFormatException)
            {
                sb.Clear();
                sb.AppendLine("<html><body><p><h1>Old Pre-1997 Excel Format File Detected</h1></p>");

                sb.AppendLine("<p>A file was discovered that seems to be Excel 5.0/7.0 (BIFF5) format.</p>");
                sb.AppendLine("<p>This software does not contain the necessary frameworks to convert this file to PDF.</p>");
                sb.AppendLine("<p>Please convert the file to docx or BIFF8 format (from Excel versions 97/2000/XP/2003) before converting.</p>");
                sb.AppendLine($"<p>The offending file's name is : {content.Attachment.FileName}</p></body></html>");
            }
            
            var renderer = new ChromePdfRenderer();
            renderer.RenderingOptions.ForcePaperSize = true;
            if (sb != null && sb.Length > 0)
            {
                using (var pdfDocument = renderer.RenderHtmlAsPdf(sb.ToString()))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        pdfDocument.Stream.CopyTo(memoryStream);

                        var bytes = memoryStream.ToArray();
                        var pdfDocFromStream = new PdfDocument(bytes);
                        allSheetsAsSeparatePdfs.Add(pdfDocFromStream);
                        //return new List<PdfDocument> { pdfDocFromStream };
                    }
                }
            }
            return allSheetsAsSeparatePdfs;
        }

        private void ConvertStream(string fileName, List<PdfDocument> allSheetsAsSeparatePdfs, MemoryStream memoryStream)
        {
            var sb = new StringBuilder();
            if (fileName.ToLower().EndsWith(".xlsx"))
            {
                //sb.AppendLine(GetCSSClasses());
                XSSFWorkbook workbook = new XSSFWorkbook(memoryStream);
                XSSFFormulaEvaluator formula = new XSSFFormulaEvaluator(workbook);

                for (int i = 0; i < workbook.NumberOfSheets; i++)
                {
                    var sheet = workbook.GetSheetAt(i);

                    Console.WriteLine($"Handling sheet : {i + 1}");
                    int rowBreakCountdown = ROW_BREAK_INTERVAL;
                    sb.AppendLine(GetCSSClasses());
                    sb.AppendLine("<div id=\"parentDiv\" style=\"max-width: 1300px;overflow: hidden;transform-origin: top left;\">");
                    sb.AppendLine("<div id=\"childDiv\" style=\"width: 100%\">");

                    sb.AppendLine("<table style=\"border-collapse: collapse;\">");
                    for (int row = 0; row <= sheet.LastRowNum; row++)
                    {
                        var currentRow = sheet.GetRow(row);
                        if (currentRow != null)
                        {
                            sb.Append("<tr>");
                            for (int col = 0; col < currentRow.LastCellNum; col++)
                            {
                                var cell = currentRow.GetCell(col);
                                if (cell != null)
                                {
                                    ICellStyle cellStyle = cell.CellStyle;

                                    var classes = GetAlignmentClasses(cellStyle);

                                    // MLH : is there a way to handle cellStyle.WrapText == false ?
                                    // floating divs maybe ? sounds risky

                                    var contentText = cell.ToString();
                                    if (contentText.Contains("3.14"))
                                    {
                                        //                  Console.WriteLine("ahoy !");
                                    }

                                    //new : var egStyle = GetFormat(cellStyle, true, workbook);
                                    //StringBuilder formatBuilder = GetFormat(cellStyle, true, null);
                                    var egStyle = GetFormat(cellStyle, true, null);
                                    AddOrReuseAStyleClass(egStyle, classes);

                                    IDataFormat dataFormat = workbook.CreateDataFormat();
                                    string formatString = dataFormat.GetFormat(cellStyle.DataFormat);

                                    formula.EvaluateInCell(cell);


                                    string cellVal = GetFormattedCellValue(cell, workbook);
                                    sb.Append($"<td class=\"{string.Join(" ", classes)}\" >{cellVal}</td>");
                                }
                                else
                                {
                                    // it's null, but add placeholder table data
                                    sb.Append("<td></td>");
                                }
                            }
                            sb.Append("</tr>");

                            // write this to the PDF list if its getting too long (and reset everything)
                            rowBreakCountdown--;
                            if (rowBreakCountdown <= 0)
                            {
                                Console.WriteLine($"At row {row} this sheet is getting pretty full ... so writing to a new PDF");
                                // make the PDF for this sheet ALONE (so we don't run out of memory)
                                sb.AppendLine("</table></div></div>");
                                sb.AppendLine(GetJavaScript());
                                sb.AppendLine("</body></html>");
                                var subPdfThisSheet = WriteBufferToPdf(sb, cssToClass);
                                allSheetsAsSeparatePdfs.Add(subPdfThisSheet);
                                // reset everything
                                sb.Clear();
                                cssToClass.Clear();

                                sb.AppendLine(GetCSSClasses());     // start a new HTML / body

                                rowBreakCountdown = ROW_BREAK_INTERVAL;
                            }
                        }
                    }
                    sb.AppendLine("</table></div></div>");
                    sb.AppendLine(GetJavaScript());
                    sb.AppendLine("</body></html>");

                    // make the PDF for anything remaining 
                    var pdfThisSheet = WriteBufferToPdf(sb, cssToClass);
                    allSheetsAsSeparatePdfs.Add(pdfThisSheet);
                    // reset everything
                    sb.Clear();
                    cssToClass.Clear();
                } // cycle through all sheets
            }
            else  //    .xls
            {
                HSSFWorkbook workbook = new HSSFWorkbook(memoryStream);
                HSSFFormulaEvaluator formula = new HSSFFormulaEvaluator(workbook);

                // MLH : note that if for whatever this should happen on a cell by cell basis,
                // use formula.EvaluateInCell(cell); and render by cell.CellType
                formula.EvaluateAll();

                for (int i = 0; i < workbook.NumberOfSheets; i++)
                {
                    var sheet = workbook.GetSheetAt(i);             // TODO : support multiple sheets

                    Console.WriteLine($"Handling sheet : {i + 1}");
                    int rowBreakCountdown = ROW_BREAK_INTERVAL;
                    sb.AppendLine(GetCSSClasses());
                    sb.AppendLine("<div id=\"parentDiv\" style=\"max-width: 1300px;overflow: hidden;transform-origin: top left;\">");
                    sb.AppendLine("<div id=\"childDiv\" style=\"width: 100%\">");
                    sb.AppendLine("<table style=\"border-collapse: collapse;\">");
                    for (int row = 0; row <= sheet.LastRowNum; row++)
                    {
                        var currentRow = sheet.GetRow(row);
                        if (currentRow != null)
                        {
                            sb.Append("<tr>");
                            for (int col = 0; col < currentRow.LastCellNum; col++)
                            {
                                var cell = currentRow.GetCell(col);
                                if (cell != null)
                                {
                                    string formatString = String.Empty;
                                    ICellStyle cellStyle = cell.CellStyle;
                                    var classes = GetAlignmentClasses(cellStyle);

                                    var contentText = cell.ToString();
                                    if (contentText.Contains("Red text"))
                                    {
                                        Console.WriteLine("ahoy !");
                                    }

                                    //var formatBuilder = GetFormat(cellStyle, false, workbook);

                                    var egStyle = GetFormat(cellStyle, false, workbook);
                                    AddOrReuseAStyleClass(egStyle, classes);

                                    IDataFormat dataFormat = workbook.CreateDataFormat();
                                    formatString = dataFormat.GetFormat(cellStyle.DataFormat);
                                    formula.EvaluateInCell(cell);
                                    sb.Append($"<td class=\"{string.Join(" ", classes)}\" >{cell.ToString()}</td>");
                                }
                                else
                                {
                                    // it's null, but add placeholder table data
                                    sb.Append("<td></td>");
                                }
                            }
                            sb.Append("</tr>");

                            // write this to the PDF list if its getting too long (and reset everything)
                            rowBreakCountdown--;
                            if (rowBreakCountdown <= 0)
                            {
                                Console.WriteLine($"At row {row} this sheet is getting pretty full ... so writing to a new PDF");
                                // make the PDF for this sheet ALONE (so we don't run out of memory)
                                sb.AppendLine("</table></div></div>");
                                sb.AppendLine(GetJavaScript());
                                sb.AppendLine("</body></html>");
                                var subPdfThisSheet = WriteBufferToPdf(sb, cssToClass);
                                allSheetsAsSeparatePdfs.Add(subPdfThisSheet);
                                // reset everything
                                sb.Clear();
                                cssToClass.Clear();

                                sb.AppendLine(GetCSSClasses());     // start a new HTML / body

                                rowBreakCountdown = ROW_BREAK_INTERVAL;
                            }
                        }
                    }
                    sb.AppendLine("</table></div></div>");
                    sb.AppendLine(GetJavaScript());
                    sb.AppendLine("</body></html>");

                    // make the PDF for anything remaining 
                    var pdfThisSheet = WriteBufferToPdf(sb, cssToClass);
                    allSheetsAsSeparatePdfs.Add(pdfThisSheet);
                    // reset everything
                    sb.Clear();
                    cssToClass.Clear();
                }
            }
        }

        private string GetJavaScript()
        {
            // this is to dynamically fit the content so IronPdf doesn't clear the overflow
            var sb = new StringBuilder();
            sb.Append("<script>");
            sb.Append("function scaleContent() {");
            sb.Append("const parentDiv = document.getElementById('parentDiv');");
            sb.Append("const childDiv = document.getElementById('childDiv');");
            sb.Append("const parentWidth = parentDiv.offsetWidth;");
            sb.Append("const childWidth = childDiv.scrollWidth;");
            sb.Append("const scaleFactor = parentWidth / childWidth;");
            sb.Append("const expansionFactor = 0.5;");
            sb.Append("const scaleString = `scale(${scaleFactor * expansionFactor})`;");
            sb.Append("childDiv.style.transform = scaleString;");
            sb.Append("childDiv.style.transformOrigin = 'top left';");
            sb.Append("}");
            sb.Append("window.onload = scaleContent;");
            sb.Append("window.onresize = scaleContent;");
            sb.Append("</script>");
            return sb.ToString();
        }

        private PdfDocument WriteBufferToPdf(StringBuilder sb, Dictionary<string, EGStyle> cssToClass)
        {
            // add in the dynamic classes
            var dynamicClasses = new StringBuilder();
            foreach (var customStyle in cssToClass.Keys)
            {
                var styleElement = cssToClass[customStyle];
                dynamicClasses.Append(styleElement.RenderFullCssWithName());
            }
            var finalML = sb.ToString().Replace(DYNAMIC_SECTION_TAG, dynamicClasses.ToString());

            // MLH : diagnostics, delete later :
            //File.WriteAllText(".\\giantNew.html", finalML);

            var renderer = new ChromePdfRenderer();
            renderer.RenderingOptions.ForcePaperSize = true;
            renderer.RenderingOptions.Timeout = 5 * 60 * 1000;      // 5 minutes

            // MLH : this might be necessary for PDF is blank or incomplete or 
            //renderer.RenderingOptions.WaitFor.RenderDelay(5 * 60 * 1000);   // 5 minutes

            var watch = System.Diagnostics.Stopwatch.StartNew();
            // the code that you want to measure comes here

            //using (var pdfDocument = renderer.RenderHtmlAsPdf(sb.ToString()))
            Console.WriteLine($"Total chars : {finalML.Length}");
            using (var pdfDocument = renderer.RenderHtmlAsPdf(finalML))
            {
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine($"Elapsed milliseconds: {elapsedMs}");

                using (var memoryStream2 = new MemoryStream())
                {
                    pdfDocument.Stream.CopyTo(memoryStream2);

                    var bytes = memoryStream2.ToArray();
                    var pdfDocFromStream = new PdfDocument(bytes);
                    return pdfDocFromStream;
                    //allSheetsAsSeparatePdfs.Add(pdfDocFromStream);
                    //return new List<PdfDocument> { pdfDocFromStream };
                }
            }
        }

        private void AddOrReuseAStyleClass(EGStyle newStyle, List<string> classesThisTd)
        {
            var candidateCss = newStyle.RenderClassBodyToString();
            if (cssToClass.ContainsKey(candidateCss))               // this is an exact match check
            {
                // already there ... lets use that one
                var preExistingClass = cssToClass[candidateCss];
                classesThisTd.Add(preExistingClass.GetName());
            }
            else
            {
                cssToClass.Add(candidateCss, newStyle);
                classesThisTd.Add(newStyle.GetName());
            }
        }

        private string GetFormattedCellValue(ICell cell, IWorkbook workbook)
        {
            if (cell.CellType == CellType.Numeric)
            {
                if (DateUtil.IsCellDateFormatted(cell))
                {
                    cell.DateCellValue?.ToString("MM/dd/yyyy");
                } else
                {
                    ICellStyle style = cell.CellStyle;
                    IDataFormat format = workbook.CreateDataFormat();
                    string formatString = style.GetDataFormatString();

                    if (formatString == "General")
                    {
                        return cell.NumericCellValue.ToString("G", CultureInfo.InvariantCulture);
                    }
                    else if (formatString != null)
                    {
                        return cell.NumericCellValue.ToString(formatString, CultureInfo.InvariantCulture);
                    } else
                    {
                        return cell.NumericCellValue.ToString(CultureInfo.InvariantCulture);
                    }
                }
            } else if (cell.CellType == CellType.String)
            {
                return cell.StringCellValue;
            } else if (cell.CellType == CellType.Boolean)
            {
                return cell.BooleanCellValue.ToString();
            } else if (cell.CellType == CellType.Formula)
            {
                return cell.CellFormula;
            }
            return cell.ToString();
        }

        private string GetCSSClasses()
        {
            return "<html><head><style>" +
                        ".page-break { page-break-before:always; } " +
                        ".center {\r\n  text-align: center;\r\n } " +
                        ".left {\r\n  text-align: left;\r\n } " +
                        ".right {\r\n  text-align: right;\r\n } " +
                        DYNAMIC_SECTION_TAG +
                        "</style></head><body>";
        }

        private EGStyle GetFormat(ICellStyle cellStyle, bool isXlsx, HSSFWorkbook workbook)
        {
            var egStyle = new EGStyle();
//            var formatBuilder = new StringBuilder();
            if (cellStyle.BorderTop != 0)
            {
                egStyle.borderTop = true;
//                formatBuilder.Append("border-top: 1px solid black;");
            }
            if (cellStyle.BorderLeft != 0)
            {
                egStyle.borderLeft = true;
//                formatBuilder.Append("border-left: 1px solid black;");
            }
            if (cellStyle.BorderRight != 0)
            {
                egStyle.borderRight = true;
//                formatBuilder.Append("border-right: 1px solid black;");
            }
            if (cellStyle.BorderBottom != 0)
            {
                egStyle.borderBottom = true;
   //             formatBuilder.Append("border-bottom: 1px solid black;");
            }
            if (cellStyle.FillForegroundColorColor != null)
            {
                // this is a byte[3] with each byte correspondin to R, G, and B
                var fgColor = cellStyle.FillForegroundColorColor.RGB;

                // MLH : for some reason on XLS files, default white background is presented as 0,0,0 which becomes black
                var convertedHexColor = BitConverter.ToString(fgColor).Replace("-", "");
                if (!isXlsx && convertedHexColor.Equals("000000"))
                    convertedHexColor = "FFFFFF";

                egStyle.backgroundColor = convertedHexColor;        // this is a "fill foreground" color = background (???)
                //formatBuilder.Append($"background-color: #{convertedHexColor};");
            }
            if (cellStyle.WrapText)
            {
                egStyle.wrapText = true;
                //formatBuilder.Append($"text-wrap: wrap;");
            }
            //else
            //{
            // MLH : this might be slowing things down to much and already true by default
            //    formatBuilder.Append($"text-wrap: nowrap;");
            //}

            bool isStrikeout = false;
            bool isItalic = false;
            bool isBold = false;
            bool isUnderline = false;
            string fontColor = "000000";
            double fontSize = 12.0;


            HSSFCellStyle src = cellStyle as HSSFCellStyle;
            if (src != null)
            {
                // xls
                var font = src.GetFont(workbook);

                isStrikeout = font.IsStrikeout;
                isItalic = font.IsItalic;
                isBold = font.IsBold;
                isUnderline = (font.Underline != FontUnderlineType.None);
                short colorIndex = font.Color;
                fontColor = ColorConverter.GetHexColor(colorIndex);
                fontSize = font.FontHeightInPoints;
            }
            else
            {   // xlsx
                XSSFCellStyle src2 = (XSSFCellStyle)cellStyle;
                if (src2 != null)
                {
                    var font = src2.GetFont();

                    isStrikeout = font.IsStrikeout;
                    isItalic = font.IsItalic;
                    isBold = font.IsBold;
                    isUnderline = (font.Underline != FontUnderlineType.None);
                    short colorIndex = font.Color;
                    var rgbColor = font.GetXSSFColor().RGB;
                    fontColor = BitConverter.ToString(rgbColor).Replace("-", "");
                    fontSize = font.FontHeightInPoints;
                }
            }

            egStyle.isBold = isBold;
            egStyle.isUnderline = isUnderline;
            egStyle.isStrikeout = isStrikeout;

            egStyle.fontColor = fontColor;
            egStyle.fontSize = fontSize;
            //egStyle.fontColor = ConvertColorToHexString(font.FontColor);
            //egStyle.backgroundColor = fontColor;
            //formatBuilder.Append($"font-size: {fontSize}px;");

            return egStyle;
        }

        public List<string> GetAlignmentClasses(ICellStyle cellStyle)
        {
            var classes = new List<string>();
            if (cellStyle.Alignment == HorizontalAlignment.Left)
            {
                classes.Add("left");
            }
            else if (cellStyle.Alignment == HorizontalAlignment.Right)
            {
                classes.Add("right");
            }
            if (cellStyle.Alignment == HorizontalAlignment.Center)
            {
                classes.Add("center");
            }
            return classes;
        }


    }
}
