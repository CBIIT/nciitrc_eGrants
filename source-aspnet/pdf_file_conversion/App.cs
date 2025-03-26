using EmailConcatenation;
using EmailConcatenation.Converters;
using EmailConcatenation.Interfaces;
using IronPdf;
using Markdig;
using MsgReader.Outlook;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace EmailConcatenation
{
    public class App
    {
        public IGeneralImageConverter generalImageConverter;
        public ITIFFConverter TIFFConverter;
        public IFormattedTextConverter formattedTextConverter;
        public IWordConverter wordConverter;
        public IHtmlConverter htmlConverter;
        public IPDFConverter PDFConverter;
        public IRTFConverter RTFConverter;
        public IEmailTextConverter EmailTextConverter;
        public IWordDocConverter wordDocConverter;

        public List<IConvertToPdf> orderedListOfPdfConverters;

        public App(IGeneralImageConverter _generalImageConverter, ITIFFConverter _tiffConverter, IFormattedTextConverter _formattedTextConverter, IWordConverter _wordConverter,
            IHtmlConverter _htmlConverter, IPDFConverter _pDFConverter, IRTFConverter _rtfConverter, IEmailTextConverter _emailTextConverter, IWordDocConverter _wordDocConverter)
        {
            generalImageConverter = _generalImageConverter;
            TIFFConverter = _tiffConverter;
            formattedTextConverter = _formattedTextConverter;
            wordConverter = _wordConverter;
            wordDocConverter = _wordDocConverter;
            htmlConverter = _htmlConverter;
            PDFConverter = _pDFConverter;
            RTFConverter = _rtfConverter;
            EmailTextConverter = _emailTextConverter;

            orderedListOfPdfConverters = new List<IConvertToPdf> { EmailTextConverter, PDFConverter, wordConverter, wordDocConverter,
                htmlConverter, formattedTextConverter, RTFConverter, generalImageConverter, TIFFConverter};

        }

        public PdfDocument Convert(Storage.Message incomingEmail)
        {
            Console.WriteLine("Looking for Outlook files with multiple attachments ...");

            //var watch = System.Diagnostics.Stopwatch.StartNew();
            // the code that you want to measure comes here

            IronPdf.License.LicenseKey = "IRONPDF.NATIONALINSTITUTESOFHEALTH.IRO240906.3804.91129-DA12E4CBF3-DBQNBY5HLE5VALY-Q5R6HRQZIG3H-QKT3YRHJTBUH-PNRD6KJMHI5C-G7MCDB5LXYT3-Y5V5MI-LNUL6X3VZT6VUA-IRONPDF.DOTNET.PLUS.5YR-P3KMXU.RENEW.SUPPORT.05.SEP.2029";

            // Disable local disk access or cross-origin requests
            Installation.EnableWebSecurity = true;

            var filesToMerge = new List<PdfDocument>();

            //using (var msg = new Storage.Message(Constants.ExamplePath16))
            //{
            // make the first instance of FilesToMerge the original email message.
            Console.WriteLine($"Main email message body text: {incomingEmail.BodyText}");
            Console.WriteLine($"Main email message body html: {incomingEmail.BodyHtml}");
            var mainTextAsHtml = incomingEmail.BodyHtml;
            var mainEmailContent = new ContentForPdf
            {
                Message = incomingEmail
            };
            var mainEmailPdf = EmailTextConverter.ToPdfDocument(mainEmailContent);              // Top level email content
            filesToMerge.AddRange(mainEmailPdf);

            var attachments = incomingEmail.Attachments;
            int count = 0;

            foreach (var attachment in attachments)
            {
                count++;
                Console.WriteLine($"Handling attachment # {count}");

                if (attachment is Storage.Attachment storageAttachment)
                {
                    var content = new ContentForPdf
                    {
                        Attachment = storageAttachment
                    };

                    Console.WriteLine($"Storage Attachment filename : {storageAttachment.FileName}");

                    if (mainEmailContent.EmailAttachmentFilenameSkipList.Contains( storageAttachment.FileName))
                    {
                        Console.WriteLine($"Not adding attachment {storageAttachment.FileName} here because it was embedded in the HTML formatted email.");
                        continue;
                    }

                    bool fileHandled = false;
                    foreach (var converter in orderedListOfPdfConverters)
                    {                            
                        if (converter.SupportsThisFileType(storageAttachment.FileName))
                        {
                            var pdfDoc = converter.ToPdfDocument(content);
                            if (pdfDoc != null)
                                filesToMerge.AddRange(pdfDoc);
                            fileHandled = true;
                            break;
                        }
                    }

                }
                else if (attachment is Storage.Message messageAttachment)                                               // email message
                {
                    var plainTextContent = new ContentForPdf
                    {
                        Message = messageAttachment
                    };
                    var messagePdf = EmailTextConverter.ToPdfDocument(plainTextContent);
                    if (messagePdf != null)
                        filesToMerge.AddRange(messagePdf);
                }
            }
            //}

            if (!filesToMerge.Any())
            {
                return null;
            }
            else if (filesToMerge.Count > 0)
            {
                Console.WriteLine("Attachments processed.");
                var merged = PdfDocument.Merge(filesToMerge);
                //merged.SaveAs("result.pdf");
                return merged;
            }
            else
            {
                Console.WriteLine("Didn't find anything to write out. Are you sure you had something here ?");
                return null;
            }
        }

        internal PdfDocument Convert(MemoryStream memoryStream, string fileName)
        {
            bool fileHandled = false;

            Console.WriteLine("Handling single file conversion case ...");

            IronPdf.License.LicenseKey = "IRONPDF.NATIONALINSTITUTESOFHEALTH.IRO240906.3804.91129-DA12E4CBF3-DBQNBY5HLE5VALY-Q5R6HRQZIG3H-QKT3YRHJTBUH-PNRD6KJMHI5C-G7MCDB5LXYT3-Y5V5MI-LNUL6X3VZT6VUA-IRONPDF.DOTNET.PLUS.5YR-P3KMXU.RENEW.SUPPORT.05.SEP.2029";

            // Disable local disk access or cross-origin requests
            Installation.EnableWebSecurity = true;

            var filesToMerge = new List<PdfDocument>();

            var content = new ContentForPdf
            {
                MemoryStream = memoryStream,
                Type = ContentForPdf.ContentType.DataAttachment,
                SingleFileFileName = fileName
            };

            foreach (var converter in orderedListOfPdfConverters)
            {
                if (converter.SupportsThisFileType(fileName))
                {
                    var pdfDoc = converter.ToPdfDocument(content);
                    if (pdfDoc != null)
                        filesToMerge.AddRange(pdfDoc);
                    fileHandled = true;
                    break;
                }
            }

            if (!filesToMerge.Any())
            {
                return null;
            }
            else if (filesToMerge.Count > 0)
            {
                Console.WriteLine("Attachments processed.");
                var merged = PdfDocument.Merge(filesToMerge);
                return merged;
            } else
            {
                return null;
            }
        }

        internal PdfDocument CreateEmptyDocument()
        {
            var content = "<html><body><h1>File or files contained no data.</h1></body></html>";

            // using an unusual overload, so manually instantiate the converter
            var tempHtmlConverter = new HtmlConverter();
            var result = tempHtmlConverter.ToPdfDocument(content).ElementAt(0);

            return result;
        }
    }
}
