using IronPdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MsgReader.Outlook;
using System.IO;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Imaging;
using BitMiracle.LibTiff.Classic;
using Markdig;

//using Iron

namespace EmailConcatenationPOC
{
    internal class Program
    {
        static readonly List<string> supportedImageTypes = new List<string> { ".jpg", ".jpeg", ".png", ".gif", ".tif" };

        static PdfDocument CreateStreamedPdfFromText(string content)
        {
            Console.WriteLine($"Here's the message text: {content}");

            var renderer = new ChromePdfRenderer();

            var imagesRemovedContent = RemoveImagesFromHtmlText(content);

            using (var pdfDocument = renderer.RenderHtmlAsPdf(imagesRemovedContent))
            {
                using (var memoryStream = new MemoryStream())
                {
                    //var newPdfFile = new PdfDocument(memoryStream);
                    //pdfDocument.SaveAs(memoryStream);
                    pdfDocument.Stream.CopyTo(memoryStream);
                    // filesToMerge.Add(pdfDocument);   // contains nothing ?

                    var bytes = memoryStream.ToArray();
                    var pdfDocFromStream = new PdfDocument(bytes);
                    return pdfDocFromStream;
                }
            }
        }

        static string RemoveImagesFromHtmlText(string content)
        {
            if (string.IsNullOrWhiteSpace(content) ||
                !content.Contains("<html")) {
                return content;
            }

            string output  = Regex.Replace(content, @"<img(.)*?>", " ");
            //sb_trim becomes "John Smith,100,000.00,M"

            return output;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Looking for Outlook files with multiple attachments ...");

            IronPdf.License.LicenseKey = "IRONPDF.NATIONALINSTITUTESOFHEALTH.IRO240906.3804.91129-DA12E4CBF3-DBQNBY5HLE5VALY-Q5R6HRQZIG3H-QKT3YRHJTBUH-PNRD6KJMHI5C-G7MCDB5LXYT3-Y5V5MI-LNUL6X3VZT6VUA-IRONPDF.DOTNET.PLUS.5YR-P3KMXU.RENEW.SUPPORT.05.SEP.2029";

            // Disable local disk access or cross-origin requests
            Installation.EnableWebSecurity = true;

            // Instantiate Renderer
            var renderer = new ChromePdfRenderer();


            // example 1 ... no embedded message files
            var examplePath1 = ".\\Outlook\\example_email_to_be_concatenated.msg";

            // example 2 ... contains embedded message files
            var examplePath2 = ".\\Outlook\\mixed_message_example.msg";

            // example 3 ... contains embedded excel file
            var examplePath3 = ".\\Outlook\\email_test_with_single_sheet_excel_html.msg";

            // example 4 ... contains gif, jpg, png, tif image types as attachments
            var examplePath4 = ".\\Images\\four_image_types.msg";

            // example 5 ... simple text
            var examplePath5 = ".\\Text\\simple_text_attach.msg";

            // example 6 ... all (non-RTF) text types
            var examplePath6 = ".\\Text\\all_simple_text_types.msg";

            // example 7 ... RTF 1 (from Word) (has weird stuff at the beginning and a TON at the end ... (renders the junk in Acrobat and Chrome))
            var examplePath7 = ".\\Text\\RTF_email_attachment_example.msg";

            // example 8 ... RTF 2 (from Word Pad, PDF converion looks great)
            var examplePath8 = ".\\Text\\RTF_file_from_wordpad.msg";

            // example 9 ... RTF 3 (from Google)
            var examplePath9 = ".\\Text\\rtf_from_google_docs.msg";

            // example 10 ... mixed security (cheesecake pdf)
            var examplePath10 = ".\\Secure\\attachments_mixed_security.msg";
            //IronSoftware.Exceptions.IronSoftwareNativeException: 'Error while opening document from bytes:
            //'Error while opening document from 132497 bytes: Invalid password'.

            var examplePath11 = ".\\Fillable\\fillable_form.msg";

            var filesToMerge = new List<PdfDocument>();

            using (var msg = new Storage.Message(examplePath3))
            {
                // make the first instance of FilesToMerge the original email message.
                Console.WriteLine($"Main email message body text: {msg.BodyText}");
                Console.WriteLine($"Main email message body html: {msg.BodyHtml}");
                var mainTextAsHtml = msg.BodyHtml;
                var mainTextAsPdf = CreateStreamedPdfFromText(mainTextAsHtml);
                filesToMerge.Add(mainTextAsPdf);

                var attachments = msg.Attachments;
                int count = 0;
                foreach(var attachment in attachments)
                {
                    count++;
                    Console.WriteLine($"Handling attachment # {count}");

                    if (attachment is Storage.Attachment storageAttachment)
                    {
                        Console.WriteLine($"Storage Attachment filename : {storageAttachment.FileName}");
                        if (storageAttachment.FileName.ToLower().EndsWith(".pdf"))
                        {
                            using (var memoryStream = new MemoryStream(storageAttachment.Data))
                            {
                                var newPdfFile = new PdfDocument(memoryStream);
                                filesToMerge.Add(newPdfFile);
                            }
                        }
                        else if (storageAttachment.FileName.ToLower().EndsWith(".docx"))
                        {
                            DocxToPdfRenderer docXRenderer = new DocxToPdfRenderer();
                            PdfDocument pdfDocument = docXRenderer.RenderDocxAsPdf(storageAttachment.Data);
                            filesToMerge.Add(pdfDocument);
                        }
                        else if (storageAttachment.FileName.ToLower().EndsWith(".xslx"))
                        {
                            throw new Exception("We can't directly handle that data type without purchasing IronXL. Save a single sheet Web (htm).");
                        }
                        else if (storageAttachment.FileName.ToLower().EndsWith(".html") ||
                            storageAttachment.FileName.ToLower().EndsWith(".htm"))
                        {
                            using (var memoryStream = new MemoryStream(storageAttachment.Data))
                            {
                                var htmlRenderer = new ChromePdfRenderer();
                                using (StreamReader reader = new StreamReader(memoryStream))
                                {
                                    string htmlContent = reader.ReadToEnd();

                                    // Render the HTML content as a PDF
                                    PdfDocument pdf = renderer.RenderHtmlAsPdf(htmlContent);
                                    Console.WriteLine("adding rendered pdf");
                                    filesToMerge.Add(pdf);
                                }
                            }
                        }
                        else if (storageAttachment.FileName.ToLower().EndsWith(".txt") ||
                            storageAttachment.FileName.ToLower().EndsWith(".log") ||
                            storageAttachment.FileName.ToLower().EndsWith(".dot"))
                        {
                            var textContent = System.Text.Encoding.Default.GetString(storageAttachment.Data);
                            
                            // MLH : there is likely going to be some "markdown" here (e.g. \n to mean new line)
                            // and this won't be natively rendered by the IronPDF chrome HTML converter
                            textContent = Markdown.ToHtml(textContent);

                            var messageAsPdf = CreateStreamedPdfFromText(textContent);  // uses HTML 
                            filesToMerge.Add(messageAsPdf);

                        }
                        else if (storageAttachment.FileName.ToLower().EndsWith(".rtf"))
                        {
                            using (var memoryStream = new MemoryStream(storageAttachment.Data))
                            {
                                var htmlRenderer = new ChromePdfRenderer();
                                using (StreamReader reader = new StreamReader(memoryStream))
                                {
                                    string rtfContent = reader.ReadToEnd();

                                    // Render the HTML content as a PDF
//                                    PdfDocument pdf = htmlRenderer.RenderRtfFileAsPdf(rtfContent);
                                    PdfDocument pdf = htmlRenderer.RenderRtfStringAsPdf(rtfContent);
                                    Console.WriteLine("adding rendered pdf");

                                    // a lot of JUNK at the beginning that starts ... {\*\xmlnstbl {\xmlns1 http://schemas.microsoft.com/office/word/2003/wordml}}\paperw12240\
                                    //      paperh15840\margl1440\margr1440\margt1440\margb1440\gutter0\ltrsect 
                                    // a LOT of JUNK at the end that starts ... \par }{\*\themedata 504b03041400
                                    filesToMerge.Add(pdf);
                                }
                            }
                        }
                        else if (storageAttachment.FileName.ToLower().Contains(".") &&
                            supportedImageTypes.Any( sit => storageAttachment.FileName.ToLower().Contains(sit)))
                        {
                            // Found an image with a supported type.
                            // Convert an image to a PDF
                            var fileNameTokens = storageAttachment.FileName.ToLower().Split('.');
                            var fileNameExtension = fileNameTokens[fileNameTokens.Length - 1].ToLower();  // the file extension is the last one
                            Console.WriteLine($"Filename extension : '{fileNameExtension}'");

                            if (fileNameExtension.Equals("tif", StringComparison.InvariantCultureIgnoreCase))
                            {
                                Console.WriteLine("Handling special case .tif file ...");
                                using (var tiffStream = new MemoryStream(storageAttachment.Data))
                                {
                                    Bitmap tiffBitmap = new Bitmap(tiffStream);
                                    using (var pngStream = new MemoryStream())
                                    {
                                        tiffBitmap.Save(pngStream, ImageFormat.Png);
                                        pngStream.Position = 0;

                                        var chromeRenderer = new ChromePdfRenderer();
                                        string base64Image = Convert.ToBase64String(pngStream.ToArray());
                                        // should end up looking like this : <img src='data:image/png;base64,... [long number of characters] />' />";
                                        string htmlString = $"<img src='data:image/{fileNameExtension};base64,{base64Image}' />";
                                        var pdfDoc = chromeRenderer.RenderHtmlAsPdf(htmlString);
                                        filesToMerge.Add(pdfDoc);
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("Handling general image case ...");
                                using (var imageStream = new MemoryStream(storageAttachment.Data))
                                {
                                    Bitmap bitmap = new Bitmap(imageStream);
                                    var chromeRenderer = new ChromePdfRenderer();
                                    string base64Image = Convert.ToBase64String((byte[])new ImageConverter().ConvertTo(bitmap, typeof(byte[])));
                                    // should end up looking like this : <img src='data:image/png;base64,... [long number of characters] />' />";
                                    string htmlString = $"<img src='data:image/{fileNameExtension};base64,{base64Image}' />";
                                    var pdfDoc = chromeRenderer.RenderHtmlAsPdf(htmlString);
                                    filesToMerge.Add(pdfDoc);

                                    Console.WriteLine($"fileNameExtension : {fileNameExtension}");
                                }
                            }
                        }
                        else
                        {
                            throw new BadImageFormatException($"The file extension for {storageAttachment.FileName} is not recognized.");
                        }
                    }
                    else if (attachment is Storage.Message messageAttachment)
                    {
                        Console.WriteLine($"Storage Message filename : {messageAttachment.FileName}");
                        var messageTextAsHtml = messageAttachment.BodyText;
                        var messageHtmlAsHtml = messageAttachment.BodyHtml;
                        var messageAsPdf = CreateStreamedPdfFromText(messageHtmlAsHtml);
                        filesToMerge.Add(messageAsPdf);
                    }
                }
            }

            if (filesToMerge.Count > 0)
            {
                var merged = PdfDocument.Merge(filesToMerge);
                merged.SaveAs("result.pdf");
            } else
            {
                Console.WriteLine("Didn't find anything to write out. Are you sure you had something here ?");
            }

            Console.WriteLine("Attachments processed.");
            Console.ReadLine();
        }
    }
}
