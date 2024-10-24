using MsgReader.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailConcatenation
{
    public class ContentForPdf
    {
        public bool IsMessage { get { return Type == ContentType.MailMessage; } }
        public bool IsAttachment { get { return Type == ContentType.DataAttachment; } }
        public bool IsSimpleMessage { get { return Type == ContentType.SimpleGeneratedMessage; } }

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
    }
}
