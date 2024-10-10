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
        public bool SupportsThisFileType(string fileName)
        {
            if (!string.IsNullOrWhiteSpace(fileName) &&
                (fileName.ToLower().EndsWith(".xlsm")))
                return true;
            return false;
        }
        public PdfDocument ToPdfDocument(ContentForPdf content)
        {
            Console.WriteLine("Handling Excel file type case ...");

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

                    sb.AppendLine(GetCSSClasses());

                    var workbook = new XLWorkbook(memoryStream);

                    //var ws1 = workbook.Worksheet(1);

                    //XSSFWorkbook workbook = new XSSFWorkbook(memoryStream);

                    //for (int i = 0; i < workbook.Worksheets.Count .NumberOfSheets; i++)
                    bool first = true;
                    foreach(var sheet in workbook.Worksheets)
                    {
                        if (!first)
                        {
                            sb.Append("<div class=\"page-break\"></div>");
                        }
                        first = false;

                        var rowCount = sheet.LastRowUsed().RowNumber();
                        var columnCount = sheet.LastColumnUsed().ColumnNumber();

                        
                        int row = 1;

                        //var sheet = workbook.GetSheetAt(i);
                        sb.AppendLine("<table>");
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
//                                    var cell = currentRow.GetCell(col);
                                if (cell != null)
                                {
                                    var contentText = cell.Value.ToString();
                                    if (contentText.Contains("3.14159265359"))
                                    {
                                        Console.WriteLine("ahoy !");
                                    }

                                    //ICellStyle cellStyle = cell.CellStyle;
                                    var cellStyle = cell.Style;    
                                    var classes = GetAlignmentClasses(cellStyle);

                                    // MLH : is there a way to handle cellStyle.WrapText == false ?
                                    // floating divs maybe ? sounds risky

                                    StringBuilder formatBuilder = GetFormat(cellStyle, true, workbook);

                                    sb.Append($"<td class=\"{string.Join(";",classes)}\" style=\"{formatBuilder.ToString()}\">{cell.GetFormattedString()}</td>");
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
                        }
                        sb.AppendLine("</table>");
                    }
                    sb.AppendLine("</body></html>");
                    
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
            

            var renderer = new ChromePdfRenderer();
            using (var pdfDocument = renderer.RenderHtmlAsPdf(sb.ToString()))
            {
                using (var memoryStream = new MemoryStream())
                {
                    pdfDocument.Stream.CopyTo(memoryStream);

                    var bytes = memoryStream.ToArray();
                    var pdfDocFromStream = new PdfDocument(bytes);
                    return pdfDocFromStream;
                }
            }

        }

        private string GetCSSClasses()
        {
            return "<html><head><style>" +
                        ".page-break { page-break-before:always; } " +
                        ".center {\r\n  text-align: center;\r\n } " +
                        ".left {\r\n  text-align: left;\r\n } " +
                        ".right {\r\n  text-align: right;\r\n } " +
                        "</style></head><body>";
        }

//        private StringBuilder GetFormat(IXLStyle cellStyle, bool isXlsx, HSSFWorkbook workbook)
        private StringBuilder GetFormat(IXLStyle cellStyle, bool isXlsx, XLWorkbook workbook)
        {
            var formatBuilder = new StringBuilder();
            if (cellStyle.Border.TopBorder != XLBorderStyleValues.None)
            {
                formatBuilder.Append("border-top: 1px solid black;");
            }
            if (cellStyle.Border.LeftBorder != XLBorderStyleValues.None)
            {
                formatBuilder.Append("border-left: 1px solid black;");
            }
            if (cellStyle.Border.RightBorder != XLBorderStyleValues.None)
            {
                formatBuilder.Append("border-right: 1px solid black;");
            }
            if (cellStyle.Border.BottomBorder != XLBorderStyleValues.None)
            {
                formatBuilder.Append("border-bottom: 1px solid black;");
            }
            if (cellStyle.Fill != null && cellStyle.Fill.PatternType != null && cellStyle.Fill.PatternType != XLFillPatternValues.None)
            {
                var color = cellStyle.Fill.BackgroundColor;

                if (!color.Color.Name.Equals("Transparent", StringComparison.InvariantCultureIgnoreCase))
                {
                    formatBuilder.Append($"background-color: #{ConvertColorToHexString(color)};");
                }
            }
            if (cellStyle.Alignment.WrapText)
            {
                formatBuilder.Append($"text-wrap: wrap;");
            }
            else
            {
                formatBuilder.Append($"text-wrap: nowrap;");
            }

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
                fontColor = ConvertColorToHexString(font.FontColor);
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
            }
            fontSize = font.FontSize;

            if (isBold)
            {
                formatBuilder.Append("font-weight: bold;");
            }
            if (isStrikeout || isUnderline)
            {
                var strikeout = isStrikeout ? "line-through" : "";
                var underline = (isUnderline) ? "underline" : "";
                formatBuilder.Append($"text-decoration: {strikeout}{underline};");
            }
            if (isItalic)
            {
                formatBuilder.Append("font-style: italic;");
            }

            formatBuilder.Append($"color: #{fontColor};");
            formatBuilder.Append($"font-size: {fontSize}px;");

            return formatBuilder;
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
