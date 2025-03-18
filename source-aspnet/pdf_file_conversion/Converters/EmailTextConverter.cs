using EmailConcatenation.Interfaces;
using IronPdf;

using MsgReader.Mime;
using MsgReader.Outlook;

using SkiaSharp;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

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

        public List<PdfDocument> ToPdfDocument(ContentForPdf content)
        {
            Console.WriteLine("Handling email message case ...");

            if (content.Type != ContentForPdf.ContentType.MailMessage)
                throw new Exception("Converted didn't see the expected type for this conversion. Make sure you set the MailMessage.");

            if (content == null || content.Message == null || string.IsNullOrWhiteSpace(content.Message.BodyHtml))
                return null;

            var renderer = new ChromePdfRenderer();

            var messageTextAsHtml = content.Message.BodyText;
            var messageHtmlAsHtml = content.Message.BodyHtml;

            var htmlWithMeta = InsertEmailMeta(messageHtmlAsHtml, content.Message);

            var embeddedImagesAndContent = ImagesBase64Encoded(htmlWithMeta, content);

            using (var pdfDocument = renderer.RenderHtmlAsPdf(embeddedImagesAndContent))
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

        private string ImagesBase64Encoded(string content, ContentForPdf contentForPdf)
        {
            var attachments = contentForPdf.Message.Attachments;

            if (string.IsNullOrWhiteSpace(content) ||
                !content.Contains("<html"))
            {
                return content;
            }

            Regex reg = new Regex(@"<img[^>]*src=""cid:[^>]*>", RegexOptions.Singleline);      // only grab the un base64 converted images
            Regex fileFormatExtensionReg = new Regex(@"(?<=[0-9][0-9][0-9]\.).*?(?=@)", RegexOptions.Singleline);
            Regex fileNameReg = new Regex(@"(?<=cid:).*?(?=@)", RegexOptions.Singleline);
            Regex imgSrcQuotesReg = new Regex(@"(?<= src="").*?(?="")", RegexOptions.Singleline);


            while (reg.Match(content).Success) {
                var matches = reg.Matches(content);

                var match = reg.Match(content);

                var capturingText = match.Value;

                // should look like :
                // <img width="878" height="138" style="width:9.1458in;height:1.4375in" id="Picture_x0020_1" src="cid:image001.png@01DA6A57.71B669D0">

                // attachment will be named something like "image001.png"

                // extract file name
                var fileName = string.Empty;
                var fileNameResult = fileNameReg.Match(capturingText);
                if (!fileNameResult.Success)
                {
                    throw new Exception("found an image in the email, but the format didn't enable extracting the file name so it could be matched with the attachment");
                }
                fileName = fileNameResult.Value;

                // need to extract image format (here it's "png")
                var fileFormatExtension = string.Empty;
                var formatResult = fileFormatExtensionReg.Match(capturingText);
                if (!formatResult.Success)
                {
                    throw new Exception("found an image in the email, but the format didn't enable extracting the file type / extension");
                }
                fileFormatExtension = formatResult.Value;

                // get the attachment
                Storage.Attachment theAttachmentForThisPic = null;
                var attachmentFileNames = new List<string>();
                foreach (var attachment in attachments)
                {
                    if (attachment is Storage.Attachment storageAttachment)
                    {
                        attachmentFileNames.Add(storageAttachment.FileName);
                        if (storageAttachment.FileName.Equals(fileName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            theAttachmentForThisPic = storageAttachment;
                        }
                    }
                }
                if (theAttachmentForThisPic == null)
                {
                    throw new Exception($"didn't find a match for embedded img with filename {fileName} among attachments {string.Join(" ", attachmentFileNames.ToArray())}");
                }
                contentForPdf.EmailAttachmentFilenameSkipList.Add(fileName);

                // convert attachment to base64 encoded string
                byte[] imageBytes = theAttachmentForThisPic.Data;
                string base64Image = Convert.ToBase64String(imageBytes);
                string fullSrcContent = $"data:image/{formatResult};base64,{base64Image}";
                var newImgContent = imgSrcQuotesReg.Replace(capturingText, fullSrcContent);

                content = content.Replace(capturingText, newImgContent);
            }

            return content;
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
