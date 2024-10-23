using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml.Office;
using EmailConcatenationPOC.Interfaces;
using IronPdf;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EmailConcatenationPOC.Converters
{
    internal class ExcelXLSMConverter : IExcelXLSMConverter, IConvertToPdf
    {
        private readonly string DYNAMIC_SECTION_TAG= "|| Here is where the generated styles go ||";
        private Dictionary<string, EGStyle> cssToClass = new Dictionary<string, EGStyle>();

        public bool SupportsThisFileType(string fileName)
        {
            if (!string.IsNullOrWhiteSpace(fileName) &&
                (fileName.ToLower().EndsWith(".xlsm")))
                return true;
            return false;
        }
        public List<PdfDocument> ToPdfDocument(ContentForPdf content)
        {
            Console.WriteLine("Handling Excel file type case ...");

            int row_break_interval = 200;

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

            try
            {
                using (var memoryStream = new MemoryStream(content.Attachment.Data))
                {

                    

                    var workbook = new XLWorkbook(memoryStream);

                    //var ws1 = workbook.Worksheet(1);

                    //XSSFWorkbook workbook = new XSSFWorkbook(memoryStream);

                    //for (int i = 0; i < workbook.Worksheets.Count .NumberOfSheets; i++)
                    foreach(var sheet in workbook.Worksheets)
                    {
                        Console.WriteLine($"Handling sheet : {sheet.Name}");
                        int rowBreakCountdown = row_break_interval;
                        sb.AppendLine(GetCSSClasses());

                        var rowCount = sheet.LastRowUsed().RowNumber();
                        var columnCount = sheet.LastColumnUsed().ColumnNumber();
                        Console.WriteLine($"Total rows this sheet : {rowCount}");

                        int row = 1;

                        //var sheet = workbook.GetSheetAt(i);
                        sb.AppendLine("<table style=\"border-collapse: collapse;\">");
                        //                        for (int row = 0; row <= sheet.LastRowNum; row++)
                        //                      foreach(var currentRow in sheet.Rows())
                        while (row <= rowCount)
                        {
                            //var currentRow = sheet.GetRow(row);

                            sb.Append("<tr>");
                            //                                for (int col = 0; col < currentRow.LastCellNum; col++)

                            int column = 1;
                            //foreach(var cell in currentRow.Cells())
                            while (column <= columnCount)
                            {
                                var cell = sheet.Cell(row, column);
                                if (cell != null)
                                {
                                    var contentText = cell.Value.ToString();
                                    if (contentText.Contains("3.14159265359"))
                                    {
                     //                   Console.WriteLine("ahoy !");
                                    }

                                    //ICellStyle cellStyle = cell.CellStyle;
                                    var cellStyle = cell.Style;    
                                    var classes = GetAlignmentClasses(cellStyle);

                                    // MLH : is there a way to handle cellStyle.WrapText == false ?
                                    // floating divs maybe ? sounds risky

                                    //StringBuilder formatBuilder = GetFormat(cellStyle, true, workbook);
                                    var egStyle = GetFormat(cellStyle, true, workbook);
                                    AddOrReuseAStyleClass(egStyle, classes);

                            //        sb.Append($"<td class=\"{string.Join(";", classes)}\" style=\"{formatBuilder.ToString()}\">{cell.GetFormattedString()}</td>");
                                    sb.Append($"<td class=\"{string.Join(" ",classes)}\" >{cell.GetFormattedString()}</td>");
                                }
                                else
                                {
                                    // it's null, but add placeholder table data
                                    sb.Append("<td></td>");
                                }
                                column++;
                            }
                            sb.Append("</tr>");
                            row++;

                            // write this to the PDF list if its getting too long (and reset everything)
                            rowBreakCountdown--;
                            if (rowBreakCountdown <= 0)
                            {
                                Console.WriteLine($"At row {row} this sheet is getting pretty full ... so writing to a new PDF");
                                // make the PDF for this sheet ALONE (so we don't run out of memory)
                                sb.AppendLine("</table>");
                                sb.AppendLine("</body></html>");
                                var subPdfThisSheet = WriteBufferToPdf(sb, cssToClass);
                                allSheetsAsSeparatePdfs.Add(subPdfThisSheet);
                                // reset everything
                                sb.Clear();
                                cssToClass.Clear();

                                sb.AppendLine(GetCSSClasses());     // start a new HTML / body

                                rowBreakCountdown = row_break_interval;
                            }

                        }
                        sb.AppendLine("</table>");
                        sb.AppendLine("</body></html>");

                        // make the PDF for this sheet ALONE (so we don't run out of memory)
                        var pdfThisSheet = WriteBufferToPdf(sb, cssToClass);
                        allSheetsAsSeparatePdfs.Add(pdfThisSheet);
                        // reset everything
                        sb.Clear();
                        cssToClass.Clear();

                    } // end all sheets sheet 
                    
                    
                }
            }
            catch (Exception)
            {
                sb.Clear();
                sb.AppendLine("<html><body><p><h1>General Xlsm Render Exception</h1></p>");

                sb.AppendLine("<p>Hit a general exception while attempting to render a .xlsm file to PDF.</p>");
                sb.AppendLine($"<p>The offending file's name is : {content.Attachment.FileName}</p>");
            //    sb.AppendLine($"<p>Exception : {ex}</p>");
                sb.AppendLine($"</body></html>");
            }


            return allSheetsAsSeparatePdfs;
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
            } else
            {
                cssToClass.Add(candidateCss, newStyle);
                classesThisTd.Add(newStyle.GetName());
            }
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

//        private StringBuilder GetFormat(IXLStyle cellStyle, bool isXlsx, HSSFWorkbook workbook)
        private EGStyle GetFormat(IXLStyle cellStyle, bool isXlsx, XLWorkbook workbook)
        {
            var egStyle = new EGStyle();
            var formatBuilder = new StringBuilder();
            if (cellStyle.Border.TopBorder != XLBorderStyleValues.None)
            {
                egStyle.borderTop = true;
//                formatBuilder.Append("border-top: 1px solid black;");
            }
            if (cellStyle.Border.LeftBorder != XLBorderStyleValues.None)
            {
                egStyle.borderLeft = true;
                //formatBuilder.Append("border-left: 1px solid black;");
            }
            if (cellStyle.Border.RightBorder != XLBorderStyleValues.None)
            {
                egStyle.borderRight = true;
                //formatBuilder.Append("border-right: 1px solid black;");
            }
            if (cellStyle.Border.BottomBorder != XLBorderStyleValues.None)
            {
                egStyle.borderBottom = true;
                //formatBuilder.Append("border-bottom: 1px solid black;");
            }
            if (cellStyle.Fill != null && cellStyle.Fill.PatternType != null && cellStyle.Fill.PatternType != XLFillPatternValues.None)
            {
                var color = cellStyle.Fill.BackgroundColor;

                if (color.ColorType != XLColorType.Theme)
                {
                    if (!color.Color.Name.Equals("Transparent", StringComparison.InvariantCultureIgnoreCase))
                    {
                        formatBuilder.Append($"background-color: #{ConvertColorToHexString(color)};");
                        egStyle.backgroundColor = ConvertColorToHexString(color);
                    }
                } else
                {
                    // MLH : slows things down too much ?

                    var themeColor = color.ThemeColor;
                    XLColor rgb;
                    switch (themeColor)
                    {
                        case XLThemeColor.Accent1:
                            rgb = workbook.Theme.Accent1;
                            break;
                        case XLThemeColor.Accent2:
                            rgb = workbook.Theme.Accent2;
                            break;
                        case XLThemeColor.Accent3:
                            rgb = workbook.Theme.Accent3;
                            break;
                        case XLThemeColor.Accent4:
                            rgb = workbook.Theme.Accent4;
                            break;
                        case XLThemeColor.Accent5:
                            rgb = workbook.Theme.Accent5;
                            break;
                        case XLThemeColor.Accent6:
                            rgb = workbook.Theme.Accent6;
                            break;
                        case XLThemeColor.Background1:
                            rgb = workbook.Theme.Background1;
                            break;
                        case XLThemeColor.Background2:
                            rgb = workbook.Theme.Background2;
                            break;
                        case XLThemeColor.Text1:
                            rgb = workbook.Theme.Text1;
                            break;

                        default:
                            rgb = XLColor.FromName("White");
                            break;
                    }
//                    formatBuilder.Append($"background-color: #{ConvertColorToHexString(rgb)};");
                    egStyle.backgroundColor = ConvertColorToHexString(rgb);
                    //formatBuilder.Append($"background-color: #ffffff;");
                }

            }
            if (cellStyle.Alignment.WrapText)
            {
                egStyle.wrapText = true;
//                formatBuilder.Append($"text-wrap: wrap;");
            }
            //else
            //{
            //    // MLH : this might be slowing things down to much and already true by default
            //    formatBuilder.Append($"text-wrap: nowrap;");
            //}

            bool isStrikeout = false;
            bool isItalic = false;
            bool isBold = false;
            bool isUnderline = false;
            string fontColor = "000000";
            double fontSize = 12.0;

            var font = cellStyle.Font;
            isStrikeout = font.Strikethrough;
            isItalic = font.Italic;
            isBold = font.Bold;
            isUnderline = font.Underline != XLFontUnderlineValues.None;
            if (font.FontColor.ColorType == XLColorType.Color)
            {
                // fontColor = font.FontColor.Color.ToString();    // has alpha value
                //fontColor = ConvertColorToHexString(font.FontColor);
                egStyle.fontColor = ConvertColorToHexString(font.FontColor);
            } else if (font.FontColor.ColorType == XLColorType.Theme)
            {
                //fontColor = workbook.Theme.
                var themeColor = font.FontColor.ThemeColor;
                switch (themeColor)
                {
                    case XLThemeColor.Text1:
                        fontColor = ConvertColorToHexString(workbook.Theme.Text1);
                        break;
                    case XLThemeColor.Text2:
                        fontColor = ConvertColorToHexString(workbook.Theme.Text2);
                        break;
                    case XLThemeColor.Background1:
                        fontColor = ConvertColorToHexString(workbook.Theme.Background1);
                        break;
                    case XLThemeColor.Background2:
                        fontColor = ConvertColorToHexString(workbook.Theme.Background2);
                        break;
                    case XLThemeColor.Accent1:
                        fontColor = ConvertColorToHexString(workbook.Theme.Accent1);
                        break;
                    case XLThemeColor.Accent2:
                        fontColor = ConvertColorToHexString(workbook.Theme.Accent2);
                        break;
                    case XLThemeColor.Accent3:
                        fontColor = ConvertColorToHexString(workbook.Theme.Accent3);
                        break;
                    case XLThemeColor.Accent4:
                        fontColor = ConvertColorToHexString(workbook.Theme.Accent4);
                        break;
                    case XLThemeColor.Accent5:
                        fontColor = ConvertColorToHexString(workbook.Theme.Accent5);
                        break;
                    case XLThemeColor.Accent6:
                        fontColor = ConvertColorToHexString(workbook.Theme.Accent6);
                        break;

                }
                egStyle.fontColor = fontColor;
            }
//            fontSize = font.FontSize;
            egStyle.fontSize = font.FontSize;

            if (isBold)
            {
                egStyle.isBold = true;
//                formatBuilder.Append("font-weight: bold;");
            }
            if (isStrikeout)
                egStyle.isStrikeout = true;
            if (isUnderline)
                egStyle.isUnderline = true;
            //if (isStrikeout || isUnderline)
            //{
            //    var strikeout = isStrikeout ? "line-through" : "";
            //    var underline = (isUnderline) ? "underline" : "";
            //    formatBuilder.Append($"text-decoration: {strikeout}{underline};");
            //}
            if (isItalic)
            {
                egStyle.isItalic = true;
//                formatBuilder.Append("font-style: italic;");
            }

            //formatBuilder.Append($"color: #{fontColor};");
            //formatBuilder.Append($"font-size: {fontSize}px;");

            return egStyle;
//            return formatBuilder;
        }

        public string ConvertColorToHexString(XLColor color)
        {
            var colorString = $"{HexValueWithLeadingZero(color.Color.R)}" +
                $"{HexValueWithLeadingZero(color.Color.G)}" +
                $"{HexValueWithLeadingZero(color.Color.B)}";

            return colorString;
        }

        public string HexValueWithLeadingZero(byte value)
        {
            var rendered = value.ToString("X");
            if (rendered.Length == 1)
            {
                rendered = $"0{rendered}";
            }
            return rendered;
        }

        //public List<string> GetAlignmentClasses(ICellStyle cellStyle)
        public List<string> GetAlignmentClasses(IXLStyle cellStyle)
        {
            var classes = new List<string>();
            //cellStyle.
//             if (cellStyle.Alignment == IXLAlignment.Left)
            if (cellStyle.Alignment.Horizontal == XLAlignmentHorizontalValues.Left)
            {
                classes.Add("left");
            }
            else if (cellStyle.Alignment.Horizontal == XLAlignmentHorizontalValues.Right)
            {
                classes.Add("right");
            }
            if (cellStyle.Alignment.Horizontal == XLAlignmentHorizontalValues.Center)
            {
                classes.Add("center");
            }
            return classes;
        }


    }
}
