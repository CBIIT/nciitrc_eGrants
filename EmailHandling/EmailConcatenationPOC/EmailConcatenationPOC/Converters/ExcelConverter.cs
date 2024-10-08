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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using static System.Net.WebRequestMethods;

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

            // TODO : add check for .xls (and NOT xlsx) here
            // TODO : add mult-sheet support
            // TODO : add formatting support
            //            using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(fileName, false))
            //Stream stream = new MemoryStream();

            // Warning : throws NPOI.HSSF.OldExcelFormatException : 'The supplied spreadsheet seems to be Excel 5.0/7.0 (BIFF5) format. POI only supports
            // BIFF8 format (from Excel versions 97/2000/XP/2003)'


            var sb = new StringBuilder();
            

            //            using (var memoryStream = new MemoryStream(content.Attachment.Data))
            // var tempFilePath = ".\\Excel\\simple_xsl.xls";
            //var tempFilePath = ".\\Excel\\simple_xls.xls";
            //var tempFilePathOld = ".\\Excel\\simple_xls_old.xls";
            //var tempFilePathNew

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
                        sb.AppendLine("<html><body><style>" +
                            ".page-break { page-break-before:always; }" +
                            ".center {\r\n  text-align: center;\r\n }" +
                            ".left {\r\n  text-align: left;\r\n }" +
                            ".right {\r\n  text-align: right;\r\n }" +
                            "</style>");
                        XSSFWorkbook workbook = new XSSFWorkbook(memoryStream);

                        for (int i = 0; i < workbook.NumberOfSheets; i++)
                        {
                            if (i > 0)
                            {
                                sb.Append("<div class=\"page-break\"></div>");
                            }

                            //var sheet = workbook.GetSheetAt(0);             // TODO : support multiple sheets
                            var sheet = workbook.GetSheetAt(i);             // TODO : support multiple sheets
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
                                            string formatString = String.Empty;
                                            ICellStyle cellStyle = cell.CellStyle;
                                            StringBuilder formatBuilder = new StringBuilder();
                                            var classes = new List<string>();

                                            // MLH : is there a way to handle cellStyle.WrapText == false ?
                                            // floating divs maybe ? sounds risky

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
                                                formatBuilder.Append($"background-color: #{BitConverter.ToString(fgColor).Replace("-", "")};");
                                                //formatBuilder.Append($"#{BitConverter.ToString(fgColor[0])}{fgColor[1]}{fgColor[2]}");
                                            }
                                            if (cellStyle.Alignment == HorizontalAlignment.Left)
                                            {
                                                classes.Add("left");
                                            } else if (cellStyle.Alignment == HorizontalAlignment.Right)
                                            {
                                                classes.Add("right");
                                            }
                                            if (cellStyle.Alignment == HorizontalAlignment.Center)
                                            {
                                                classes.Add("center");
                                            }
                                            var contentText = cell.ToString();
                                            if (contentText == "Red text")
                                            {
                                                Console.WriteLine("ahoy !");
                                            }
                                            XSSFCellStyle src = (XSSFCellStyle)cellStyle;
                                            if (src != null)
                                            {
                                                var font = src.GetFont();
                                                if (font.IsBold)
                                                {
                                                    formatBuilder.Append("font-weight: bold;");
                                                }
                                                if (font.IsStrikeout || font.IsItalic || (font.Underline != FontUnderlineType.None))
                                                {
                                                    var strikeout = font.IsStrikeout ? "line-through" : "";
                                                    var italic = font.IsItalic ? "line-through" : "";
                                                    var underline = (font.Underline != FontUnderlineType.None) ? "underline" : "";
                                                    formatBuilder.Append($"text-decoration: {strikeout}{italic}{underline};");
                                                    // css : underline (but where is this in NPOI ?)
                                                }
                                                //var color = font.Color;
                                                //formatBuilder.Append($"color: {font.Color}px;");      // always 0
                                                //formatBuilder.Append($"color: {src.FillForegroundColor}px;");
                                                var fontColor = font.GetXSSFColor().RGB;
                                                formatBuilder.Append($"color: #{BitConverter.ToString(fontColor).Replace("-", "")};");
                                                formatBuilder.Append($"font-size: {font.FontHeightInPoints}px;");
                                            }
                                            //formatBuilder.Append($"color: {font.Color}px;");      // always 0
                                            //formatBuilder.Append($"color: {src.FillForegroundColor}px;");
                                            //formatBuilder.Append($"color: {src.GetXSFColor()}px;");

                                            //                                        cell.Font
                                            //cellStyle.GetFont()
                                            //if (cellStyle.GetFont())
                                            IDataFormat dataFormat = workbook.CreateDataFormat();
                                            formatString = dataFormat.GetFormat(cellStyle.DataFormat);
                                            sb.Append($"<td class=\"{string.Join(";",classes)}\" style=\"{formatBuilder.ToString()}\">{cell.ToString()}</td>");
                                        }
                                    }
                                    sb.Append("</tr>");
                                }
                            }
                            sb.AppendLine("</table></body>");
                        }
                        sb.AppendLine("</html>");
                    }
                    else
                    {
                        
                        HSSFWorkbook workbook = new HSSFWorkbook(memoryStream);
                        var sheet = workbook.GetSheetAt(0);             // TODO : support multiple sheets
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
                                        sb.Append($"<td>{cell.ToString()}</td>");
                                        Console.Write(cell.ToString());
                                    }
                                }
                                sb.Append("</tr>");
                            }
                        }
                        sb.AppendLine("</table></body></html>");
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


            // Extract the attachment
            //var attachment = msg.Attachments;

            // Load the MSG file
            //using (var msg = new Storage.Message("path_to_msg_file.msg"))
            //{
            // Extract the attachment
            //var attachment = content.Attachment;
            //if (attachment != null)
            //{
            //    using (var memoryStream = new MemoryStream(attachment.Data))
            //    {
            //        using (var workbook = new XLWorkbook(memoryStream))
            //        {
            //            ConvertXlsxToPdf(workbook, "tempname.pdf");
            //        }
            //    }
            //}
        }


        //static void ReadExcelFileDOM(string fileName)
        //{
        //    using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(fileName, false))
        //    {

        //        WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart ?? spreadsheetDocument.AddWorkbookPart();
        //        WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
        //        SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();
        //        //string? text;
        //        string text;

        //        foreach (Row r in sheetData.Elements<Row>())
        //        {
        //            foreach (Cell c in r.Elements<Cell>())
        //            {
        //                text = c?.CellValue?.Text;
        //                Console.Write(text + " ");
        //            }
        //        }


        //        Console.WriteLine();
        //        Console.ReadKey();
        //    }
        //}

        //static void OpenAndAddToSpreadsheetStream(Stream stream)
        //{
        //    // Open a SpreadsheetDocument based on a stream.
        //    SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(stream, true);

        //    if (spreadsheetDocument != null)
        //    {
        //        // Get or create the WorkbookPart
        //        WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart ?? spreadsheetDocument.AddWorkbookPart();


        //        // Add a new worksheet.
        //        WorksheetPart newWorksheetPart = workbookPart.AddNewPart<WorksheetPart>();
        //        newWorksheetPart.Worksheet = new Worksheet(new SheetData());
        //        newWorksheetPart.Worksheet.Save();


        //        Workbook workbook = workbookPart.Workbook ?? new Workbook();

        //        if (workbookPart.Workbook is null)
        //        {
        //            workbookPart.Workbook = workbook;
        //        }

        //        Sheets sheets = workbook.GetFirstChild<Sheets>() ?? workbook.AppendChild(new Sheets());
        //        string relationshipId = workbookPart.GetIdOfPart(newWorksheetPart);

        //        // Get a unique ID for the new worksheet.
        //        uint sheetId = 1;

        //        if (sheets.Elements<Sheet>().Count() > 0)
        //        {
        //            sheetId = (sheets.Elements<Sheet>().Select(s => s.SheetId?.Value).Max() + 1) ?? (uint)sheets.Elements<Sheet>().Count() + 1;
        //        }

        //        // Give the new worksheet a name.
        //        string sheetName = "Sheet" + sheetId;

        //        // Append the new worksheet and associate it with the workbook.
        //        Sheet sheet = new Sheet() { Id = relationshipId, SheetId = sheetId, Name = sheetName };
        //        sheets.Append(sheet);
        //        workbookPart.Workbook.Save();

        //        // Dispose the document handle.
        //        spreadsheetDocument.Dispose();
        //    }
        //}



        // let's try converting XLWorkbook to HTML ... everything after that is a SOLVED problem
        //              plan B ... convert XLWorkbook to CSV
        // QuestPDF might be an option but it looks like you ahve to design the PDF programatically  and doesn't convert directly

        //public Stream GetStream(XLWorkbook excelWorkbook)
        //{
        //    Stream fs = new MemoryStream();
        //    excelWorkbook.SaveAs(fs);
        //    fs.Position = 0;
        //    return fs;
        //}

        //static void ConvertXlsxToPdf(XLWorkbook workbook, string outputPath)
        //{
        //    using (var pdfDocument = new IronPdf.PdfDocument())
        //    {
        //        foreach (var worksheet in workbook.Worksheets)
        //        {
        //            var pdfPage = pdfDocument.AddPage();
        //            using (var graphics = XGraphics.FromPdfPage(pdfPage))
        //            {
        //                // Render the worksheet to the PDF page
        //                RenderWorksheetToPdf(worksheet, graphics);
        //            }
        //        }

        //        pdfDocument.Save(outputPath);
        //    }
        //}

        //public PdfDocument ToPdfDocument(ContentForPdf content)
        //{
        //    Console.WriteLine("Handling Excel file type case ...");

        //    if (content.Type != ContentForPdf.ContentType.DataAttachment)
        //        throw new Exception("Converted didn't see the expected type for this conversion. Make sure you set the Attachment.");

        //    var renderer = new ChromePdfRenderer();

        //    // Extract the attachment
        //    //var attachment = msg.Attachments;

        //    // Load the MSG file
        //    //using (var msg = new Storage.Message("path_to_msg_file.msg"))
        //    //{
        //    // Extract the attachment
        //    var attachment = content.Attachment;
        //    if (attachment != null)
        //    {
        //        using (var memoryStream = new MemoryStream(attachment.Data))
        //        {
        //            using (var workbook = new XLWorkbook(memoryStream))
        //            {
        //                ConvertXlsxToPdf(workbook, "tempname.pdf");
        //            }
        //        }
        //    }
            //}

            //// Load the MSG file
            ////using (var msg = new Storage.Message("path_to_msg_file.msg"))
            ////using (var msg = new Storage.Message("path_to_msg_file.msg"))
            ////{
            //// Extract the attachment
            //// var attachment = msg.Attachments;
            //var attachment = content.Attachment;
            //using (var memoryStream = new MemoryStream())
            //{
            //    // Save the attachment to a memory stream
            //    attachment.Save(memoryStream);
            //    memoryStream.Position = 0;

            //    // Load the XLSX file from the memory stream
            //    using (var workbook = new XLWorkbook(memoryStream))
            //    {
            //        // Convert the XLSX to PDF
            //        ConvertXlsxToPdf(workbook, "output.pdf");
            //    }
            //}
            ////}


            //var sb = new StringBuilder();
            //// we need IronXL (or some open source stuff Lata is gathering feedback on)
            //sb.AppendLine("<html><body><p><h1>Explicitly Unsupported File Type</h1></p>");
            //sb.AppendLine("<p>We can't directly handle that data type without an additional framework.</p>");
            //sb.AppendLine("<p>For now you can save .doc files (old Word format) to .docx.</p>");
            //sb.AppendLine("<p>FYI : files with this type have been found to exist in the eGrants database.</p>");
            //sb.AppendLine($"<p>The offending file's name is : {content.Attachment.FileName}</p></body></html>");

            //using (var pdfDocument = renderer.RenderHtmlAsPdf(sb.ToString()))
            //{
            //    using (var memoryStream = new MemoryStream())
            //    {
            //        pdfDocument.Stream.CopyTo(memoryStream);

            //        var bytes = memoryStream.ToArray();
            //        var pdfDocFromStream = new PdfDocument(bytes);
            //        return pdfDocFromStream;
            //    }
            //}
        //}
    }
}
