using EmailConcatenation.Interfaces;
using IronPdf;

using MsgReader.Mime;
using MsgReader.Outlook;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EmailConcatenation.Converters
{
    internal class EmailTextConverter : IEmailTextConverter, IConvertToPdf
    {
        public bool SupportsThisFileType(string fileName)
        {
            // MLH : we shouldn't need this check much because these kinds of files are picked up outside of the regular attachment process

            if (fileName.ToLower().Contains(".") && fileName.ToLower().Contains(".msg"))
                return true;
            return false;
        }

        static string RemoveImagesFromHtmlText(string content)
        {
            if (string.IsNullOrWhiteSpace(content) ||
                !content.Contains("<html"))
            {
                return content;
            }

            string output = Regex.Replace(content, @"<img(.)*?>", " ");

            return output;
        }

        public List<PdfDocument> ToPdfDocument(ContentForPdf content)
        {
            Console.WriteLine("Handling email message case ...");

            if (content.Type != ContentForPdf.ContentType.MailMessage)
                throw new Exception("Converted didn't see the expected type for this conversion. Make sure you set the MailMessage.");

            var renderer = new ChromePdfRenderer();

            var messageTextAsHtml = content.Message.BodyText;
            var messageHtmlAsHtml = content.Message.BodyHtml;

            var htmlWithMeta = InsertEmailMeta(messageHtmlAsHtml, content.Message);

            var imagesRemovedContent = RemoveImagesFromHtmlText(htmlWithMeta);

            using (var pdfDocument = renderer.RenderHtmlAsPdf(imagesRemovedContent))
            {
                using (var memoryStream = new MemoryStream())
                {
                    pdfDocument.Stream.CopyTo(memoryStream);

                    var bytes = memoryStream.ToArray();
                    var pdfDocFromStream = new PdfDocument(bytes);
                    return new List<PdfDocument> { pdfDocFromStream };
                }
            }
        }

        private string InsertEmailMeta(string messageHtmlAsHtml, MsgReader.Outlook.Storage.Message message)
        {
            string pattern1 = "<div class=\"WordSection1\">";
            string pattern2 = "<body(.)*?>";

            string pattern = pattern1;      // default

            bool placeAtAbsoluteBeginning = false;      // for non html cases

            // check for div section
            // Create a Regex
            Regex rgFormatCheck = new Regex(pattern);
            var matches = rgFormatCheck.Matches(messageHtmlAsHtml);
            if (matches.Count == 0) 
            {
                // no div
                Regex rgFormatCheck2 = new Regex("<body(.)*?>");
                if (rgFormatCheck2.IsMatch(messageHtmlAsHtml))
                {
                    pattern = pattern2;
                } else
                {
                    placeAtAbsoluteBeginning = true;
                }
            }

            string inboundStartTag = String.Empty;

            // extract the start tag
            if (!placeAtAbsoluteBeginning)
            {
                Regex rg = new Regex(pattern);
                
                // Get all matches, should only be one
                MatchCollection matchedHeadTags = rg.Matches(messageHtmlAsHtml);
                for (int count = 0; count < matchedHeadTags.Count; count++)
                {
                    inboundStartTag = matchedHeadTags[count].Value;
                }
            }

            if (!placeAtAbsoluteBeginning)
            {
                // create div containing email meta (To, From, Title, Date Sent)
                var sb = new StringBuilder();
                sb.Append("<div>");
                sb.Append($"<p>To : { GetFormattedReceiptientsText(message.Recipients)}</p>");
                sb.Append($"<p>From : {message.Sender.Email}</p>");
                sb.Append($"<p>Subject : {message.Subject}</p>");
                sb.Append($"<p>Date Sent : {message.SentOn}</p>");
                sb.Append($"<hr style=\"width:50%;text-align:left;margin-left:0\">");
                sb.Append("</div><br/>");

                string OldBodyTagAndMeta = inboundStartTag + sb.ToString();
                string BodyWithMeta = Regex.Replace(messageHtmlAsHtml, pattern, OldBodyTagAndMeta);

                // we should be able to inocuously (i.e. consistently) insert the meta stuff here

                return BodyWithMeta;
            } else
            {
                // create unstructured content containing email meta (To, From, Title, Date Sent)
                var sb = new StringBuilder();
                sb.Append("\r\n");
                sb.Append($"To : { GetFormattedReceiptientsText(message.Recipients)}\r\n");
                sb.Append($"From : {message.Sender.Email}\r\n");
                sb.Append($"Subject : {message.Subject}\r\n");
                sb.Append($"Date Sent : {message.SentOn}\r\n");
                sb.Append("\r\n");

                string ContentWithMeta = sb.ToString() + messageHtmlAsHtml;
                return ContentWithMeta;
            }
        }

        private string GetFormattedReceiptientsText(List<Storage.Recipient> recipients)
        {
            var recipientsList = new List<String>();

            foreach(var recipient in recipients)
            {
                recipientsList.Add($"{recipient.DisplayName} ({recipient.Email})");
            }

            return string.Join(", ", recipientsList.ToArray());
        }
    }
}
