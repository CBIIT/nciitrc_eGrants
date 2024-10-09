using EmailConcatenationPOC.Converters;
using EmailConcatenationPOC.Interfaces;
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

namespace EmailConcatenationPOC
{
    public class App
    {
        public IGeneralImageConverter generalImageConverter;
        public ITIFFConverter TIFFConverter;
        public IFormattedTextConverter formattedTextConverter;
        public IUndiscoveredTextConverter undiscoveredTextConverter;
        public IExplicitlyUnsupportedTextConverter explicitlyUnsupportedTextConverter;
        public IUnrecognizedTextConverter unrecognizedTextConverter;
        public IWordConverter wordConverter;
        public IExcelConverter excelConverter;
        public IExcelXLSMConverter excelXLSMConverter;
        public IHtmlConverter htmlConverter;
        public IPDFConverter PDFConverter;
        public IRTFConverter RTFConverter;
        public IEmailTextConverter EmailTextConverter;

        public List<IConvertToPdf> orderedListOfPdfConverters;

        public App(IGeneralImageConverter _generalImageConverter, ITIFFConverter _tiffConverter, IFormattedTextConverter _formattedTextConverter,
            IUndiscoveredTextConverter _undiscoveredTextConverter, IExplicitlyUnsupportedTextConverter _explicitlyUnsupportedTextConverter,
            IUnrecognizedTextConverter _unrecognizedTextConverter, IExcelConverter _excelConverter, IWordConverter _wordConverter,
            IHtmlConverter _htmlConverter, IPDFConverter _pDFConverter, IRTFConverter _rtfConverter, IEmailTextConverter _emailTextConverter,
            IExcelXLSMConverter _excelXLSMConverter)
        {
            generalImageConverter = _generalImageConverter;
            TIFFConverter = _tiffConverter;
            formattedTextConverter = _formattedTextConverter;
            undiscoveredTextConverter = _undiscoveredTextConverter;
            explicitlyUnsupportedTextConverter = _explicitlyUnsupportedTextConverter;
            unrecognizedTextConverter = _unrecognizedTextConverter;
            excelConverter = _excelConverter;
            wordConverter = _wordConverter;
            htmlConverter = _htmlConverter;
            PDFConverter = _pDFConverter;
            RTFConverter = _rtfConverter;
            EmailTextConverter = _emailTextConverter;
            excelXLSMConverter = _excelXLSMConverter;

            orderedListOfPdfConverters = new List<IConvertToPdf> { EmailTextConverter, PDFConverter, wordConverter, excelConverter, excelXLSMConverter,
                htmlConverter, formattedTextConverter, RTFConverter, generalImageConverter, TIFFConverter, undiscoveredTextConverter,
                explicitlyUnsupportedTextConverter, unrecognizedTextConverter};
        }

        public void Run()
        {
            Console.WriteLine("Looking for Outlook files with multiple attachments ...");

            IronPdf.License.LicenseKey = "IRONPDF.NATIONALINSTITUTESOFHEALTH.IRO240906.3804.91129-DA12E4CBF3-DBQNBY5HLE5VALY-Q5R6HRQZIG3H-QKT3YRHJTBUH-PNRD6KJMHI5C-G7MCDB5LXYT3-Y5V5MI-LNUL6X3VZT6VUA-IRONPDF.DOTNET.PLUS.5YR-P3KMXU.RENEW.SUPPORT.05.SEP.2029";

            // Disable local disk access or cross-origin requests
            Installation.EnableWebSecurity = true;

            var filesToMerge = new List<PdfDocument>();

            using (var msg = new Storage.Message(Constants.ExamplePath16))
            {
                // make the first instance of FilesToMerge the original email message.
                Console.WriteLine($"Main email message body text: {msg.BodyText}");
                Console.WriteLine($"Main email message body html: {msg.BodyHtml}");
                var mainTextAsHtml = msg.BodyHtml;
                var mainEmailContent = new ContentForPdf
                {
                    Message = msg
                };
                var mainEmailPdf = EmailTextConverter.ToPdfDocument(mainEmailContent);              // Top level email content
                filesToMerge.Add(mainEmailPdf);

                var attachments = msg.Attachments;
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

                        bool fileHandled = false;
                        foreach (var converter in orderedListOfPdfConverters)
                        {                            
                            if (converter.SupportsThisFileType(storageAttachment.FileName))
                            {
                                var pdfDoc = converter.ToPdfDocument(content);
                                filesToMerge.Add(pdfDoc);
                                fileHandled = true;
                                break;
                            }
                        }
                        if (!fileHandled)
                            throw new ArgumentOutOfRangeException($"Failed to find an event handler for attached file '{storageAttachment.FileName}'. Worst case is the unrecognized converter should at least do something and mark it handled.");

                    }
                    else if (attachment is Storage.Message messageAttachment)                                               // email message
                    {
                        var plainTextContent = new ContentForPdf
                        {
                            Message = messageAttachment
                        };
                        var messagePdf = EmailTextConverter.ToPdfDocument(plainTextContent);
                        filesToMerge.Add(messagePdf);
                    }
                    else
                    {
                        var content = new ContentForPdf                                                                     // unprocessable
                        {
                            SimpleMessage = "This file was not recognized as either an email message nor as an attachment file."
                        };
                        var unrecogniedPDF = unrecognizedTextConverter.ToPdfDocument(content);
                        filesToMerge.Add(unrecogniedPDF);
                    }
                }
            }

            if (filesToMerge.Count > 0)
            {
                var merged = PdfDocument.Merge(filesToMerge);
                merged.SaveAs("result.pdf");
            }
            else
            {
                Console.WriteLine("Didn't find anything to write out. Are you sure you had something here ?");
            }

            Console.WriteLine("Attachments processed.");
            Console.ReadLine();
        }
    }
}
