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

//using Iron

namespace EmailConcatenationPOC
{
    internal class Program
    {
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

            //var filesToMerge = new List<Storage.Attachment>();
            var filesToMerge = new List<PdfDocument>();

            using (var msg = new Storage.Message(examplePath2))
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
