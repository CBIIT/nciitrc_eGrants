using MsgReader.Outlook;

using Org.BouncyCastle.Utilities;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace EmailConcatenation
{
    public class ContentForPdf
    {
        public bool IsMessage { get { return Type == ContentType.MailMessage; } }
        public bool IsAttachment { get { return Type == ContentType.DataAttachment; } }
        public bool IsSimpleMessage { get { return Type == ContentType.SimpleGeneratedMessage; } }

        public bool IsMemoryStream { get; set; }

        private Storage.Attachment attachment;
        public Storage.Attachment Attachment {
            get { return attachment; }
            set { attachment = value; Type = ContentType.DataAttachment; }
        }

        private Storage.Message message;
        public Storage.Message Message {
            get { return message; }
            set { message = value; Type = ContentType.MailMessage; }
        }

        private MemoryStream memoryStream;
        public MemoryStream MemoryStream
        {
            get { return memoryStream; }
            set { memoryStream = value; IsMemoryStream = true; }
        }

        private string simpleMessage;
        public string SimpleMessage
        {
            get { return simpleMessage; }
            set { simpleMessage = value; Type = ContentType.SimpleGeneratedMessage; }
        }

        public enum ContentType
        {
            MailMessage,
            DataAttachment,
            SimpleGeneratedMessage
        }

        public ContentType Type;

        private List<string> emailAttachmentFilenameSkipList;
        public List<string> EmailAttachmentFilenameSkipList
        {
            get { return emailAttachmentFilenameSkipList; }
            set { emailAttachmentFilenameSkipList = value; }
        }

        // MLH : we can't just use the attachment.Filename for memoryStreams
        private string singleFileFileName;
        public string SingleFileFileName
        {
            get; set;
        }


        public ContentForPdf()
        {
            emailAttachmentFilenameSkipList = new List<string>();
            IsMemoryStream = false;
        }

        public byte[] GetBytes()
        {
            if (!IsMemoryStream)
            {
                return Attachment.Data;
            }
            else
            {
                return MemoryStream.ToArray();
            }
        }
    }
}
