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
            var examplePath1 = ".\\example_email_to_be_concatenated.msg";

            // example 2 ... contains embedded message files
            var examplePath2 = ".\\mixed_message_example.msg";

            // example 3 ... contains embedded excel file
            var examplePath3 = ".\\email_test_with_single_sheet_excel_html.msg";

            // example 4 ... contains gif, jpg, png, tif image types as attachments
            var examplePath4 = ".\\Images\\four_image_types.msg";

            //var filesToMerge = new List<Storage.Attachment>();
            var filesToMerge = new List<PdfDocument>();

            using (var msg = new Storage.Message(examplePath4))
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
                            throw new Exception("We can't handle that data type without purchasing IronXL :(");
                        } else if (storageAttachment.FileName.ToLower().EndsWith(".html") ||
                            storageAttachment.FileName.ToLower().EndsWith(".htm"))
                        {
                            //var htmlRenderer = new ChromePdfRenderer();
                            //var document = renderer.RenderHtmlFileAsPdf(@"C:\Users\Administrator\Desktop\Inventory.html");
                            //var mainTextAsPdf = CreateStreamedPdfFromText(storageAttachment.Data);

                            //var document = renderer.RenderHtmlFileAsPdf(@"C:\Users\hooverrl\Desktop\NCI\nciitrc_eGrants\EmailHandling\EmailConcatenationPOC\EmailConcatenationPOC\bin\Debug\simple_excel_test_doc.htm");
                            //document.SaveAs("tempDirect.pdf");

                            // this just creates a blank PDF :(
                            using (var memoryStream = new MemoryStream(storageAttachment.Data))
                            {
                                var htmlRenderer = new ChromePdfRenderer();
                                //var document = htmlRenderer.RenderHtmlFileAsPdf(@"C:\Users\Administrator\Desktop\Inventory.html");
                                //var document = htmlRenderer.Render(memoryStream);
                                //var newPdfFile = new PdfDocument(memoryStream);

                                using (StreamReader reader = new StreamReader(memoryStream))
                                {
                                    string htmlContent = reader.ReadToEnd();

                                    // Render the HTML content as a PDF
                                    PdfDocument pdf = renderer.RenderHtmlAsPdf(htmlContent);
                                    //pdf.SaveAs("temp.pdf");
                                    Console.WriteLine("adding rendered pdf");
                                    filesToMerge.Add(pdf);
                                    // Save teh PDF to a file or further process it as needed
                                    //pdf.SaveAs("output.pdf");
                                }
                                //filesToMerge.Add(newPdfFile);
                            }

                            //filesToMerge.Add(document);
                        } else if (storageAttachment.FileName.ToLower().Contains(".") &&
                            supportedImageTypes.Any( sit => storageAttachment.FileName.ToLower().Contains(sit)))
                        {
                            // Found an image with a supported type.
                            // Convert an image to a PDF
                            //PdfDocument pdf = ImageToPdfConverter.ImageToPdf(imagePath);
                            //PdfDocument pdf = ImageToPdfConverter.ImageToPdf(storageAttachment.Data);

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

                            // Export the PDF
                            //pdf.SaveAs("imageToPdf.pdf");
                        } else
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
