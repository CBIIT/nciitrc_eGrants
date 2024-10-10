using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using EmailConcatenationPOC.Interfaces;
using IronPdf;
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
using System.Windows.Interop;
using static System.Net.WebRequestMethods;
using CellType = NPOI.SS.UserModel.CellType;

namespace EmailConcatenationPOC.Converters
{
    internal class ExcelConverter : IExcelConverter, IConvertToPdf
    {
        public bool SupportsThisFileType(string fileName)
        {
            if (!string.IsNullOrWhiteSpace(fileName) &&
                (fileName.ToLower().EndsWith(".xls") ||
                fileName.ToLower().EndsWith(".xlsx")))
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
                    if (content.Attachment.FileName.ToLower().EndsWith(".xlsx"))
                    {
                        sb.AppendLine(GetCSSClasses());
                        XSSFWorkbook workbook = new XSSFWorkbook(memoryStream);
                        XSSFFormulaEvaluator formula = new XSSFFormulaEvaluator(workbook);

                        for (int i = 0; i < workbook.NumberOfSheets; i++)
                        {
                            if (i > 0)
                            {
                                sb.Append("<div class=\"page-break\"></div>");
                            }

                            var sheet = workbook.GetSheetAt(i);
                            sb.AppendLine("<table>");
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
                                                Console.WriteLine("ahoy !");
                                            }

                                            StringBuilder formatBuilder = GetFormat(cellStyle, true, null);
                                            
                                            IDataFormat dataFormat = workbook.CreateDataFormat();
                                            string formatString = dataFormat.GetFormat(cellStyle.DataFormat);
                                            formula.EvaluateInCell(cell);
                                            string cellVal = GetFormattedCellValue(cell, workbook);
                                            sb.Append($"<td class=\"{string.Join(";", classes)}\" style=\"{formatBuilder.ToString()}\">{cellVal}</td>");
                                        }
                                        else
                                        {
                                            // it's null, but add placeholder table data
                                            sb.Append("<td></td>");
                                        }
                                    }
                                    sb.Append("</tr>");
                                }
                            }
                            sb.AppendLine("</table>");
                        }
                        sb.AppendLine("</body></html>");
                    }
                    else  //    .xls
                    {
                        sb.AppendLine(GetCSSClasses());
                        HSSFWorkbook workbook = new HSSFWorkbook(memoryStream);
                        HSSFFormulaEvaluator formula = new HSSFFormulaEvaluator(workbook);

                        // MLH : note that if for whatever this should happen on a cell by cell basis,
                        // use formula.EvaluateInCell(cell); and render by cell.CellType
                        formula.EvaluateAll();

                        for (int i = 0; i < workbook.NumberOfSheets; i++)
                        {
                            if (i > 0)
                            {
                                sb.Append("<div class=\"page-break\"></div>");
                            }

                            var sheet = workbook.GetSheetAt(i);             // TODO : support multiple sheets
                            sb.AppendLine("<table><body><html>");
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
                                            if (contentText.Contains("3.14"))
                                            {
                                                Console.WriteLine("ahoy !");
                                            }

                                            var formatBuilder = GetFormat(cellStyle, false, workbook);

                                            IDataFormat dataFormat = workbook.CreateDataFormat();
                                            formatString = dataFormat.GetFormat(cellStyle.DataFormat);
                                            formula.EvaluateInCell(cell);
                                            sb.Append($"<td class=\"{string.Join(";", classes)}\" style=\"{formatBuilder.ToString()}\">{cell.ToString()}</td>");
                                        } else
                                        {
                                            // it's null, but add placeholder table data
                                            sb.Append("<td></td>");
                                        }
                                    }
                                    sb.Append("</tr>");
                                }
                            }
                            sb.AppendLine("</table></html>");
                        }
                        sb.AppendLine("</body></html>");
                    }
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
                        "</style></head><body>";
        }

        private StringBuilder GetFormat(ICellStyle cellStyle, bool isXlsx, HSSFWorkbook workbook)
        {
            var formatBuilder = new StringBuilder();
            if (cellStyle.BorderTop != 0)
            {
                formatBuilder.Append("border-top: 1px solid black;");
            }
            if (cellStyle.BorderLeft != 0)
            {
                formatBuilder.Append("border-left: 1px solid black;");
            }
            if (cellStyle.BorderRight != 0)
            {
                formatBuilder.Append("border-right: 1px solid black;");
            }
            if (cellStyle.BorderBottom != 0)
            {
                formatBuilder.Append("border-bottom: 1px solid black;");
            }
            if (cellStyle.FillForegroundColorColor != null)
            {
                // this is a byte[3] with each byte correspondin to R, G, and B
                var fgColor = cellStyle.FillForegroundColorColor.RGB;

                // MLH : for some reason on XLS files, default white background is presented as 0,0,0 which becomes black
                var convertedHexColor = BitConverter.ToString(fgColor).Replace("-", "");
                if (!isXlsx && convertedHexColor.Equals("000000"))
                    convertedHexColor = "FFFFFF";

                formatBuilder.Append($"background-color: #{convertedHexColor};");
            }
            if (cellStyle.WrapText)
            {
                formatBuilder.Append($"text-wrap: wrap;");
            } else
            {
                formatBuilder.Append($"text-wrap: nowrap;");
            }

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

            formatBuilder.Append($"color: {fontColor};");
            formatBuilder.Append($"font-size: {fontSize}px;");

            return formatBuilder;
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
