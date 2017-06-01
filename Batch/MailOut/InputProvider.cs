using SBM.Common;
using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Net.Mail;
using System.Text;

namespace SBM.MailOut
{
    internal class InputProvider 
    {
        public InputProvider()
        { 
        }

        protected BatchContext Context { get; set; }

        public virtual IEnumerator GetMails()
        {
            var msg = CreateMessage();

            if (!string.IsNullOrWhiteSpace(Parameter.Source.From))
            {
                msg.From = new MailAddress(Parameter.Source.From);
            }

            foreach (var to in Parameter.Source.To.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                msg.To.Add(new MailAddress(to));
            }

            foreach (var cc in Parameter.Source.CC.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                msg.CC.Add(new MailAddress(cc));
            }

            foreach (var bcc in Parameter.Source.BCC.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                msg.Bcc.Add(new MailAddress(bcc));
            }

            foreach (var reply in Parameter.Source.Reply.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                msg.ReplyToList.Add(new MailAddress(reply));
            }

            msg.Subject = Parameter.Source.Subject.ReeplaceWithContext(Context);
            msg.Body = Parameter.Source.Body.ReeplaceWithContext(Context);

            if (!string.IsNullOrWhiteSpace(Parameter.Source.AttachFolder))
            {
                foreach (var fileName in Parameter.Source.AttachFiles.Split(new[] { ';', ','}, StringSplitOptions.RemoveEmptyEntries))
                {
                    msg.Attachments.Add(new Attachment(Path.Combine(Parameter.Source.AttachFolder.Trim(), fileName.Trim())));
                }
            }

            return new[] { new Item(msg) }.GetEnumerator();
        }

        public virtual int Update(DataRow row)
        {
            //do nothing

            return 0;
        }

        protected MailMessage CreateMessage()
        {
            return new MailMessage()
            {
                SubjectEncoding = Encoding.UTF8,
                HeadersEncoding = Encoding.UTF8,
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = true,
            };
        }

        public static InputProvider GetProvider(BatchContext context)
        {
            switch ( Parameter.Source.Source)
            {
                case SOURCE.MSSQL:
                case SOURCE.SQL:
                    return new InputSQLProvider()
                    {
                        Context = context
                    };

                case SOURCE.XML:
                    return new InputXMLProvider()
                    {
                        Context = context
                    };

                default:
                    return new InputProvider()
                    {
                        Context = context
                    };
            }
        }
    }
}
