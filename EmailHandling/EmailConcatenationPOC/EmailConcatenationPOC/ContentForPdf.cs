using MsgReader.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailConcatenationPOC
{
    public class ContentForPdf
    {
        public bool IsMessage { get { return Type == ContentType.MailMessage; } }
        public bool IsAttachment { get { return Type == ContentType.DataAttachment; } }
        public bool IsSimpleMessage { get { return Type == ContentType.SimpleGeneratedMessage; } }

        public Storage.Attachment Attachment {
            get { return Attachment; }
            set { Attachment = value; Type = ContentType.DataAttachment; }
        }

        public Storage.Message Message {
            get { return Message; }
            set { Message = value; Type = ContentType.MailMessage; }
        }

        public string SimpleMessage
        {
            get { return SimpleMessage; }
            set { SimpleMessage = value; Type = ContentType.SimpleGeneratedMessage; }
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
